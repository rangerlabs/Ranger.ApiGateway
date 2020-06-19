using Microsoft.AspNetCore.Authorization;

namespace Ranger.ApiGateway
{
    ///<summary>
    /// This requirement is necessary because the User Role can access all endpoints that the Project API Key is permitted to access,
    /// however, the Project API Key may not access all endpoints the User Role can access
    /// For Example, the the Project API Key can GET the Tenant, and all /geofences endpoints, but cannot GET /integrations endpoints
    ///</summary>
    public class UserBelongsToProjectOrValidProjectApiKeyRequirement : IAuthorizationRequirement
    {
    }
}