using System;
using System.Threading.Tasks;
using ChargeBee.Api;
using ChargeBee.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Ranger.Common;
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
        public async Task<IActionResult> GetAllProjects([FromQuery] string planId)
        {
            if (String.IsNullOrWhiteSpace(planId))
            {
                var apiErrorContent = new ApiErrorContent();
                apiErrorContent.Errors.Add($"{nameof(planId)} was missing from the query parameters.");
                return BadRequest(apiErrorContent);
            }

            try
            {
                var hostedPage = await subscriptionsClient.GenerateCheckoutExistingUrl<RangerChargeBeeHostedPage>(UserFromClaims.Domain, planId);
                return Ok(hostedPage);
            }
            catch (HttpClientException<RangerChargeBeeHostedPage> ex)
            {
                logger.LogError(ex, "Failed to retrieve hosted page url.");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpGet("/subscriptions/plan-id")]
        public async Task<IActionResult> GetPlanId()
        {
            try
            {
                var subscription = await subscriptionsClient.GetSubscriptionPlanId<Subscription>(UserFromClaims.Domain);
                return Ok(subscription);
            }
            catch (HttpClientException<Subscription> ex)
            {
                logger.LogError(ex, "Failed to retrieve subscription Plan Id.");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpGet("/subscriptions/limit-details")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> GetLimitDetails()
        {
            try
            {
                var limitDetails = await subscriptionsClient.GetLimitDetails<SubscriptionLimitDetails>(UserFromClaims.Domain);
                return Ok(limitDetails);
            }
            catch (HttpClientException<Subscription> ex)
            {
                logger.LogError(ex, "Failed to retrieve limit details.");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
    }
}