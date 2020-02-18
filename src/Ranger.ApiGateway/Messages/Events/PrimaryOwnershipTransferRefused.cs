using Ranger.RabbitMQ;

namespace Ranger.ApiGateway
{
    [MessageNamespace("operations")]
    public class PrimaryOwnershipTransferRefused : IEvent
    {
        public PrimaryOwnershipTransferRefused()
        { }
    }
}