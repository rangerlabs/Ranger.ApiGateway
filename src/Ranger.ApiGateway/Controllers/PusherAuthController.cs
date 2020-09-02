using System;
using System.Linq;
using System.Threading.Tasks;
using AutoWrapper.Filters;
using AutoWrapper.Wrappers;
using IdentityServer4;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PusherServer;
using Ranger.Common;
using Ranger.InternalHttpClient;
using Ranger.RabbitMQ;
using Ranger.RabbitMQ.BusPublisher;

namespace Ranger.ApiGateway
{
    [ApiController]
    [ApiVersion("1.0")]
    [Authorize(Policy = AuthorizationPolicyNames.TenantIdResolved)]
    [AutoWrapIgnore]
    public class PusherController : BaseController
    {
        private readonly IdentityHttpClient identityClient;
        private readonly ILogger<PusherController> logger;
        private readonly RangerPusherOptions pusherOptions;
        private readonly IBusPublisher busPublisher;

        public PusherController(IBusPublisher busPublisher, IdentityHttpClient identityClient, RangerPusherOptions pusherOptions, ILogger<PusherController> logger) : base(busPublisher, logger)
        {
            this.busPublisher = busPublisher;
            this.pusherOptions = pusherOptions;
            this.identityClient = identityClient;
            this.logger = logger;
        }
        ///<summary>
        /// Authenticates a user to connect to the requested channel
        ///</summary>
        ///<param name="pusherAuthModel">The model necessary to validate pusher channel auth</param>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [HttpPost("/pusher/auth")]
        public async Task<IActionResult> Index([FromForm] PusherAuthModel pusherAuthModel)
        {
            var apiResponse = await this.identityClient.GetUserAsync<User>(TenantId, pusherAuthModel.userEmail);
            if (apiResponse.Result.Email == UserFromClaims.Email)
            {
                var pusher = new Pusher(pusherOptions.AppId, pusherOptions.Key, pusherOptions.Secret, new PusherOptions { Cluster = pusherOptions.Cluster, Encrypted = bool.Parse(pusherOptions.Encrypted) });
                return Ok(pusher.Authenticate(pusherAuthModel.channel_name, pusherAuthModel.socket_id));
            }
            return Forbid();
        }
    }
}