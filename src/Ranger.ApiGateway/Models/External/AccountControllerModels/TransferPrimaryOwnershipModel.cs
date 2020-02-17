using System.ComponentModel.DataAnnotations;

namespace Ranger.ApiGateway
{
    public class TransferPrimaryOwnershipModel
    {
        [EmailAddress]
        public string Email { get; set; }
    }
}