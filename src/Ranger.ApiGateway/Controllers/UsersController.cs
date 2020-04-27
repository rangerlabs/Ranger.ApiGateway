using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoWrapper.Wrappers;
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
        private readonly IdentityHttpClient identityClient;
        private readonly ILogger<UserController> logger;
        private readonly ProjectsHttpClient projectsClient;
        private readonly TenantsHttpClient tenantsClient;
        private readonly IBusPublisher busPublisher;

        public UserController(IBusPublisher busPublisher, IdentityHttpClient identityClient, ProjectsHttpClient projectsClient, TenantsHttpClient tenantsClient, ILogger<UserController> logger) : base(busPublisher, logger)
        {
            this.busPublisher = busPublisher;
            this.projectsClient = projectsClient;
            this.tenantsClient = tenantsClient;
            this.identityClient = identityClient;
            this.logger = logger;
        }

        ///<summary>
        /// Deletes a user
        ///</summary>
        ///<param name="email">The email address of the user to delete</param>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [HttpDelete("/users/{email}")]
        [Authorize(Policy = "TenantIdResolved")]
        public async Task<ApiResponse> DeleteUser(string email)
        {
            var deleteUserContent = new DeleteUserModel()
            {
                CommandingUserEmail = UserFromClaims.Email
            };
            var apiResponse = await identityClient.DeleteUserAsync(TenantId, email, JsonConvert.SerializeObject(deleteUserContent));
            busPublisher.Send(new SendPusherDomainUserPredefinedNotification("ForceSignoutNotification", TenantId, email), CorrelationContext.Empty);
            return new ApiResponse("Successfully deleted user");
        }

        ///<summary>
        /// Requests a password reset
        ///</summary>
        ///<param name="passwordResetModel">The model necessary to initiate a password reset</param>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [HttpPut("/users/password-reset")]
        [Authorize(Policy = "TenantIdResolved")]
        public async Task<ApiResponse> PasswordResetRequest(PasswordResetModel passwordResetModel)
        {
            var apiResponse = await identityClient.RequestPasswordReset(TenantId, UserFromClaims.Email, JsonConvert.SerializeObject(passwordResetModel));
            return new ApiResponse("Successfully requested password reset");
        }

        ///<summary>
        /// Requests a password reset
        ///</summary>
        ///<param name="emailChangeModel">The model necessary to initiate an email change</param>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [HttpPut("/users/email-change")]
        [Authorize(Policy = "TenantIdResolved")]
        public async Task<ApiResponse> EmailChangeRequest(EmailChangeModel emailChangeModel)
        {
            var apiResponse = await identityClient.RequestEmailChange(TenantId, UserFromClaims.Email, JsonConvert.SerializeObject(emailChangeModel));
            return new ApiResponse("Successfully requested email change");
        }

        ///<summary>
        /// Anonymous method to set a user's new password
        ///</summary>
        ///<param name="email">The email of the user whose password will be set</param>
        ///<param name="confirmModel">The model necessary to set a new password</param>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [AllowAnonymous]
        [HttpPost("/users/{email}/password-reset")]
        public async Task<ApiResponse> PasswordReset(string email, UserConfirmPasswordResetModel confirmModel)
        {
            var tenantApiResponse = await tenantsClient.GetTenantByDomainAsync<ContextTenant>(confirmModel.Domain);
            var requestContent = JsonConvert.SerializeObject(new
            {
                NewPassword = confirmModel.NewPassword,
                ConfirmPassword = confirmModel.ConfirmPassword,
                Token = confirmModel.Token
            });
            var apiResponse = await identityClient.UserConfirmPasswordResetAsync(tenantApiResponse.Result.TenantId, email, requestContent);
            return new ApiResponse("Successfully set new password");
        }

        ///<summary>
        /// Anonymous method to set a user's new password
        ///</summary>
        ///<param name="email">The email of the user whose email will be changed</param>
        ///<param name="confirmModel">The model necessary to change a user's email address</param>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [HttpPost("/users/{email}/email-change")]
        [AllowAnonymous]
        public async Task<ApiResponse> EmailChange(string email, UserConfirmEmailChangeModel confirmModel)
        {
            var tenantApiResponse = await tenantsClient.GetTenantByDomainAsync<ContextTenant>(confirmModel.Domain);
            var requestContent = new
            {
                Email = confirmModel.Email,
                Token = confirmModel.Token
            };
            var apiResponse = await identityClient.UserConfirmEmailChangeAsync(tenantApiResponse.Result.TenantId, email, JsonConvert.SerializeObject(requestContent));
            return new ApiResponse("Successfully set new email address");
        }

        ///<summary>
        /// Get the projects a user is authorized for
        ///</summary>
        ///<param name="email">The email of the user to get authorized projects for</param>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [HttpGet("/users/{email}/authorized-projects")]
        [Authorize(Policy = "TenantIdResolved")]
        public async Task<ApiResponse> GetAuthorizedProjectsForUser(string email)
        {
            var apiResponse = (await projectsClient.GetAllProjectsForUserAsync<IEnumerable<ProjectModel>>(TenantId, email));
            return new ApiResponse("Successfully retrieved authorized projects", apiResponse.Result.Select(_ => _.ProjectId));
        }

        ///<summary>
        /// Confirms a new user
        ///</summary>
        ///<param name="email">The email of the user should be confirmed</param>
        ///<param name="confirmModel">The model necessary to confirm a new user</param>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [AllowAnonymous]
        [HttpPut("/users/{email}/confirm")]
        public async Task<ApiResponse> ConfirmUser(string email, UserConfirmModel confirmModel)
        {
            var tenantApiResponse = await tenantsClient.GetTenantByDomainAsync<ContextTenant>(confirmModel.Domain);
            var requestContent = JsonConvert.SerializeObject(new
            {
                NewPassword = confirmModel.NewPassword,
                ConfirmPassword = confirmModel.ConfirmPassword,
                Token = confirmModel.Token
            });
            var apiResponse = await identityClient.ConfirmUserAsync(tenantApiResponse.Result.TenantId, email, requestContent);
            return new ApiResponse("Successfully confirmed new user");
        }

        ///<summary>
        /// Gets a user
        ///</summary>
        ///<param name="email">The email of the user</param>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet("/users/{email}")]
        [Authorize(Policy = "TenantIdResolved")]
        public async Task<ApiResponse> GetUser(string email)
        {
            var apiResponse = await identityClient.GetUserAsync<UserApiResponseModel>(TenantId, email);
            var userResponseModel = new UserApiResponseModel
            {
                Email = apiResponse.Result.Email,
                FirstName = apiResponse.Result.FirstName,
                LastName = apiResponse.Result.LastName,
                Role = apiResponse.Result.Role,
            };
            return new ApiResponse("Successfully retrieved user", userResponseModel);
        }

        ///<summary>
        /// Gets all users
        ///</summary>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet("/users")]
        [Authorize(Policy = "TenantIdResolved")]
        public async Task<ApiResponse> GetAllUsers()
        {
            var apiResponse = await identityClient.GetAllUsersAsync<IEnumerable<UserApiResponseModel>>(TenantId);
            var userResponseCollection = new List<UserApiResponseModel>();
            foreach (var user in apiResponse.Result)
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
            return new ApiResponse("Successfully retrieved all users", userResponseCollection);
        }

        ///<summary>
        /// Creates a new user
        ///</summary>
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [HttpPost("/users")]
        [Authorize(Policy = "TenantIdResolved")]
        public async Task<ApiResponse> CreateUser(PostApplicationUserModel postApplicationUserModel)
        {
            var applicationUserCommand = new CreateUserSagaInitializer(
                TenantId,
                postApplicationUserModel.Email,
                postApplicationUserModel.FirstName,
                postApplicationUserModel.LastName,
                postApplicationUserModel.Role,
                UserFromClaims.Email,
                postApplicationUserModel.AuthorizedProjects
            );
            return await Task.Run(() => base.Send(applicationUserCommand));
        }

        ///<summary>
        /// Updates a users permissions
        ///</summary>
        ///<param name="email">The email of the user</param>
        ///<param name="putPermissionsModel">The model necessary to update the user's permissions</param>
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [HttpPut("/users/{email}")]
        [Authorize(Policy = "TenantIdResolved")]
        public async Task<ApiResponse> PutPermissions(string email, PutPermissionsModel putPermissionsModel)
        {
            var updateUserPermissionsCommand = new UpdateUserPermissionsSagaInitializer(
                TenantId,
                email,
                UserFromClaims.Email,
                putPermissionsModel.Role,
                putPermissionsModel.AuthorizedProjects
            );
            return await Task.Run(() => base.Send(updateUserPermissionsCommand));
        }
    }
}