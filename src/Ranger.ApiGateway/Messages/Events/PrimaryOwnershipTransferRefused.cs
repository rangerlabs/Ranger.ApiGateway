using Ranger.RabbitMQ;

namespace Ranger.ApiGateway
{
    [MessageNamespace("operations")]
    public class RefusePrimaryOwnershipTransfer : ICommand
    {
        public RefusePrimaryOwnershipTransfer()
        { }
    }
}