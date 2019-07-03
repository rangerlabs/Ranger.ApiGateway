using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Ranger.InternalHttpClient;

namespace Ranger.ApiGateway {
    [ApiController]
    [Authorize (Roles = "Admin")]
    public class UserController : ControllerBase {
        private readonly IIdentityClient identityClient;

        public UserController (IIdentityClient identityClient) {
            this.identityClient = identityClient;
        }

        [HttpGet ("/app/user")]
        public async Task<IActionResult> Index (string email) {
            IActionResult response = new StatusCodeResult (StatusCodes.Status500InternalServerError);
            await identityClient.SetClientToken ();
            var apiResponse = await identityClient.GetUserAsync<ApplicationUserApiResponseModel> (email);
            if (apiResponse.IsSuccessStatusCode) {
                var userResponseModel = new ApplicationUserApiResponseModel {
                    Email = apiResponse.ResponseObject.Email,
                    FirstName = apiResponse.ResponseObject.FirstName,
                    LastName = apiResponse.ResponseObject.LastName,
                    Role = apiResponse.ResponseObject.Role,
                };
                response = Ok (userResponseModel);
            } else {
                response = BadRequest (new { errors = new { serverErrors = apiResponse.Errors } });
            }
            return response;
        }

        [HttpGet ("/app/user/all")]
        public async Task<IActionResult> All () {
            IActionResult response = new StatusCodeResult (StatusCodes.Status500InternalServerError);
            await identityClient.SetClientToken ();
            var apiResponse = await identityClient.GetAllUsersAsync<IEnumerable<ApplicationUserApiResponseModel>> ();
            if (apiResponse.IsSuccessStatusCode) {
                var userResponseCollection = new List<ApplicationUserApiResponseModel> ();
                foreach (var user in apiResponse.ResponseObject) {
                    var userResponseModel = new ApplicationUserApiResponseModel {
                        Email = user.Email,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        Role = user.Role,
                    };
                    userResponseCollection.Add (userResponseModel);
                }
                response = Ok (userResponseCollection);
            } else {
                response = BadRequest (new { errors = new { serverErrors = apiResponse.Errors } });
            }
            return response;
        }

        [HttpPost ("/app/user")]
        public async Task<IActionResult> Post (ApplicationUserModel userModel) {
            if (userModel == null) {
                throw new ArgumentNullException (nameof (userModel));
            }

            IActionResult response = new StatusCodeResult (StatusCodes.Status500InternalServerError);
            await identityClient.SetClientToken ();

            var apiRoleResponse = await identityClient.GetRoleAsync<Role> (Enum.GetName (typeof (Role), userModel.Role));

            var user = new ApplicationUserRequestModel {
                Email = userModel.Email,
                FirstName = userModel.FirstName,
                LastName = userModel.LastName,
                Role = apiRoleResponse.ResponseObject,
            };
            var passwordHasher = new PasswordHasher<ApplicationUserRequestModel> ();

            // user.PasswordHash = passwordHasher.HashPassword (user, Common.Utilities.Cryptography.Crypto.GenerateSudoRandomPasswordString ());
            user.PasswordHash = passwordHasher.HashPassword (user, "password");
            user.TenantDomain = "testtenant";

            var apiCreateUserResponse = await identityClient.CreateUser (user);
            if (apiCreateUserResponse.IsSuccessStatusCode) {
                var userResponseModel = new ApplicationUserApiResponseModel {
                    Email = userModel.Email,
                    FirstName = userModel.FirstName,
                    LastName = userModel.LastName,
                    Role = Enum.GetName (typeof (Role), userModel.Role),
                };
                response = Created ("/app/user", userResponseModel);
            } else {
                response = BadRequest (new { errors = new { serverErrors = apiCreateUserResponse.Errors } });
            }
            return response;
        }
    }
}