using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ranger.ApiUtilities;

namespace Ranger.ApiGateway
{
    [ApiVersion("1.0")]
    [ApiController]
    [Authorize(Roles = "User")]
    [TenantDomainRequired]
    public class IntegrationController : ControllerBase
    {
        [HttpGet("{projectName}/integrations")]
        public async Task<IActionResult> All(string projectName)
        {
            return Ok(new object[0]);
        }
    }
}