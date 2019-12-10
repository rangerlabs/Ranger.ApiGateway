using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Ranger.InternalHttpClient;
using Ranger.RabbitMQ;

namespace Ranger.ApiGateway
{
    [ApiVersion("1.0")]
    [ApiController]
    public class TenantController : BaseController<TenantController>
    {
        private readonly ITenantsClient tenantsClient;

        public TenantController(ITenantsClient tenantsClient, IBusPublisher busPublisher, ILogger<TenantController> logger) : base(busPublisher, logger)
        {
            this.tenantsClient = tenantsClient;
        }

        [HttpPost("/tenant")]
        [AllowAnonymous]
        public async Task<IActionResult> Post(TenantModel tenantModel)
        {
            var domain = new Domain(tenantModel.DomainForm.Domain.ToLower(), tenantModel.DomainForm.OrganizationName);
            var owner = new NewTenantOwner(tenantModel.UserForm.Email, tenantModel.UserForm.FirstName, tenantModel.UserForm.LastName, tenantModel.UserForm.Password);

            var createTenantMsg = new CreateTenant(domain, owner);
            return await Task.Run(() => Send(createTenantMsg));
        }

        [HttpGet("/tenant/{domain}/exists")]
        [AllowAnonymous]
        public async Task<IActionResult> Exists(string domain)
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

        [HttpGet("/tenant/{domain}/enabled")]
        [AllowAnonymous]
        public async Task<IActionResult> Enabled(string domain)
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

        [HttpPut("/tenant/{domain}/confirm")]
        [AllowAnonymous]
        public async Task<IActionResult> Confirm(string domain, TenantConfirmModel confirmModel)
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