using System;

namespace Ranger.ApiGateway
{
    public class ProjectAuthenticationResult
    {
        public Guid ProjectId { get; set; }
        public string Name { get; set; }
        public bool Enabled { get; set; }
    }
}