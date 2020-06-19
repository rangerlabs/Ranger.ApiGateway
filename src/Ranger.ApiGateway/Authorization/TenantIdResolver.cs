using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Ranger.InternalHttpClient;

namespace Ranger.ApiGateway
{
    public static class TenantIdResolver<TAuthorizationHandler> where TAuthorizationHandler : IAuthorizationHandler
    {
        public static async Task<string> ResolveTenantId(HttpContext httpContext, TenantsHttpClient tenantsHttpClient, string domain, ILogger<TAuthorizationHandler> logger)
        {
            var tenantId = httpContext.Items[HttpContextAuthItems.TenantId] as string;
            if (!String.IsNullOrWhiteSpace(tenantId))
            {
                return tenantId;
            }
            else
            {
                var tenantApiResponse = await tenantsHttpClient.GetTenantByDomainAsync<TenantResultModel>(domain);
                if (!tenantApiResponse.IsError)
                {
                    if (tenantApiResponse.Result.Confirmed)
                    {
                        return tenantApiResponse.Result.TenantId;
                    }
                    else
                    {
                        logger.LogInformation("The tenant with tenant id {TenantID} is not yet confirmed", tenantApiResponse.Result.TenantId);
                    }
                }
                else
                {
                    logger.LogInformation("Received {Status} when attempting to retrieve tenant for domain {Domain}", tenantApiResponse.StatusCode, domain);
                }
            }
            return default;
        }
    }
}