using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Ranger.ApiUtilities;
using Ranger.Common;
using Ranger.InternalHttpClient;
using Ranger.RabbitMQ;

namespace Ranger.ApiGateway
{
    [ApiVersion("1.0")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class UserController : BaseController
    {
        private readonly IIdentityClient identityClient;

        public UserController(IBusPublisher busPublisher, IIdentityClient identityClient, ILogger<UserController> logger) : base(busPublisher, logger)
        {
            this.identityClient = identityClient;
        }

        [HttpPut("/user/confirm")]
        [AllowAnonymous]
        public async Task<IActionResult> Confirm(ConfirmModel confirmModel)
        {
            if (string.IsNullOrWhiteSpace(confirmModel.Domain))
            {
                var apiErrorContent = new ApiErrorContent();
                apiErrorContent.Errors.Add($"{nameof(confirmModel.Domain)} was null or whitespace.");
                return BadRequest(apiErrorContent);
            }
            if (string.IsNullOrWhiteSpace(confirmModel.RegistrationKey))
            {
                var apiErrorContent = new ApiErrorContent();
                apiErrorContent.Errors.Add($"{nameof(confirmModel.RegistrationKey)} was null or whitespace.");
                return BadRequest(apiErrorContent);
            }

            bool confirmed = await identityClient.ConfirmUserAsync(confirmModel.Domain, confirmModel.RegistrationKey);
            return confirmed ? NoContent() : StatusCode(StatusCodes.Status304NotModified);
        }

        [HttpGet("/user")]
        [TenantDomainRequired]
        public async Task<IActionResult> Index([FromQuery]string email)
        {
            ApplicationUserApiResponseModel applicationUserApiResponse = null;
            try
            {
                applicationUserApiResponse = await identityClient.GetAllUsersAsync<ApplicationUserApiResponseModel>(Domain);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"An exception occurred retrieving the retrieving the application user for email '{email}'.");
                return InternalServerError($"An error occurred retrieving the retrieving the application user for email '{email}'.");
            }

            var userResponseModel = new ApplicationUserApiResponseModel
            {
                Email = applicationUserApiResponse.Email,
                FirstName = applicationUserApiResponse.FirstName,
                LastName = applicationUserApiResponse.LastName,
                Role = applicationUserApiResponse.Role,
            };
            return Ok(userResponseModel);
        }

        [HttpGet("/user/all")]
        [TenantDomainRequired]
        public async Task<IActionResult> All()
        {
            IEnumerable<ApplicationUserApiResponseModel> applicationUserApiResponse = null;
            try
            {
                applicationUserApiResponse = await identityClient.GetAllUsersAsync<IEnumerable<ApplicationUserApiResponseModel>>(Domain);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"An exception occurred retrieving the retrieving the application users.");
                return InternalServerError($"An error occurred retrieving the retrieving the application users.");
            }

            var userResponseCollection = new List<ApplicationUserApiResponseModel>();
            foreach (var user in applicationUserApiResponse)
            {
                var userResponseModel = new ApplicationUserApiResponseModel
                {
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Role = user.Role,
                };
                userResponseCollection.Add(userResponseModel);
            }
            return Ok(userResponseCollection);
        }

        [HttpPost("/user")]
        [TenantDomainRequired]
        public async Task<IActionResult> Post(PostApplicationUserModel postApplicationUserModel)
        {
            var domain = HttpContext.Request.Headers.GetPreviouslyVerifiedTenantHeader();
            var applicationUserCommand = new CreateNewApplicationUserSagaInitializer(
                domain,
                postApplicationUserModel.Email,
                postApplicationUserModel.FirstName,
                postApplicationUserModel.LastName,
                postApplicationUserModel.Role,
                User.UserFromClaims().Email,
                postApplicationUserModel.AuthorizedProjects
            );
            return await Task.Run(() => base.Send(applicationUserCommand));
        }

        [HttpPut("/user/{email}")]
        [TenantDomainRequired]
        public async Task<IActionResult> PutPermissions([FromRoute]string email, PutPermissionsModel putPermissionsModel)
        {
            if (putPermissionsModel == null)
            {
                var errors = new ApiErrorContent();
                errors.Errors.Add($"The body content was not found or could not be bound.");
                return BadRequest(errors);
            }
            if (String.IsNullOrWhiteSpace(putPermissionsModel.Role) && (putPermissionsModel.AuthorizedProjects == null || putPermissionsModel.AuthorizedProjects.Count == 0))
            {
                var errors = new ApiErrorContent();
                errors.Errors.Add($"Both Role and Permitted Projects do not have values. Provide one or both values to update.");
                return BadRequest(errors);
            }
            var domain = HttpContext.Request.Headers.GetPreviouslyVerifiedTenantHeader();
            var updateUserPermissionsCommand = new UpdateUserPermissions(
                domain,
                email,
                putPermissionsModel.Role,
                putPermissionsModel.AuthorizedProjects
            );
            return await Task.Run(() => base.Send(updateUserPermissionsCommand));
        }
    }
}