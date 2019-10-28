using System.ComponentModel.DataAnnotations;

namespace Ranger.ApiGateway
{
    public class ApiKeyResetModel
    {
        [Required]
        public int Version { get; set; }
    }
}