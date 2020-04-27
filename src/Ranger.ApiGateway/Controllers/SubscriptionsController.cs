using System.Threading.Tasks;
using AutoWrapper.Wrappers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Ranger.InternalHttpClient;
using Ranger.RabbitMQ;

namespace Ranger.ApiGateway.Controllers
{
    [ApiVersion("1.0")]
    [ApiController]
    [Authorize(Roles = "Owner")]
    [Authorize(Policy = "TenantIdResolved")]
    public class SubscriptionsController : BaseController<SubscriptionsController>
    {
        private readonly SubscriptionsHttpClient subscriptionsClient;
        private readonly ILogger<SubscriptionsController> logger;
        public SubscriptionsController(IBusPublisher busPublisher, SubscriptionsHttpClient subscriptionsClient, ILogger<SubscriptionsController> logger) : base(busPublisher, logger)
        {
            this.logger = logger;
            this.subscriptionsClient = subscriptionsClient;
        }

        ///<summary>
        /// Get hosted checkout page url
        ///</summary>
        ///<param name="planId">The plan id to retrieve the hosted page for</param>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet("/subscriptions/checkout-existing-hosted-page-url")]
        public async Task<ApiResponse> GetHostedCheckoutPageUrl([FromQuery]string planId)
        {
            if (string.IsNullOrWhiteSpace(planId))
            {
                throw new ApiException("The 'planId' query parameter is required", StatusCodes.Status400BadRequest);
            }

            var apiResponse = await subscriptionsClient.GenerateCheckoutExistingUrl(TenantId, planId);
            return new ApiResponse("Successfully retrieved hosted checkout page url", apiResponse.Result);
        }

        ///<summary>
        /// Get tenant's plan id
        ///</summary>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet("/subscriptions/plan-id")]
        public async Task<ApiResponse> GetPlanId()
        {
            var apiResponse = await subscriptionsClient.GetSubscriptionPlanId(TenantId);
            return new ApiResponse("Successfully retrieved plan id", apiResponse.Result);
        }

        ///<summary>
        /// Get tenant's subscription details
        ///</summary>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet("/subscriptions/limit-details")]
        [Authorize(Roles = "User")]
        public async Task<ApiResponse> GetLimitDetails()
        {
            var apiResponse = await subscriptionsClient.GetLimitDetails<SubscriptionLimitDetails>(TenantId);
            return new ApiResponse("Successfully retrieved tenant limit details", apiResponse.Result);
        }
    }
}