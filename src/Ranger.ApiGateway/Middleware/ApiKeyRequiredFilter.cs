using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Ranger.ApiUtilities;
using Ranger.InternalHttpClient;

namespace Ranger.ApiGateway.Middleware
{
    public class ApiKeyRequiredAttribute : TypeFilterAttribute
    {
        public ApiKeyRequiredAttribute() : base(typeof(ApiKeyRequiredFilterImpl)) { }

        private class ProjectAuthenticationResult
        {
            public string ProjectId { get; set; }
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
                        if (Guid.TryParse(apiKey, out _))
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
                                                await next();
                                            }
                                            else
                                            {
                                                context.Result = new ForbidResult($"The project for the provided x-ranger-apikey header is not enabled.");
                                                return;
                                            }
                                        }
                                        catch (HttpClientException ex)
                                        {
                                            if ((int)ex.ApiResponse.StatusCode == StatusCodes.Status404NotFound)
                                            {
                                                context.Result = new NotFoundObjectResult($"No project was found for the provided x-ranger-apikey header '{apiKey}'.");
                                                return;
                                            }
                                            throw;
                                        }
                                        catch (Exception ex)
                                        {
                                            this.logger.LogError(ex, $"An exception occurred validating the API Key '{apiKey}'.");
                                            context.Result = new StatusCodeResult(StatusCodes.Status500InternalServerError);
                                            return;
                                        }
                                    }
                                    else
                                    {
                                        context.Result = new ForbidResult($"The tenant '{tenantApiResponse.Domain}' is not enabled. Ensure the domain has been confirmed.");
                                        return;
                                    }
                                }
                                catch (HttpClientException ex)
                                {
                                    if ((int)ex.ApiResponse.StatusCode == StatusCodes.Status404NotFound)
                                    {
                                        context.Result = new NotFoundObjectResult($"No tenant found for the provided API Key '{apiKey}'.");
                                        return;
                                    }
                                    throw;
                                }
                                catch (Exception ex)
                                {
                                    this.logger.LogError(ex, $"An exception occurred validating whether the domain exists for API Key '{apiKey}'.");
                                    context.Result = new StatusCodeResult(StatusCodes.Status500InternalServerError);
                                    return;
                                }
                            }
                            catch (HttpClientException ex)
                            {
                                if ((int)ex.ApiResponse.StatusCode == StatusCodes.Status404NotFound)
                                {
                                    context.Result = new NotFoundObjectResult($"No tenant found for the provided API key '{apiKey}'.");
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
                            context.Result = new BadRequestObjectResult(new { errors = new { serverErrors = "The API Key was not a valid format." } });
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