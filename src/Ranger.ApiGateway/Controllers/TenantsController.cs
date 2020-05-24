using System;
using System.Threading.Tasks;
using AutoWrapper.Wrappers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Ranger.Common;
using Ranger.InternalHttpClient;
using Ranger.RabbitMQ;

namespace Ranger.ApiGateway
{
    [ApiController]
    [ApiVersion("1.0")]
    public class TenantsController : BaseController<TenantsController>
    {
        private readonly TenantsHttpClient tenantsClient;
        private readonly IBusPublisher busPublisher;

        public TenantsController(TenantsHttpClient tenantsClient, IBusPublisher busPublisher, ILogger<TenantsController> logger) : base(busPublisher, logger)
        {
            this.busPublisher = busPublisher;
            this.tenantsClient = tenantsClient;

        }

        ///<summary>
        /// Deletes a tenant
        ///</summary>
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [HttpDelete("/tenants/{domain}")]
        [Authorize(Roles = "PrimaryOwner")]
        [Authorize(Policy = "TenantIdResolved")]
        public async Task<ApiResponse> DeleteTenant()
        {
            var deleteTenantMsg = new DeleteTenantSagaInitializer(UserFromClaims.Email, TenantId);
            return await Task.Run(() => Send(deleteTenantMsg));
        }

        ///<summary>
        /// Creates a new tenant
        ///</summary>
        ///<param name="tenantModel">The model necessary to create a new tenant</param>
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [HttpPost("/tenants")]
        [AllowAnonymous]
        public async Task<ApiResponse> Post(TenantModel tenantModel)
        {
            var createTenantMsg = new CreateTenant(tenantModel.DomainForm.Domain.ToLower(), tenantModel.DomainForm.OrganizationName, tenantModel.UserForm.Email, tenantModel.UserForm.FirstName, tenantModel.UserForm.LastName, tenantModel.UserForm.Password);
            return await Task.Run(() => Send(createTenantMsg));
        }

        ///<summary>
        /// Determines whether a tenant exists
        ///</summary>
        ///<param name="domain">The domain to check availability for</param>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [HttpGet("/tenants/{domain}/exists")]
        [AllowAnonymous]
        public async Task<ApiResponse> TenantExists(string domain)
        {
            var apiResponse = await tenantsClient.DoesExistAsync(domain);
            return new ApiResponse("Successfully determined tenant existence", apiResponse.Result);
        }

        ///<summary>
        /// Determines whether a tenant is confirmed
        ///</summary>
        ///<param name="domain">The domain to check whether it has been confirmed</param>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet("/tenants/{domain}/confirmed")]
        [AllowAnonymous]
        public async Task<ApiResponse> TenantConfirmed(string domain)
        {
            var apiResponse = await tenantsClient.IsConfirmedAsync(domain);
            return new ApiResponse("Successfully determined tenant confirmation status", apiResponse.Result);
        }

        ///<summary>
        /// Determines whether a tenant is confirmed
        ///</summary>
        ///<param name="domain">The domain to check whether it has been confirmed</param>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [HttpGet("/tenants/{domain}/primary-owner-transfer")]
        [Authorize(Roles = "PrimaryOwner")]
        [Authorize(Policy = "TenantIdResolved")]
        public async Task<ApiResponse> GetPrimaryOwnerTransfer(string domain)
        {
            var apiResponse = await tenantsClient.GetPrimaryOwnerTransferByDomain<PrimaryOwnerTransferModel>(domain);
            return new ApiResponse("Successfully retrieved primary owner transfer", apiResponse.Result);
        }

        ///<summary>
        /// Confirms a tenant's domain
        ///</summary>
        ///<param name="domain">The domain to check whether it has been confirmed</param>
        ///<param name="confirmModel">The model necessary to confirm a tenant's domain</param>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPut("/tenants/{domain}/confirm")]
        [AllowAnonymous]
        public async Task<ApiResponse> ConfirmTenant(string domain, TenantConfirmModel confirmModel)
        {
            var apiResponse = await tenantsClient.ConfirmTenantAsync(domain, JsonConvert.SerializeObject(confirmModel));
            return new ApiResponse(apiResponse.Message, apiResponse.Result);
        }
    }
}