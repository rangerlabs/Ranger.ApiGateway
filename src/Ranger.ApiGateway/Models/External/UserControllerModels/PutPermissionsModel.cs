using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Ranger.Common;

namespace Ranger.ApiGateway
{
    public class PutPermissionsModel
    {
        [EnumDataType(typeof(RolesEnum))]
        public string Role { get; set; }

        public IList<Guid> AuthorizedProjects { get; set; } = new List<Guid>();
    }
}