using System.Collections.Generic;
using Newtonsoft.Json;

namespace Ranger.ApiGateway {

    public class ServerErrors {
        public Dictionary<string, string[]> errors { get; set; }
    }
}