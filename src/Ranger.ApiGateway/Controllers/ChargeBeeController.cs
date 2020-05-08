using System;
using System.IO;
using System.Threading.Tasks;
using AutoWrapper.Wrappers;
using ChargeBee.Models;
using ChargeBee.Models.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Ranger.InternalHttpClient;
using Ranger.RabbitMQ;

namespace Ranger.ApiGateway.Controllers
{

    [ApiController]
    [ApiVersionNeutral]
    public class ChargeBeeController : BaseController<ChargeBeeController>
    {
        private readonly IBusPublisher busPublisher;
        private readonly SubscriptionsHttpClient subscriptionsHttpClient;
        private readonly ILogger<ChargeBeeController> logger;

        public ChargeBeeController(IBusPublisher busPublisher, SubscriptionsHttpClient subscriptionsHttpClient, ILogger<ChargeBeeController> logger)
            : base(busPublisher, logger)
        {
            this.busPublisher = busPublisher;
            this.subscriptionsHttpClient = subscriptionsHttpClient;
            this.logger = logger;
        }

        [HttpPost("/chargebee/webhook")]
        public async Task<ApiResponse> Webhook()
        {
            logger.LogInformation("Received ChargeBee Webhook");

            DateTime occurredAt;
            string subscriptionId;
            string planId;
            string eventType;

            try
            {
                using var streamReader = new StreamReader(Request.Body);
                var content = JToken.Parse(await streamReader.ReadToEndAsync());
                occurredAt = DateTimeOffset.FromUnixTimeSeconds((long)content.SelectToken("occurred_at", true)).UtcDateTime;
                subscriptionId = (string)content.SelectToken("content.subscription.id", true);
                planId = (string)content.SelectToken("content.subscription.plan_id", true);
                eventType = (string)content.SelectToken("event_type", true);
            }
            catch (Exception)
            {
                return new ApiResponse("The event did not contain the necessary content for a subscription", statusCode: StatusCodes.Status200OK);
            }

            var apiResponse = await subscriptionsHttpClient.GetTenantIdForSubscriptionId(subscriptionId);

            return eventType switch
            {
                "subscription_changed" => base.Send(new UpdateSubscription(apiResponse.Result, subscriptionId, planId, occurredAt)),
                "subscription_paused" => base.Send(new UpdateSubscription(apiResponse.Result, subscriptionId, planId, occurredAt, false)),
                "subscription_resumed" => base.Send(new UpdateSubscription(apiResponse.Result, subscriptionId, planId, occurredAt)),
                "subscription_cancelled" => base.Send(new UpdateSubscription(apiResponse.Result, subscriptionId, planId, occurredAt, false)),
                "subscription_reactivated" => base.Send(new UpdateSubscription(apiResponse.Result, subscriptionId, planId, occurredAt)),
                "subscription_cancellation_reminder" => base.Send(new UpdateSubscription(apiResponse.Result, subscriptionId, planId, occurredAt, scheduledCancellationDate: DateTime.Now.AddDays(6))), //per chargebee api docs
                "subscription_scheduled_cancellation_removed" => base.Send(new UpdateSubscription(apiResponse.Result, subscriptionId, planId, occurredAt)),
                _ => new ApiResponse("The webhook event was not a subscription event", statusCode: StatusCodes.Status200OK)
            };
        }
    }
}