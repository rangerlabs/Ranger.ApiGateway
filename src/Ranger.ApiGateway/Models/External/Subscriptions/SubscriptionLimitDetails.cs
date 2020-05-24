using System;

namespace Ranger.ApiGateway
{
    public class SubscriptionLimitDetails
    {
        public string PlanId { get; set; }
        public LimitFields Utilized { get; set; }
        public LimitFields Limit { get; set; }
        public bool Active { get; set; }
        public DateTime? ScheduledCancellationDate { get; set; }
    }
    public class LimitFields
    {
        public int Geofences { get; set; }
        public int Integrations { get; set; }
        public int Projects { get; set; }
        public int Accounts { get; set; }
    }
}