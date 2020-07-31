using System;
using System.Collections.Generic;
using Ranger.Common;

namespace Ranger.ApiGateway
{
    public class PostApplicationUserModel
    {
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public RolesEnum Role { get; set; }
        public IList<Guid> AuthorizedProjects { get; set; } = new List<Guid>();
    }
}