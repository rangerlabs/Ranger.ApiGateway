using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Ranger.Common;
using Ranger.InternalHttpClient;

namespace Ranger.ApiGateway {
    [ApiController]
    [Authorize (Roles = "Admin")]
    public class UserController : ControllerBase {
        private readonly IIdentityClient identityClient;

        public UserController (IIdentityClient identityClient) {
            this.identityClient = identityClient;
        }

        [HttpGet ("/user")]
        public async Task<IActionResult> Index (string email) {
            IActionResult response = new StatusCodeResult (StatusCodes.Status500InternalServerError);
            await identityClient.SetClientToken ();

            StringValues domain;
            bool success = Request.Headers.TryGetValue ("X-Tenant-Domain", out domain);
            if (success) {
                if (domain.Count == 1) {
                    var apiResponse = await identityClient.GetAllUsersAsync<ApplicationUserApiResponseModel> (domain);
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
                } else {
                    response = BadRequest (new { errors = new { serverErrors = "Multiple header values were found for X-Tenant-Domain." } });
                }
            } else {
                response = BadRequest (new { errors = new { serverErrors = "No domain header value was found." } });
            }
            return response;
        }

        [HttpGet ("/user/all")]
        public async Task<IActionResult> All () {
            IActionResult response = new StatusCodeResult (StatusCodes.Status500InternalServerError);
            await identityClient.SetClientToken ();

            StringValues domain;
            bool success = Request.Headers.TryGetValue ("X-Tenant-Domain", out domain);
            if (success) {
                if (domain.Count == 1) {
                    var apiResponse = await identityClient.GetAllUsersAsync<IEnumerable<ApplicationUserApiResponseModel>> (domain);
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
                } else {
                    response = BadRequest (new { errors = new { serverErrors = "Multiple header values were found for X-Tenant-Domain." } });
                    throw new DomainNotFoundException ();
                }
            } else {
                response = BadRequest (new { errors = new { serverErrors = "No domain header value was found." } });
            }
            return response;
        }

        [HttpPost ("/user")]
        public async Task<IActionResult> Post (ApplicationUserModel userModel) {
            if (userModel == null) {
                throw new ArgumentNullException (nameof (userModel));
            }

            IActionResult response = new StatusCodeResult (StatusCodes.Status500InternalServerError);

            return response;
        }
    }
}