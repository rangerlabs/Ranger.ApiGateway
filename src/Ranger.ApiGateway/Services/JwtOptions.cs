using System;
using System.Text.RegularExpressions;

namespace Ranger.ApiGateway {
    public class JwtOptions {
        public string key { get; set; }
        public string issuer { get; set; }
    }
}