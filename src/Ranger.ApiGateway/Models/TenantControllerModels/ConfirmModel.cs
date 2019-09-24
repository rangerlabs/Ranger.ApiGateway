using System.ComponentModel.DataAnnotations;

namespace Ranger.ApiGateway
{
    public class ConfirmModel
    {
        [Required]
        public string RegistrationKey { get; set; }
    }
}