using System;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ranger.ApiGateway {
    public class StatusController : ControllerBase {

        [HttpGet ("/app/status")]
        [ProducesResponseType (200)]
        [AllowAnonymous]
        public IActionResult Index () {
            return Ok ($"Api App running as of {DateTime.UtcNow}");
        }

        [Authorize (Roles = "Admin")]
        [HttpGet ("/app/status/identity")]
        [ProducesResponseType (200)]
        public IActionResult GetIdentity () {
            var result = new JsonResult (from c in User.Claims select new { c.Type, c.Value });
            return result;
        }
    }
}