using System;
using System.Collections.Generic;
using Ranger.Common;

namespace Ranger.ApiGateway
{
    public class PutPermissionsModel
    {
        public RolesEnum Role { get; set; }
        public IList<Guid> AuthorizedProjects { get; set; } = new List<Guid>();
    }
}