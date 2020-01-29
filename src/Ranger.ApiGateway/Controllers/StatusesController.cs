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
            return Ok($"The Ranger API is up and running as of {DateTime.UtcNow} UTC.");
        }
    }
}