using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Ranger.InternalHttpClient;

namespace Ranger.ApiGateway {
    public class ValidateDomainExistsAttribute : TypeFilterAttribute {
        public ValidateDomainExistsAttribute () : base (typeof (ValidateDomainExistsFilterImpl)) { }
        private class ValidateDomainExistsFilterImpl : IAsyncActionFilter {
            private readonly ITenantsClient tenantsClient;

            public ILogger<ValidateDomainExistsFilterImpl> logger { get; }

            public ValidateDomainExistsFilterImpl (ITenantsClient tenantsClient, ILogger<ValidateDomainExistsFilterImpl> logger) {
                this.tenantsClient = tenantsClient;
                this.logger = logger;
            }
            public async Task OnActionExecutionAsync (ActionExecutingContext context, ActionExecutionDelegate next) {

                string domain = context.HttpContext.Request.Headers["Domain"];
                await tenantsClient.SetClientToken ();
                var tenantExistsResponse = await tenantsClient.ExistsAsync (domain);
                if (tenantExistsResponse.IsSuccessStatusCode && tenantExistsResponse.ResponseObject) {
                    await next ();
                } else if (tenantExistsResponse.IsSuccessStatusCode && !tenantExistsResponse.IsSuccessStatusCode) {
                    context.Result = new NotFoundObjectResult ($"No tenant found for the requested domain '{domain}'.");
                } else {
                    logger.LogError ("Failed to validate domain {domain}.", tenantExistsResponse.Errors);
                }
            }
        }
    }
}