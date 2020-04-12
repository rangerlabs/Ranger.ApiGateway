using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Ranger.Common;
using Ranger.InternalHttpClient;
using Ranger.RabbitMQ;

namespace Ranger.ApiGateway
{
    [ApiVersion("1.0")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class UserController : BaseController<UserController>
    {
        private readonly IIdentityClient identityClient;
        private readonly ILogger<UserController> logger;
        private readonly IProjectsClient projectsClient;
        private readonly IBusPublisher busPublisher;

        public UserController(IBusPublisher busPublisher, IIdentityClient identityClient, IProjectsClient projectsClient, ILogger<UserController> logger) : base(busPublisher, logger)
        {
            this.busPublisher = busPublisher;
            this.projectsClient = projectsClient;
            this.identityClient = identityClient;
            this.logger = logger;
        }

        [HttpDelete("/users/{email}")]
        public async Task<IActionResult> DeleteUser([FromRoute] string email)
        {
            try
            {
                var deleteUserContent = new DeleteUserModel()
                {
                    CommandingUserEmail = UserFromClaims.Email
                };
                await identityClient.DeleteUserAsync(UserFromClaims.Domain, email, JsonConvert.SerializeObject(deleteUserContent));
            }
            catch (HttpClientException ex)
            {
                if ((int)ex.ApiResponse.StatusCode == StatusCodes.Status402PaymentRequired)
                {
                    var errors = new ApiErrorContent();
                    errors.Errors = ex.ApiResponse.Errors.Errors;
                    return StatusCode(StatusCodes.Status402PaymentRequired, errors);
                }
                if ((int)ex.ApiResponse.StatusCode == StatusCodes.Status403Forbidden)
                {
                    return Forbid();
                }
                if ((int)ex.ApiResponse.StatusCode == StatusCodes.Status404NotFound)
                {
                    return NotFound();
                }
                return InternalServerError();
            }
            busPublisher.Send(new SendPusherDomainUserPredefinedNotification("ForceSignoutNotification", UserFromClaims.Domain, email), CorrelationContext.Empty);
            return NoContent();
        }

        [HttpPut("/users/{email}/password-reset")]
        public async Task<IActionResult> PasswordResetRequest([FromRoute] string email, PasswordResetModel passwordResetModel)
        {
            bool submitted = await identityClient.RequestPasswordReset(UserFromClaims.Domain, email, JsonConvert.SerializeObject(passwordResetModel));
            return submitted ? NoContent() : StatusCode(StatusCodes.Status400BadRequest);
        }

        [HttpPut("/users/{email}/email-change")]
        public async Task<IActionResult> EmailChangeRequest([FromRoute] string email, EmailChangeModel emailChangeModel)
        {
            bool submitted = false;
            try
            {
                submitted = await identityClient.RequestEmailChange(UserFromClaims.Domain, email, JsonConvert.SerializeObject(emailChangeModel));
            }
            catch (HttpClientException ex)
            {
                if ((int)ex.ApiResponse.StatusCode == StatusCodes.Status409Conflict)
                {
                    return Conflict(ex.ApiResponse.Errors);
                }
                throw;
            }
            return submitted ? NoContent() : StatusCode(StatusCodes.Status400BadRequest);
        }

        [HttpPost("/users/{userId}/password-reset")]
        [AllowAnonymous]
        public async Task<IActionResult> PasswordReset([FromRoute] string userId, UserConfirmPasswordResetModel confirmModel)
        {
            try
            {
                var requestContent = JsonConvert.SerializeObject(new
                {
                    NewPassword = confirmModel.NewPassword,
                    ConfirmPassword = confirmModel.ConfirmPassword,
                    Token = confirmModel.Token
                });
                bool confirmed = await identityClient.UserConfirmPasswordResetAsync(confirmModel.Domain, userId, requestContent);
                return confirmed ? NoContent() : StatusCode(StatusCodes.Status304NotModified);
            }
            catch (HttpClientException ex)
            {
                logger.LogError(ex, "Failed to reset user password.");
                return InternalServerError();
            }
        }

        [HttpPost("/users/{userId}/email-change")]
        [AllowAnonymous]
        public async Task<IActionResult> EmailChange([FromRoute] string userId, UserConfirmEmailChangeModel userConfirmEmailChangeModel)
        {
            var requestContent = new
            {
                Email = userConfirmEmailChangeModel.Email,
                Token = userConfirmEmailChangeModel.Token
            };
            try
            {
                await identityClient.UserConfirmEmailChangeAsync(userConfirmEmailChangeModel.Domain, userId, JsonConvert.SerializeObject(requestContent));
            }
            catch (HttpClientException ex)
            {
                if ((int)ex.ApiResponse.StatusCode == StatusCodes.Status409Conflict)
                {
                    return Conflict(ex.ApiResponse.Errors);
                }
                if ((int)ex.ApiResponse.StatusCode == StatusCodes.Status400BadRequest)
                {
                    return BadRequest(ex.ApiResponse.Errors);
                }
                return InternalServerError();
            }
            return NoContent();
        }

        [HttpGet("/users/{email}/authorized-projects")]
        public async Task<IActionResult> GetAuthorizedProjectsForUser([FromRoute] string email)
        {
            IEnumerable<string> result = null;
            try
            {
                result = await projectsClient.GetProjectIdsForUser(UserFromClaims.Domain, email);
            }
            catch (HttpClientException<IEnumerable<string>> ex)
            {
                if ((int)ex.ApiResponse.StatusCode == StatusCodes.Status404NotFound)
                {
                    return NotFound();
                }
                if ((int)ex.ApiResponse.StatusCode == StatusCodes.Status400BadRequest)
                {
                    var badRequestContent = new ApiErrorContent();
                    badRequestContent.Errors.Add("Ensure the user's email is a valid format.");
                    return BadRequest(badRequestContent);
                }
            }
            return Ok(result);
        }

        [HttpPut("/users/{userId}/confirm")]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmUser([FromRoute] string userId, UserConfirmModel confirmModel)
        {
            try
            {
                var requestContent = JsonConvert.SerializeObject(new
                {
                    NewPassword = confirmModel.NewPassword,
                    ConfirmPassword = confirmModel.ConfirmPassword,
                    Token = confirmModel.Token
                });
                bool confirmed = await identityClient.ConfirmUserAsync(confirmModel.Domain, userId, requestContent);
                return confirmed ? NoContent() : StatusCode(StatusCodes.Status304NotModified);
            }
            catch (HttpClientException ex)
            {
                logger.LogError(ex, "Failed to set new user password.");
                return InternalServerError();
            }
        }

        [HttpGet("/users/{email}")]
        public async Task<IActionResult> GetUser([FromRoute] string email)
        {
            UserApiResponseModel applicationUserApiResponse = null;
            try
            {
                applicationUserApiResponse = await identityClient.GetUserAsync<UserApiResponseModel>(UserFromClaims.Domain, email);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"An exception occurred retrieving the application user for email '{email}'.");
                return InternalServerError($"An error occurred retrieving the application user for email '{email}'.");
            }

            var userResponseModel = new UserApiResponseModel
            {
                Email = applicationUserApiResponse.Email,
                FirstName = applicationUserApiResponse.FirstName,
                LastName = applicationUserApiResponse.LastName,
                Role = applicationUserApiResponse.Role,
            };
            return Ok(userResponseModel);
        }

        [HttpGet("/users")]
        public async Task<IActionResult> GetAllUsers()
        {
            IEnumerable<UserApiResponseModel> applicationUserApiResponse = null;
            try
            {
                applicationUserApiResponse = await identityClient.GetAllUsersAsync<IEnumerable<UserApiResponseModel>>(UserFromClaims.Domain);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"An exception occurred retrieving the application users.");
                return InternalServerError($"An error occurred retrieving the application users.");
            }

            var userResponseCollection = new List<UserApiResponseModel>();
            foreach (var user in applicationUserApiResponse)
            {
                var userResponseModel = new UserApiResponseModel
                {
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Role = user.Role,
                    AuthorizedProjects = user.AuthorizedProjects,
                    EmailConfirmed = user.EmailConfirmed
                };
                userResponseCollection.Add(userResponseModel);
            }
            if (userResponseCollection.Count() > 0)
            {
                return Ok(userResponseCollection);
            }
            else
            {
                return NoContent();
            }
        }

        [HttpPost("/users")]
        public async Task<IActionResult> CreateUser(PostApplicationUserModel postApplicationUserModel)
        {
            var user = UserFromClaims;
            var applicationUserCommand = new CreateUserSagaInitializer(
                UserFromClaims.Domain,
                postApplicationUserModel.Email,
                postApplicationUserModel.FirstName,
                postApplicationUserModel.LastName,
                postApplicationUserModel.Role,
                user.Email,
                postApplicationUserModel.AuthorizedProjects
            );
            return await Task.Run(() => base.Send(applicationUserCommand));
        }

        [HttpPut("/users/{email}")]
        public async Task<IActionResult> PutPermissions([FromRoute] string email, PutPermissionsModel putPermissionsModel)
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
            var user = UserFromClaims;
            var updateUserPermissionsCommand = new UpdateUserPermissionsSagaInitializer(
                UserFromClaims.Domain,
                email,
                user.Email,
                putPermissionsModel.Role,
                putPermissionsModel.AuthorizedProjects
            );
            return await Task.Run(() => base.Send(updateUserPermissionsCommand));
        }
    }
}