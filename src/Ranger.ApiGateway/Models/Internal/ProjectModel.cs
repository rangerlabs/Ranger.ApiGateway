using System;

namespace Ranger.ApiGateway
{
    public class ProjectModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public bool Enabled { get; set; }
    }
}