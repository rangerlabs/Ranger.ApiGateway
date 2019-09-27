using System.Collections.Generic;

namespace Ranger.ApiGateway
{
    public interface IGeoFenceModel
    {
        string Name { get; set; }
        string Description { get; set; }
        GeoFenceShapeEnum Shape { get; }
        IEnumerable<string> IntegrationIds { get; set; }
        bool OnEnter { get; set; }
        bool OnExit { get; set; }

    }
}