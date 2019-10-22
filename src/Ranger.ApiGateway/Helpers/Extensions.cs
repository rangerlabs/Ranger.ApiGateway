using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using System.Collections.Generic;
using System.Linq;

namespace Ranger.ApiGateway
{
    public static class Extensions
    {
        public static string GetPreviouslyVerifiedTenantHeader(this IHeaderDictionary headers)
        {
            StringValues domain;
            headers.TryGetValue("x-ranger-domain", out domain);
            return domain[0];
        }
    }
}