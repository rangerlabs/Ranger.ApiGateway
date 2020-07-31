using System.ComponentModel.DataAnnotations;

namespace Ranger.ApiGateway
{
    public class UserConfirmEmailChangeModel
    {
        public string Domain { get; set; }
        public string Token { get; set; }
        public string Email { get; set; }
    }
}