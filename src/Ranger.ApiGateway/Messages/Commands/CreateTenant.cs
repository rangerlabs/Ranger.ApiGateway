using Ranger.RabbitMQ;

namespace Ranger.ApiGateway {

    [MessageNamespace ("tenants")]
    public class CreateTenant : ICommand {
        public CreateTenant (Domain domain, NewPrimaryOwner owner) {
            this.Domain = domain;
            this.Owner = owner;
        }
        public Domain Domain { get; }
        public NewPrimaryOwner Owner { get; }
    }
}