using System;
using Ranger.Common;

namespace Ranger.ApiGateway {
    public class TenantsResponseModel {
        public int Id { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? LastAccessed { get; set; }

        public string Name { get; set; }

        public string Domain { get; set; }

        public string ConnectionString { get; set; }

        public string DomainPassword { get; set; }

        public string RegistrationKey { get; set; }
        public bool DomainConfirmed { get; set; }
    }
}