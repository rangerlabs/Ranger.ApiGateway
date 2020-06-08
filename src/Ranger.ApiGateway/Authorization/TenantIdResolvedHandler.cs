using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Ranger.InternalHttpClient;

namespace Ranger.ApiGateway.Authorization
{
    public class TenantIdResolvedHandler : AuthorizationHandler<TenantIdResolvedRequirement>
    {
        private readonly TenantsHttpClient tenantsClient;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly ILogger<TenantIdResolvedHandler> logger;
        public TenantIdResolvedHandler(TenantsHttpClient tenantsClient, IHttpContextAccessor httpContextAccessor, ILogger<TenantIdResolvedHandler> logger)
        {
            this.logger = logger;
            this.httpContextAccessor = httpContextAccessor;
            this.tenantsClient = tenantsClient;
        }
        private class TenantResult
        {
            public string TenantId { get; set; }
            public bool Confirmed { get; set; }
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, TenantIdResolvedRequirement requirement)
        {
            var domain = GetDomainFromUserClaims(httpContextAccessor);
            if (!String.IsNullOrWhiteSpace(domain))
            {
                var tenantApiResponse = await tenantsClient.GetTenantByDomainAsync<TenantResult>(domain);
                if (!tenantApiResponse.IsError)
                {
                    if (tenantApiResponse.Result.Confirmed)
                    {
                        httpContextAccessor.HttpContext.Items["TenantId"] = tenantApiResponse.Result.TenantId;
                        context.Succeed(requirement);
                    }
                    else
                    {
                        logger.LogInformation("The tenant is not yet confirmed");
                    }
                }
                else
                {
                    logger.LogInformation("Received {Status} when attempting to retrieve tenant for domain {Domain}", tenantApiResponse.StatusCode, domain);
                }
            }
            else
            {
                logger.LogInformation("No domain was present on the user's claims");
            }
        }

        private string GetDomainFromUserClaims(IHttpContextAccessor context) => context.HttpContext.User.UserFromClaims().Domain;
    }
}