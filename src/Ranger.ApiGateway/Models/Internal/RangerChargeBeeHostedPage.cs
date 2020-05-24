using System;

namespace Ranger.ApiGateway
{
    public class RangerChargeBeeHostedPage
    {
        public string Id { get; set; }
        public string Type { get; set; }
        public string Url { get; set; }
        public string State { get; set; }
        public bool Embed { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
    }
}