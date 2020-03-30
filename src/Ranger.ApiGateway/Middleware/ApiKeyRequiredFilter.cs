using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;

using Ranger.InternalHttpClient;

namespace Ranger.ApiGateway.Middleware
{
    public class ApiKeyRequiredAttribute : TypeFilterAttribute
    {
        public ApiKeyRequiredAttribute() : base(typeof(ApiKeyRequiredFilterImpl)) { }

        private class ProjectAuthenticationResult
        {
            public Guid ProjectId { get; set; }
            public string Name { get; set; }
            public bool Enabled { get; set; }
        }

        private class DatabaseUsernameResult
        {
            public string DatabaseUsername { get; set; }
        }

        private class TenantResult
        {
            public bool Enabled { get; set; }
            public string Domain { get; set; }
        }

        private class ApiKeyRequiredFilterImpl : IAsyncActionFilter
        {
            private readonly ITenantsClient tenantsClient;
            private readonly IProjectsClient projectsClient;
            private readonly ILogger<ApiKeyRequiredFilterImpl> logger;
            public ApiKeyRequiredFilterImpl(ITenantsClient tenantsClient, IProjectsClient projectsClient, ILogger<ApiKeyRequiredFilterImpl> logger)
            {
                this.tenantsClient = tenantsClient;
                this.projectsClient = projectsClient;
                this.logger = logger;
            }

            public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
            {
                StringValues apiKey;

                var apiKeySuccess = context.HttpContext.Request.Headers.TryGetValue("x-ranger-apikey", out apiKey);
                if (apiKeySuccess)
                {
                    if (apiKey.Count == 1)
                    {
                        var apiKeyParts = apiKey[0].Split('.');
                        if (apiKeyParts?.Length == 2 && (apiKeyParts[0] == "live" || apiKeyParts[0] == "test") && Guid.TryParse(apiKeyParts[1], out _))
                        {
                            try
                            {
                                //TODO: Cache this
                                var databaseUsernameResult = await projectsClient.GetDatabaseUsernameByApiKeyAsync<DatabaseUsernameResult>(apiKey);
                                TenantResult tenantApiResponse = null;
                                try
                                {
                                    //TODO: Cache this
                                    tenantApiResponse = await tenantsClient.GetTenantByDatabaseUsernameAsync<TenantResult>(databaseUsernameResult.DatabaseUsername);
                                    if (tenantApiResponse.Enabled)
                                    {
                                        try
                                        {
                                            //TODO: Cache this
                                            var projectApiResponse = await projectsClient.GetProjectByApiKeyAsync<ProjectAuthenticationResult>(tenantApiResponse.Domain, apiKey);
                                            if (projectApiResponse.Enabled)
                                            {
                                                context.HttpContext.Items["ApiKeyEnvironment"] = apiKeyParts[0].ToUpperInvariant();
                                                context.HttpContext.Items["DatabaseUsername"] = databaseUsernameResult.DatabaseUsername;
                                                context.HttpContext.Items["ProjectId"] = projectApiResponse.ProjectId.ToString();
                                                context.HttpContext.Items["Domain"] = tenantApiResponse.Domain;

                                                await next();
                                            }
                                            else
                                            {
                                                context.Result = new ContentResult()
                                                {
                                                    StatusCode = StatusCodes.Status403Forbidden,
                                                    Content = $"The project is not enabled.",
                                                    ContentType = "application/json"
                                                };
                                                return;
                                            }
                                        }
                                        catch (HttpClientException<ProjectAuthenticationResult> ex)
                                        {
                                            if ((int)ex.ApiResponse.StatusCode == StatusCodes.Status404NotFound)
                                            {
                                                logger.LogError($"A mismatched occured in the project service for tenant '{tenantApiResponse.Domain}' and '{apiKey}'. The tenant was found and enabled but no project was found.");
                                                context.Result = new StatusCodeResult(StatusCodes.Status500InternalServerError);
                                                return;
                                            }
                                            throw;
                                        }
                                        catch (Exception ex)
                                        {
                                            this.logger.LogError(ex, $"An exception occurred validating the API key '{apiKey}'.");
                                            context.Result = new StatusCodeResult(StatusCodes.Status500InternalServerError);
                                            return;
                                        }
                                    }
                                    else
                                    {
                                        logger.LogDebug($"The tenant '{tenantApiResponse.Domain}' is not enabled. Ensure the domain has been confirmed.");
                                        context.Result = new UnauthorizedResult();
                                        return;
                                    }
                                }
                                catch (HttpClientException<TenantResult> ex)
                                {
                                    if ((int)ex.ApiResponse.StatusCode == StatusCodes.Status404NotFound)
                                    {

                                        logger.LogError($"No tenant found for the Database user '{databaseUsernameResult.DatabaseUsername}' for the provided API key '{apiKey}'.");
                                        context.Result = new StatusCodeResult(StatusCodes.Status500InternalServerError);
                                        return;
                                    }
                                    throw;
                                }
                                catch (Exception ex)
                                {
                                    this.logger.LogError(ex, $"An exception occurred validating whether the domain exists for API key '{apiKey}'.");
                                    context.Result = new StatusCodeResult(StatusCodes.Status500InternalServerError);
                                    return;
                                }
                            }
                            catch (HttpClientException<DatabaseUsernameResult> ex)
                            {
                                if ((int)ex.ApiResponse.StatusCode == StatusCodes.Status404NotFound)
                                {
                                    logger.LogDebug($"No database user was found in the projects service for the provided API key '{apiKey}'.");
                                    context.Result = new UnauthorizedResult();
                                    return;
                                }
                                throw;
                            }
                            catch (Exception ex)
                            {
                                this.logger.LogError(ex, $"An exception occurred retrieving the database username for API key '{apiKey}'.");
                                context.Result = new StatusCodeResult(StatusCodes.Status500InternalServerError);
                                return;
                            }
                        }
                        else
                        {

                            logger.LogDebug($"The API key was not a valid format: '{apiKey}'.");
                            context.Result = new UnauthorizedResult();
                            return;
                        }
                    }
                    else
                    {
                        context.Result = new BadRequestObjectResult(new { errors = new { serverErrors = "Multiple x-ranger-apikey header values were found for x-ranger-apikey." } });
                        return;
                    }
                }
                else
                {
                    context.Result = new BadRequestObjectResult(new { errors = new { serverErrors = "No x-ranger-apikey header value was found." } });
                    return;
                }
            }
        }
    }
}