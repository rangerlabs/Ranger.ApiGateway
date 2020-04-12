using Ranger.RabbitMQ;

namespace Ranger.ApiGateway
{
    public abstract class SagaInitializer
    {
        public string TenantId { get; protected set; }
    }
}