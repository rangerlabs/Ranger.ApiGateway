namespace Ranger.ApiGateway
{
    public static class AuthorizationPolicyNames
    {
        public const string BelongsToProject = "BelongsToProject";
        public const string ValidBreadcrumbApiKey = "ValidBreadcrumbApiKey";
        public const string TenantIdResolved = "TenantIdResolved";
        public const string UserBelongsToProjectOrValidProjectApiKey = "UserBelongsToProjectOrValidProjectApiKey";
    }
}