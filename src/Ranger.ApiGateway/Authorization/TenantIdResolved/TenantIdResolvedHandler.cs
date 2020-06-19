using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Ranger.InternalHttpClient;

namespace Ranger.ApiGateway
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

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, TenantIdResolvedRequirement requirement)
        {
            try
            {
                if (!String.IsNullOrWhiteSpace(httpContextAccessor.HttpContext.Items[HttpContextAuthItems.Project] as string))
                {
                    logger.LogInformation("The TenantId was previously set. Authorization succeeded for policy 'TenantIdResolved");
                    context.Succeed(requirement);
                }
                else
                {
                    var user = context.User.UserFromClaims();
                    if (!String.IsNullOrWhiteSpace(user.Domain))
                    {
                        var tenantId = await TenantIdResolver<TenantIdResolvedHandler>.ResolveTenantId(httpContextAccessor.HttpContext, tenantsClient, user.Domain, logger);
                        if (String.IsNullOrWhiteSpace(tenantId))
                        {
                            context.Fail();
                        }
                        else
                        {
                            httpContextAccessor.HttpContext.Items[HttpContextAuthItems.TenantId] = tenantId;
                            logger.LogInformation("Authorization succeeded for policy 'TenantIdResolved");
                            context.Succeed(requirement);
                        }
                    }
                    else
                    {
                        logger.LogInformation("No domain was present on the user's claims");
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogCritical(ex, "An exception occurred validating the tenant is confirmed");
            }
        }
    }
}