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
    public class BelongsToProjectHandler : AuthorizationHandler<BelongsToProjectRequirement>
    {
        private readonly IProjectsClient projectsClient;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly ILogger<BelongsToProjectHandler> logger;
        public BelongsToProjectHandler(IProjectsClient projectsClient, IHttpContextAccessor httpContextAccessor, ILogger<BelongsToProjectHandler> logger)
        {
            this.logger = logger;
            this.httpContextAccessor = httpContextAccessor;
            this.projectsClient = projectsClient;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, BelongsToProjectRequirement requirement)
        {
            var projectName = httpContextAccessor.HttpContext.Request.Path.Value.Split("/")[1];
            if (!String.IsNullOrWhiteSpace(projectName))
            {
                var domain = httpContextAccessor.HttpContext.Request.Headers["x-ranger-domain"].First();
                if (!String.IsNullOrWhiteSpace(domain))
                {
                    var email = context.User.UserFromClaims().Email;
                    if (!String.IsNullOrWhiteSpace(email))
                    {
                        var role = context.User.UserFromClaims().Role;
                        if (!String.IsNullOrWhiteSpace(role))
                        {
                            var roleEnum = Enum.Parse<RolesEnum>(role);
                            if (roleEnum == RolesEnum.User)
                            {
                                IEnumerable<ProjectModel> authorizedProjectNames;
                                try
                                {
                                    authorizedProjectNames = await projectsClient.GetAllProjectsForUserAsync<IEnumerable<ProjectModel>>(domain, email).ConfigureAwait(false);
                                }
                                catch (Exception ex)
                                {
                                    logger.LogError(ex, $"Failed to retrieve authorized projects to validate user's project authorization. Domain: '{domain}', Email: '{email}'.");
                                    throw;
                                }
                                if (authorizedProjectNames.Select(_ => _.Name).Contains(projectName))
                                {
                                    context.Succeed(requirement);
                                }
                            }
                            context.Succeed(requirement);
                        }
                    }
                }
            }
            return;
        }
    }
}