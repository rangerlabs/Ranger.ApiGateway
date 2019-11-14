using System.ComponentModel.DataAnnotations;

namespace Ranger.ApiGateway
{
    public class PusherAuthModel
    {
        [Required]
        public string socket_id { get; set; }
        [Required]
        public string channel_name { get; set; }
        [Required]
        public string userEmail { get; set; }
    }
}