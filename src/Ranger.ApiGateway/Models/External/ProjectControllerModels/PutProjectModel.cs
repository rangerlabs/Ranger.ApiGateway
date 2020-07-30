using System.ComponentModel.DataAnnotations;

namespace Ranger.ApiGateway
{
    public class PutProjectModel
    {
        public string Name { get; set; }
        public bool Enabled { get; set; }
        public string Description { get; set; }
        public int Version { get; set; }
    }
}