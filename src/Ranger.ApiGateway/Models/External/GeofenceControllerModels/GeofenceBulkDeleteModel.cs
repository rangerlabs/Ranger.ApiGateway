using System.Collections.Generic;

namespace Ranger.ApiGateway
{
    public class GeofenceBulkDeleteModel
    {
        public IEnumerable<string> ExternalIds { get; set; }
    }
}