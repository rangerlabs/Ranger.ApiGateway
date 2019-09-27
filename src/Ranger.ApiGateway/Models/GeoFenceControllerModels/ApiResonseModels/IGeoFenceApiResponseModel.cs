using System.Collections.Generic;

namespace Ranger.ApiGateway
{
    public interface IGeoFenceApiResponseModel
    {
        string Id { get; set; }
        string AppId { get; set; }
        IEnumerable<string> IntegrationIds { get; set; }
        string Name { get; set; }
        string Description { get; set; }
        string Shape { get; }
        bool OnEnter { get; set; }
        bool OnExit { get; set; }

    }
}