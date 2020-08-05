using System;
using System.Collections.Generic;
using System.Linq;
using Ranger.Common;
using Ranger.RabbitMQ;

namespace Ranger.ApiGateway
{
    [MessageNamespace("testRunner")]
    public class ExecuteTestRun : ICommand
    {
        public ExecuteTestRun(string tenantId, Guid projectId, string projectName, IEnumerable<Breadcrumb> breadcrumbs)
        {
            if (string.IsNullOrWhiteSpace(tenantId))
            {
                throw new ArgumentException($"'{nameof(tenantId)}' cannot be null or whitespace", nameof(tenantId));
            }

            if (string.IsNullOrWhiteSpace(projectName))
            {
                throw new ArgumentException($"'{nameof(projectName)}' cannot be null or whitespace", nameof(projectName));
            }

            if (breadcrumbs.Count() == 0)
            {
                throw new ArgumentException($"'{nameof(Breadcrumbs)}' cannot be empty");
            }

            this.TenantId = tenantId;
            this.ProjectId = projectId;
            this.ProjectName = projectName;
            this.Breadcrumbs = breadcrumbs;
        }

        public string TenantId { get; set; }
        public Guid ProjectId { get; set; }
        public string ProjectName { get; set; }
        public IEnumerable<Breadcrumb> Breadcrumbs { get; set; }
    }
}