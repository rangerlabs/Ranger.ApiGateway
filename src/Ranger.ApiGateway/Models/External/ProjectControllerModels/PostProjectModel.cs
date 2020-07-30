using System.ComponentModel.DataAnnotations;

namespace Ranger.ApiGateway
{
    public class PostProjectModel
    {
        public string Name { get; set; }
        public bool Enabled { get; set; } = true;
        public string Description { get; set; }
    }
}