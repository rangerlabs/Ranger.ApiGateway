using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoWrapper.Wrappers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Ranger.InternalHttpClient;

namespace Ranger.ApiGateway.Tests
{

    public partial class BelongsToProjectHandlerStub : AuthorizationHandler<BelongsToProjectRequirementStub>
    {
        private readonly IHttpContextAccessor httpContextAccessor;

        public BelongsToProjectHandlerStub(IHttpContextAccessor httpContextAccessor)
        {
            this.httpContextAccessor = httpContextAccessor;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, BelongsToProjectRequirementStub requirement)
        {
            httpContextAccessor.HttpContext.Items[HttpContextAuthItems.TenantId] = "tenant-1";
            httpContextAccessor.HttpContext.Items[HttpContextAuthItems.Project] = new ProjectModel{Id = Guid.NewGuid(), Name = "project-1", Enabled = true};
            context.Succeed(requirement);
            await Task.CompletedTask;
        }
    }
}