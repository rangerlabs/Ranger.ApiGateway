using Ranger.RabbitMQ;

namespace Ranger.ApiGateway
{
    public abstract class SagaInitializer
    {
        public string Domain { get; protected set; }
    }
}