using System.ComponentModel.DataAnnotations;

namespace Ranger.ApiGateway
{
    public class ContactFormModel
    {
        public string Organization { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Message { get; set; }
    }
}