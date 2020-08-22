using System;

namespace Ranger.ApiGateway
{
    class ProjectModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public bool Enabled { get; set; }
    }
}