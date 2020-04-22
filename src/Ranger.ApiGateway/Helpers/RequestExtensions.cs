using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Ranger.Common;

namespace Ranger.ApiGateway
{
    public static class RequestExtensions
    {
        public static User UserFromClaims(this ClaimsPrincipal user) => new User(
            user.Claims.SingleOrDefault(c => c.Type == "domain")?.Value,
            user.Claims.SingleOrDefault(c => c.Type == "email")?.Value,
            user.Claims.SingleOrDefault(c => c.Type == "firstName")?.Value,
            user.Claims.SingleOrDefault(c => c.Type == "lastName")?.Value,
            user.Claims.SingleOrDefault(c => c.Type == "phoneNumber")?.Value,
            SystemRole(user.Claims.Where(c => c.Type == "role").Select(c => c.Value))
        );

        private static string SystemRole(IEnumerable<string> roles)
        {
            var PrimaryOwner = Enum.GetName(typeof(RolesEnum), RolesEnum.PrimaryOwner);
            var owner = Enum.GetName(typeof(RolesEnum), RolesEnum.Owner);
            var admin = Enum.GetName(typeof(RolesEnum), RolesEnum.Admin);
            var user = Enum.GetName(typeof(RolesEnum), RolesEnum.User);
            if (roles.Contains(PrimaryOwner))
            {
                return PrimaryOwner;
            }
            if (roles.Contains(owner))
            {
                return owner;
            }
            if (roles.Contains(admin))
            {
                return admin;
            }
            return user;
        }
    }
}