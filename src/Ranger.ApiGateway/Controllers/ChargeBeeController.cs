using System;
using System.Threading.Tasks;
using AutoWrapper.Wrappers;
using ChargeBee.Models;
using ChargeBee.Models.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
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
            Event chargeBeeEvent = null;
            string subscriptionId = "";
            try
            {
                chargeBeeEvent = new Event(Request.Body);
                subscriptionId = chargeBeeEvent.Content.Subscription.Id;
            }
            catch (Exception)
            {
                return new ApiResponse("The event did not contain subscription content", statusCode: StatusCodes.Status200OK);
            }
            var apiResponse = await subscriptionsHttpClient.GetTenantIdForSubscriptionId(subscriptionId);

            return chargeBeeEvent.EventType switch
            {
                EventTypeEnum.SubscriptionChanged => base.Send(new UpdateSubscriptionSagaInitializer(apiResponse.Result, subscriptionId, chargeBeeEvent.Content.Subscription.PlanId, EventTypeEnum.SubscriptionChanged)),
                EventTypeEnum.SubscriptionPaused => base.Send(new UpdateSubscriptionSagaInitializer(apiResponse.Result, subscriptionId, chargeBeeEvent.Content.Subscription.PlanId, EventTypeEnum.SubscriptionPaused, false)),
                EventTypeEnum.SubscriptionResumed => base.Send(new UpdateSubscriptionSagaInitializer(apiResponse.Result, subscriptionId, chargeBeeEvent.Content.Subscription.PlanId, EventTypeEnum.SubscriptionResumed)),
                EventTypeEnum.SubscriptionCancelled => base.Send(new UpdateSubscriptionSagaInitializer(apiResponse.Result, subscriptionId, chargeBeeEvent.Content.Subscription.PlanId, EventTypeEnum.SubscriptionCancelled, false)),
                EventTypeEnum.SubscriptionReactivated => base.Send(new UpdateSubscriptionSagaInitializer(apiResponse.Result, subscriptionId, chargeBeeEvent.Content.Subscription.PlanId, EventTypeEnum.SubscriptionReactivated)),
                EventTypeEnum.SubscriptionCancellationReminder => base.Send(new UpdateSubscriptionSagaInitializer(apiResponse.Result, subscriptionId, chargeBeeEvent.Content.Subscription.PlanId, EventTypeEnum.SubscriptionCancellationReminder, scheduledCancellationDate: DateTime.Now.AddDays(6))), //per chargebee api docs
                EventTypeEnum.SubscriptionScheduledCancellationRemoved => base.Send(new UpdateSubscriptionSagaInitializer(apiResponse.Result, subscriptionId, chargeBeeEvent.Content.Subscription.PlanId, EventTypeEnum.SubscriptionScheduledCancellationRemoved)),
                _ => new ApiResponse("The webhook event was not a subscription event", statusCode: StatusCodes.Status200OK)
            };
        }
    }
}