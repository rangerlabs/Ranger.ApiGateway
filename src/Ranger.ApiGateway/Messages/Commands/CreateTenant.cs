using Ranger.RabbitMQ;

namespace Ranger.ApiGateway {

    [MessageNamespace ("tenants")]
    public class CreateTenant : ICommand {
        public CorrelationContext CorrelationContext { get; set; }
        public string OrganizationName { get; set; }
        public string Domain { get; set; }
    }
}