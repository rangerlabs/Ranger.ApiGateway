using System;
using System.ComponentModel.DataAnnotations;

namespace Ranger.ApiGateway
{
    public class ProjectResponseModel
    {
        public string Domain { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ApiKey { get; set; }
        public int Version { get; set; }
    }
}