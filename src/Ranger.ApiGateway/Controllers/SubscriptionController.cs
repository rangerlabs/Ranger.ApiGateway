using System;
using System.Threading;
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
    [Authorize(Policy = AuthorizationPolicyNames.TenantIdResolved)]
    public class SubscriptionController : BaseController<SubscriptionController>
    {
        private readonly SubscriptionsHttpClient subscriptionsClient;
        private readonly ILogger<SubscriptionController> logger;
        public SubscriptionController(IBusPublisher busPublisher, SubscriptionsHttpClient subscriptionsClient, ILogger<SubscriptionController> logger) : base(busPublisher, logger)
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
        [Authorize(Roles = "Owner")]
        [HttpGet("/subscriptions/checkout-existing-hosted-page-url")]
        public async Task<ApiResponse> GetHostedCheckoutPageUrl([FromQuery] string planId, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(planId))
            {
                throw new ApiException("The 'planId' query parameter is required", StatusCodes.Status400BadRequest);
            }

            var apiResponse = await subscriptionsClient.GenerateCheckoutExistingUrl<RangerChargeBeeHostedPage>(TenantId, planId, cancellationToken);
            return new ApiResponse("Successfully retrieved hosted checkout page url", apiResponse.Result);
        }

        ///<summary>
        /// Get hosted portal session
        ///</summary>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Authorize(Roles = "Owner")]
        [HttpGet("/subscriptions/portal-session")]
        public async Task<ApiResponse> GetPortalSession(CancellationToken cancellationToken)
        {
            var apiResponse = await subscriptionsClient.GetPortalSession<RangerChargeBeePortalSession>(TenantId, cancellationToken);
            return new ApiResponse("Successfully retrieved portal session", apiResponse.Result);
        }

        ///<summary>
        /// Get tenant's plan id
        ///</summary>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Authorize(Roles = "Owner")]
        [HttpGet("/subscriptions/plan-id")]
        public async Task<ApiResponse> GetPlanId(CancellationToken cancellationToken)
        {
            var apiResponse = await subscriptionsClient.GetSubscriptionPlanId(TenantId, cancellationToken);
            return new ApiResponse("Successfully retrieved plan id", apiResponse.Result);
        }

        ///<summary>
        /// Get tenant's subscription details
        ///</summary>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet("/subscriptions")]
        [Authorize(Roles = "User")]
        public async Task<ApiResponse> GetSubscription()
        {
            var apiResponse = await subscriptionsClient.GetSubscription<SubscriptionLimitDetails>(TenantId);
            var result = new
            {
                PlanId = apiResponse.Result.PlanId,
                Utilized = apiResponse.Result.Utilized,
                Limit = apiResponse.Result.Limit,
                Active = apiResponse.Result.Active,
                DaysUntilCancellation = apiResponse.Result.ScheduledCancellationDate.HasValue ? (int?)apiResponse.Result.ScheduledCancellationDate.Value.Subtract(DateTime.Now).Days : null
            };
            return new ApiResponse("Successfully retrieved tenant subscription ", result);
        }
    }
}