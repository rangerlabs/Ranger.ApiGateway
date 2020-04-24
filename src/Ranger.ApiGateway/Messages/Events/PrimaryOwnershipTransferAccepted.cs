using Ranger.RabbitMQ;

namespace Ranger.ApiGateway
{
    [MessageNamespace("operations")]
    public class AcceptPrimaryOwnershipTransfer : ICommand
    {
        public AcceptPrimaryOwnershipTransfer(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                throw new System.ArgumentException($"{nameof(token)} was null or whitespace");
            }

            this.Token = token;

        }
        public string Token { get; set; }
    }
}