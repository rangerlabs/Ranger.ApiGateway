using System;

namespace Ranger.ApiGateway
{
    public class RangerChargeBeePortalSession
    {
        public string Id { get; set; }
        public string Token { get; set; }
        public string AccessUrl { get; set; }
        public string Status { get; set; }
        public string Object { get; set; }
        public string CustomerId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
    }

}