using System;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PusherServer;
using Ranger.Common;
using Ranger.InternalHttpClient;
using Ranger.RabbitMQ;

namespace Ranger.ApiGateway
{
    [ApiController]
    [Authorize]
    [ApiVersion("1.0")]
    public class PusherController : BaseController<PusherController>
    {
        private readonly IIdentityClient identityClient;
        private readonly ILogger<PusherController> logger;
        private readonly RangerPusherOptions pusherOptions;
        private readonly IBusPublisher busPublisher;

        public PusherController(IBusPublisher busPublisher, IIdentityClient identityClient, RangerPusherOptions pusherOptions, ILogger<PusherController> logger) : base(busPublisher, logger)
        {
            this.busPublisher = busPublisher;
            this.pusherOptions = pusherOptions;
            this.identityClient = identityClient;
            this.logger = logger;
        }

        [HttpPost("/pusher/auth")]
        public async Task<IActionResult> Index([FromForm] PusherAuthModel pusherAuthModel)
        {
            User user = null;
            try
            {
                user = await this.identityClient.GetUserAsync<User>(UserFromClaims.Domain, pusherAuthModel.userEmail);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "An exception occurred retrieving the ContextTenant object. Cannot construct the tenant specific repository.");
                return StatusCode((int)StatusCodes.Status500InternalServerError);
            }

            if (user != null && user.Email == UserFromClaims.Email)
            {
                var pusher = new Pusher(pusherOptions.AppId, pusherOptions.Key, pusherOptions.Secret, new PusherOptions { Cluster = pusherOptions.Cluster, Encrypted = bool.Parse(pusherOptions.Encrypted) });
                return Ok(pusher.Authenticate(pusherAuthModel.channel_name, pusherAuthModel.socket_id));
            }
            return Forbid();
        }
    }
}