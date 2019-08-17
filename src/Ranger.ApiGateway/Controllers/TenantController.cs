using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Ranger.InternalHttpClient;
using Ranger.RabbitMQ;

namespace Ranger.ApiGateway {
    [ApiController]
    public class TenantController : BaseController {
        private readonly ITenantsClient tenantsClient;

        public TenantController (ITenantsClient tenantsClient, IBusPublisher busPublisher, ILogger<TenantController> logger) : base (busPublisher, logger) {
            this.tenantsClient = tenantsClient;
        }

        [HttpPost ("/tenant")]
        [AllowAnonymous]
        public async Task<IActionResult> Post (TenantModel tenantModel) {
            var domain = new Domain (tenantModel.DomainForm.Domain.ToLower (), tenantModel.DomainForm.OrganizationName);
            var owner = new NewTenantOwner (tenantModel.UserForm.Email, tenantModel.UserForm.FirstName, tenantModel.UserForm.LastName, tenantModel.UserForm.Password);

            var createTenantMsg = new CreateTenant (domain, owner);
            return await Task.Run (() => Send (createTenantMsg));
        }

        [HttpGet ("/tenant/{domain}/exists")]
        [AllowAnonymous]
        public async Task<IActionResult> Exists (string domain) {
            var exists = false;
            try {
                exists = await tenantsClient.ExistsAsync (domain);
                if (exists) {
                    return Ok ();
                } else {
                    return NotFound ();
                }
            } catch (Exception ex) {
                Logger.LogError (ex, $"An exception occurred validating whether the domain '{domain}' exists.");
                return InternalServerError ();
            }
        }
    }
}