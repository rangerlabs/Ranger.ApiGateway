using Ranger.RabbitMQ;

namespace Ranger.ApiGateway
{
    [MessageNamespace("operations")]
    public class CancelPrimaryOwnershipTransfer : ICommand
    {
        public CancelPrimaryOwnershipTransfer()
        {
        }
    }
}