using System.Collections;
using System.Collections.Generic;
using Ranger.RabbitMQ;

namespace Ranger.ApiGateway
{
    [MessageNamespace("identity")]
    public class UpdateUserPermissions : ICommand
    {
        public string Domain { get; }
        public string Email { get; }
        public string Role { get; }
        public IEnumerable<string> PermittedProjects { get; }

        public UpdateUserPermissions(string domain, string email, string role = "", IEnumerable<string> permittedProjects = null)
        {
            if (string.IsNullOrWhiteSpace(domain))
            {
                throw new System.ArgumentException($"{nameof(domain)} was null or whitespace.");
            }

            if (string.IsNullOrWhiteSpace(email))
            {
                throw new System.ArgumentException($"{nameof(email)} was null or whitespace.");
            }


            this.Email = email;
            this.Domain = domain;
            this.Role = role;
            this.PermittedProjects = permittedProjects;
        }
    }
}