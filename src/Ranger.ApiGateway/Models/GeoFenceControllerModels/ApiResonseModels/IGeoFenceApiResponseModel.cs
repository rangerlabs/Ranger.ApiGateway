using System.Collections.Generic;

namespace Ranger.ApiGateway {
    public interface IGeoFenceApiResponseModel {
        string AppName { get; set; }
        IEnumerable<string> Integrations { get; set; }
        string Name { get; set; }
        string Description { get; set; }
        string Shape { get; }
        bool OnEnter { get; set; }
        bool OnExit { get; set; }

    }
}