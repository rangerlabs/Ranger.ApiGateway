using System;
using System.Collections.Generic;
using Ranger.Common;
using Ranger.RabbitMQ;

namespace Ranger.ApiGateway
{
    [MessageNamespace("operations")]
    public class UpdateUserPermissionsSagaInitializer : SagaInitializer, ICommand
    {
        public string Email { get; }
        public string CommandingUserEmail { get; }
        public RolesEnum Role { get; }
        public IEnumerable<Guid> AuthorizedProjects { get; }

        public UpdateUserPermissionsSagaInitializer(string tenantid, string email, string commandingUserEmail, RolesEnum role, IEnumerable<Guid> authorizedProjects = null)
        {
            if (string.IsNullOrWhiteSpace(tenantid))
            {
                throw new System.ArgumentException($"{nameof(tenantid)} was null or whitespace");
            }

            if (string.IsNullOrWhiteSpace(email))
            {
                throw new System.ArgumentException($"{nameof(email)} was null or whitespace");
            }

            if (string.IsNullOrWhiteSpace(commandingUserEmail))
            {
                throw new System.ArgumentException($"{nameof(commandingUserEmail)} was null or whitespace");
            }

            this.Email = email;
            this.CommandingUserEmail = commandingUserEmail;
            TenantId = tenantid;
            this.Role = role;
            this.AuthorizedProjects = authorizedProjects;
        }
    }
}