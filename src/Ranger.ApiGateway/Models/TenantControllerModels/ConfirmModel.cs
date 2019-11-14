using System.ComponentModel.DataAnnotations;

namespace Ranger.ApiGateway
{
    public class TenantConfirmModel
    {
        [Required]
        public string RegistrationKey { get; set; }
        public string Domain { get; set; }
    }
}