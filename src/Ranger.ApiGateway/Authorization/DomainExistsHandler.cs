using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Ranger.Common;
using Ranger.InternalHttpClient;

namespace Ranger.ApiGateway.Authorization
{
    public class DomainExistsHandler : AuthorizationHandler<DomainExistsRequirement>
    {
        private readonly TenantsHttpClient tenantsClient;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly ILogger<DomainExistsHandler> logger;
        public DomainExistsHandler(TenantsHttpClient tenantsClient, IHttpContextAccessor httpContextAccessor, ILogger<DomainExistsHandler> logger)
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

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, DomainExistsRequirement requirement)
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
                        logger.LogDebug("The tenant is not yet confirmed");
                    }
                }
                else
                {
                    logger.LogDebug("Received {Status} when attempting to retrieve tenant for domain {Domain}", tenantApiResponse.StatusCode, domain);
                }
            }
            else
            {
                logger.LogDebug("No domain was present on the user's claims");
            }
        }

        private string GetDomainFromUserClaims(IHttpContextAccessor context) => context.HttpContext.User.UserFromClaims().Domain;
    }
}