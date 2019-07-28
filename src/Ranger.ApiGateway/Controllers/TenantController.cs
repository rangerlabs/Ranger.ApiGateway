using System;
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
    public class TenantController : BaseController<TenantController> {
        private readonly ITenantsClient tenantsClient;
        private readonly IBusPublisher busPublisher;
        private readonly ILogger<TenantController> logger;

        public TenantController (ITenantsClient tenantsClient, IBusPublisher busPublisher, ILogger<TenantController> logger) : base (busPublisher, logger)

        {
            this.tenantsClient = tenantsClient;
            this.busPublisher = busPublisher;
            this.logger = logger;
        }

        [HttpPost ("/tenant")]
        [AllowAnonymous]
        public async Task<IActionResult> Post (TenantModel tenantModel) {
            IActionResult response = new StatusCodeResult (StatusCodes.Status500InternalServerError);

            var domain = new Domain (tenantModel.DomainForm.Domain.ToLower (), tenantModel.DomainForm.OrganizationName);
            var user = new User (tenantModel.UserForm.Email, tenantModel.UserForm.FirstName, tenantModel.UserForm.LastName, tenantModel.UserForm.Password);

            var createTenantMsg = new CreateTenant (domain, user);
            Send (createTenantMsg);
            response = new AcceptedResult ();
            return response;
        }

        [HttpGet ("/tenant/exists")]
        [AllowAnonymous]
        public async Task<IActionResult> Exists (string domain) {
            IActionResult response = new StatusCodeResult (StatusCodes.Status500InternalServerError);
            await tenantsClient.SetClientToken ();
            var apiResponse = await tenantsClient.ExistsAsync (domain);
            if (apiResponse.IsSuccessStatusCode) {
                response = Ok (apiResponse.ResponseObject);
            }
            return response;
        }
    }
}