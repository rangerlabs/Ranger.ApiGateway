using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Ranger.InternalHttpClient;
using Ranger.RabbitMQ;

namespace Ranger.ApiGateway {
    [ApiController]
    public class TenantController : ControllerBase {
        private readonly ITenantsClient tenantsClient;
        private readonly IBusPublisher busPublisher;

        public TenantController (ITenantsClient tenantsClient, IBusPublisher busPublisher) {
            this.tenantsClient = tenantsClient;
            this.busPublisher = busPublisher;
        }

        [HttpPost ("/tenant")]
        [AllowAnonymous]
        public async Task<IActionResult> Post (TenantModel tenantModel) {
            IActionResult response = new StatusCodeResult (StatusCodes.Status500InternalServerError);
            var correlationContext = new CorrelationContext {
                Id = Guid.NewGuid (),
                UserId = Guid.NewGuid (),
                ResourceId = Guid.NewGuid (),
                TraceId = "",
                SpanContext = "",
                ConnectionId = this.HttpContext.Connection.Id,
                Name = "Create_Tenant",
                Resource = "Api_Gateway",
                Origin = "ApiGateway",
                Culture = "",
                CreatedAt = DateTime.UtcNow,
                Retries = 0
            };

            var domain = new Domain (tenantModel.DomainForm.Domain.ToLower (), tenantModel.DomainForm.OrganizationName);
            var user = new User (tenantModel.UserForm.Email, tenantModel.UserForm.FirstName, tenantModel.UserForm.LastName, tenantModel.UserForm.Password);

            var createTenantMsg = new CreateTenant (correlationContext, domain, user);
            busPublisher.Send<CreateTenant> (createTenantMsg);
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