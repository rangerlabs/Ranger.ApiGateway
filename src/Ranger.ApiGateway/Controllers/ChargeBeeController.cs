using System.Threading.Tasks;
using ChargeBee.Models;
using ChargeBee.Models.Enums;
using Microsoft.AspNetCore.Mvc;

namespace Ranger.ApiGateway.Controllers {
    [ApiController]
    public class ChargeBeeController {
        public ChargeBeeController () { }

        [HttpPost ("/chargebee/webhook")]
        public async Task Webhook (Event chargeBeeEvent) {
            var b = EventTypeEnum.SubscriptionChanged;
        }
    }
}