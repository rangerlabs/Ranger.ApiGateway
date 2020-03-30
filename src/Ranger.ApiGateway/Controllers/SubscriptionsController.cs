using System.Threading.Tasks;
using ChargeBee.Api;
using ChargeBee.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Ranger.InternalHttpClient;
using Ranger.RabbitMQ;

namespace Ranger.ApiGateway.Controllers
{
    [ApiVersion("1.0")]
    [ApiController]
    [Authorize(Roles = "Owner")]
    public class SubscriptionsController : BaseController<SubscriptionsController>
    {
        private readonly ISubscriptionsClient subscriptionsClient;
        private readonly ILogger<SubscriptionsController> logger;
        public SubscriptionsController(IBusPublisher busPublisher, ISubscriptionsClient subscriptionsClient, ILogger<SubscriptionsController> logger) : base(busPublisher, logger)
        {
            this.logger = logger;
            this.subscriptionsClient = subscriptionsClient;
        }

        [HttpGet("/subscriptions/checkout-existing-hosted-page-url")]
        public async Task<IActionResult> GetAllProjects()
        {
            try
            {
                var hostedPage = await subscriptionsClient.GenerateCheckoutExistingUrl<RangerChargeBeeHostedPage>(UserFromClaims.Domain);
                return Ok(hostedPage);
            }
            catch (HttpClientException<EntityResult> ex)
            {
                logger.LogError(ex, "Failed to retrieve hosted page url.");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
    }
}