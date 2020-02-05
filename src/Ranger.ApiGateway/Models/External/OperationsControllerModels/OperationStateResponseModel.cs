using System;

namespace Ranger.ApiGateway
{
    public class OperationStateResponseModel
    {
        public Guid Id { get; set; }
        public string State { get; set; }
        public string Inititor { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public string RejectedReason { get; set; }
    }
}