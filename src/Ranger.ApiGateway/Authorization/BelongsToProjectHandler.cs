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
        private readonly ProjectsHttpClient projectsClient;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly ILogger<BelongsToProjectHandler> logger;
        public BelongsToProjectHandler(ProjectsHttpClient projectsClient, IHttpContextAccessor httpContextAccessor, ILogger<BelongsToProjectHandler> logger)
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
                var user = context.User.UserFromClaims();
                if (!String.IsNullOrWhiteSpace(user.Domain) && !String.IsNullOrWhiteSpace(user.Email) && !String.IsNullOrWhiteSpace(user.Role))
                {
                    var roleEnum = Enum.Parse<RolesEnum>(user.Role);
                    if (roleEnum == RolesEnum.User)
                    {
                        var apiResponse = await projectsClient.GetAllProjectsForUserAsync<IEnumerable<ProjectModel>>(user.Domain, user.Email).ConfigureAwait(false);
                        if (apiResponse.IsError)
                        {
                            logger.LogError($"Failed to retrieve authorized projects to validate user's project authorization. Domain: '{user.Domain}', Email: '{user.Email}'.");
                        }
                        else if (apiResponse.Result.Select(_ => _.Name).Contains(projectName))
                        {
                            context.Succeed(requirement);
                        }
                    }
                    else
                    {
                        context.Succeed(requirement);
                    }
                }
            }
        }
    }
}