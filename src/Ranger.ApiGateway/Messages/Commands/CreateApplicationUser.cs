using System.Collections.Generic;
using Ranger.RabbitMQ;

namespace Ranger.ApiGateway
{
    [MessageNamespaceAttribute("identity")]
    public class CreateApplicationUser : ICommand
    {
        public string Domain { get; set; }
        public string Email { get; }
        public string FirstName { get; }
        public string LastName { get; }
        public string Role { get; }
        public IEnumerable<string> PermittedProjectIds { get; }

        public CreateApplicationUser(string domain, string email, string firstName, string lastName, string role, IEnumerable<string> permittedProjectIds)
        {
            this.Domain = domain;
            this.Email = email;
            this.FirstName = firstName;
            this.LastName = lastName;
            this.Role = role;
            this.PermittedProjectIds = permittedProjectIds;
        }
    }
}