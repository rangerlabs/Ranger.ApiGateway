using System;

namespace Ranger.ApiGateway
{
    public class PrimaryOwnershipAcceptModel
    {
        public Guid CorrelationId { get; set; }
        public string Token { get; set; }
    }
}