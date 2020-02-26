using System;
using System.Threading.Tasks;
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
    [ApiVersion("1.0")]
    [ApiController]
    public class TenantController : BaseController<TenantController>
    {
        private readonly ITenantsClient tenantsClient;
        private readonly IBusPublisher busPublisher;

        public TenantController(ITenantsClient tenantsClient, IBusPublisher busPublisher, ILogger<TenantController> logger) : base(busPublisher, logger)
        {
            this.busPublisher = busPublisher;
            this.tenantsClient = tenantsClient;

        }

        [HttpDelete("/tenants/{domain}")]
        [Authorize(Roles = "PrimaryOwner")]
        public async Task<IActionResult> DeleteTenant([FromRoute] string domain)
        {
            var deleteTenantMsg = new DeleteTenantSagaInitializer(User.UserFromClaims().Email, Domain);
            return await Task.Run(() => Send(deleteTenantMsg));
        }

        [HttpPost("/tenants")]
        [AllowAnonymous]
        public async Task<IActionResult> Post(TenantModel tenantModel)
        {
            var createTenantMsg = new CreateTenant(tenantModel.DomainForm.Domain.ToLower(), tenantModel.DomainForm.OrganizationName, tenantModel.UserForm.Email, tenantModel.UserForm.FirstName, tenantModel.UserForm.LastName, tenantModel.UserForm.Password);
            return await Task.Run(() => Send(createTenantMsg));
        }

        [HttpGet("/tenants/{domain}/exists")]
        [AllowAnonymous]
        public async Task<IActionResult> TenantExists(string domain)
        {
            try
            {
                var exists = await tenantsClient.ExistsAsync(domain);
                if (exists)
                {
                    return Ok();
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"An exception occurred validating whether the domain '{domain}' exists.");
                return InternalServerError();
            }
        }

        [HttpGet("/tenants/{domain}/primary-owner-transfer")]
        [Authorize(Roles = "PrimaryOwner")]
        public async Task<IActionResult> GetPrimaryOwnerTransfer(string domain)
        {
            try
            {
                var primaryOwnerTransferModel = await tenantsClient.GetPrimaryOwnerTransferByDomain<PrimaryOwnerTransferModel>(domain);
                if (primaryOwnerTransferModel is null)
                {
                    return NoContent();
                }
                return Ok(primaryOwnerTransferModel);
            }
            catch (HttpClientException<PrimaryOwnerTransferModel> ex)
            {
                if ((int)ex.ApiResponse.StatusCode == StatusCodes.Status404NotFound)
                {
                    return NotFound();
                }
                Logger.LogError(ex, $"An exception occurred retrieving the primary owner transfer for domain '{domain}'.");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"An exception occurred retrieving the primary owner transfer for domain '{domain}'.");
            }
            return InternalServerError();
        }

        [HttpGet("/tenants/{domain}/enabled")]
        [AllowAnonymous]
        public async Task<IActionResult> TenantEnabled(string domain)
        {
            try
            {
                var enabled = await tenantsClient.EnabledAsync<TenantEnabledModel>(domain);
                return Ok(new { enabled = enabled.Enabled });
            }
            catch (HttpClientException<TenantEnabledModel> ex)
            {
                if ((int)ex.ApiResponse.StatusCode == StatusCodes.Status404NotFound)
                {
                    return NotFound();
                }
                throw;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"An exception occurred determining if the tenant domain '{domain}' was confirmed.");
                return InternalServerError();
            }
        }

        [HttpPut("/tenants/{domain}/confirm")]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmTenant(string domain, TenantConfirmModel confirmModel)
        {
            try
            {
                var result = await tenantsClient.ConfirmTenantAsync(domain, JsonConvert.SerializeObject(confirmModel));
                if (result)
                {
                    return Ok();
                }
                return StatusCode(StatusCodes.Status409Conflict);
            }
            catch (HttpClientException ex)
            {
                if ((int)ex.ApiResponse.StatusCode == StatusCodes.Status404NotFound)
                {
                    return NotFound();
                }
                throw;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"An exception occurred confirming the tenant domain '{domain}'.");
                return InternalServerError();
            }
        }
    }
}