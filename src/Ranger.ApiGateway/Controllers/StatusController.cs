using System;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ranger.ApiGateway
{
    [ApiVersionNeutral]
    public class StatusController : ControllerBase
    {

        [HttpGet("/status")]
        [ProducesResponseType(200)]
        [AllowAnonymous]
        public IActionResult Index()
        {
            return Ok($"The Ranger API running as of {DateTime.UtcNow}");
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("/status/identity")]
        [ProducesResponseType(200)]
        public IActionResult GetIdentity()
        {
            var result = new JsonResult(from c in User.Claims select new { c.Type, c.Value });
            return result;
        }
    }
}