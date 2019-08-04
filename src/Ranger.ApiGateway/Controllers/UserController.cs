using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Ranger.InternalHttpClient;
using Ranger.RabbitMQ;

namespace Ranger.ApiGateway {
    [ApiController]
    [TenantDomainRequired]
    [Authorize (Roles = "Admin")]
    public class UserController : BaseController {
        private readonly IIdentityClient identityClient;

        public UserController (IBusPublisher busPublisher, IIdentityClient identityClient, ILogger<UserController> logger) : base (busPublisher, logger) {
            this.identityClient = identityClient;
        }

        [HttpGet ("/user/{email}")]
        public async Task<IActionResult> Index (string email) {
            ApplicationUserApiResponseModel applicationUserApiResponse = null;
            try {
                applicationUserApiResponse = await identityClient.GetAllUsersAsync<ApplicationUserApiResponseModel> (Domain);
            } catch (Exception ex) {
                Logger.LogError (ex, $"An exception occurred retrieving the retrieving the application user for email '{email}'.");
                return InternalServerError ($"An error occurred retrieving the retrieving the application user for email '{email}'.");
            }

            var userResponseModel = new ApplicationUserApiResponseModel {
                Email = applicationUserApiResponse.Email,
                FirstName = applicationUserApiResponse.FirstName,
                LastName = applicationUserApiResponse.LastName,
                Role = applicationUserApiResponse.Role,
            };
            return Ok (userResponseModel);
        }

        [HttpGet ("/user/all")]
        public async Task<IActionResult> All () {
            IEnumerable<ApplicationUserApiResponseModel> applicationUserApiResponse = null;
            try {
                applicationUserApiResponse = await identityClient.GetAllUsersAsync<IEnumerable<ApplicationUserApiResponseModel>> (Domain);
            } catch (Exception ex) {
                Logger.LogError (ex, $"An exception occurred retrieving the retrieving the application users.");
                return InternalServerError ($"An error occurred retrieving the retrieving the application users.");
            }

            var userResponseCollection = new List<ApplicationUserApiResponseModel> ();
            foreach (var user in applicationUserApiResponse) {
                var userResponseModel = new ApplicationUserApiResponseModel {
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Role = user.Role,
                };
                userResponseCollection.Add (userResponseModel);
            }
            return Ok (userResponseCollection);
        }

        [HttpPost ("/user")]
        public async Task<IActionResult> Post (ApplicationUserModel userModel) {
            return Ok ();
        }
    }
}