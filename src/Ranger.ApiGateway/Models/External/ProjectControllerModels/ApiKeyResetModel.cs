using System.ComponentModel.DataAnnotations;

namespace Ranger.ApiGateway
{
    public class ApiKeyResetModel
    {
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Only positive values allowed")]
        public int Version { get; set; }
    }
}