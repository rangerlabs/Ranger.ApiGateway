using System;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PusherServer;
using Ranger.ApiUtilities;
using Ranger.Common;
using Ranger.InternalHttpClient;

namespace Ranger.ApiGateway
{
    [ApiController]
    [TenantDomainRequired]
    [Authorize]
    [ApiVersion("1.0")]
    public class PusherController : ControllerBase
    {
        private readonly IIdentityClient identityClient;
        private readonly ILogger<UserController> logger;

        public PusherController(IIdentityClient identityClient, ILogger<UserController> logger)
        {
            this.identityClient = identityClient;
            this.logger = logger;
        }

        [HttpPost("/pusher/auth")]
        public async Task<IActionResult> Index([FromForm]PusherAuthModel pusherAuthModel)
        {
            var domain = HttpContext.Request.Headers.GetPreviouslyVerifiedTenantHeader();
            User user = null;
            try
            {
                user = await this.identityClient.GetUserAsync<User>(domain, pusherAuthModel.userEmail);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "An exception occurred retrieving the ContextTenant object. Cannot construct the tenant specific repository.");
                return StatusCode((int)StatusCodes.Status500InternalServerError);
            }

            if (user != null && user.Email == User.UserFromClaims().Email)
            {
                var options = new PusherOptions
                {
                    Cluster = "us2",
                    Encrypted = true
                };

                var pusher = new Pusher(
                    "828034",
                    "aed7ba7c7247aca9680e",
                    "df532af7ccf602593aa5",
                    options);

                return Ok(pusher.Authenticate(pusherAuthModel.channel_name, pusherAuthModel.socket_id));
            }
            return Forbid();
        }
    }
}