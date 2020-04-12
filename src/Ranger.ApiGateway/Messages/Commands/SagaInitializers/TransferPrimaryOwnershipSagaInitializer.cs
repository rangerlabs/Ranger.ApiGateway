using Ranger.RabbitMQ;

namespace Ranger.ApiGateway
{
    [MessageNamespace("operations")]
    public class TransferPrimaryOwnershipSagaInitializer : SagaInitializer, ICommand
    {
        public TransferPrimaryOwnershipSagaInitializer(string commandingUserEmail,
            string transferUserEmail,
            string tenantid)
        {
            if (string.IsNullOrWhiteSpace(commandingUserEmail))
            {
                throw new System.ArgumentException($"{nameof(commandingUserEmail)} was null or whitespace.");
            }

            if (string.IsNullOrWhiteSpace(transferUserEmail))
            {
                throw new System.ArgumentException($"{nameof(transferUserEmail)} was null or whitespace.");
            }

            if (string.IsNullOrWhiteSpace(tenantid))
            {
                throw new System.ArgumentException($"{nameof(tenantid)} was null or whitespace.");
            }

            CommandingUserEmail = commandingUserEmail;
            TransferUserEmail = transferUserEmail;
            TenantId = tenantid;
        }

        public string CommandingUserEmail { get; private set; }
        public string TransferUserEmail { get; private set; }
    }
}