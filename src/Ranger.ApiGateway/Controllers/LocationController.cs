using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Ranger.ApiGateway.Middleware;
using Ranger.RabbitMQ;

namespace Ranger.ApiGateway.Controllers
{
    [ApiVersion("1.0")]
    [ApiController]
    [ApiKeyRequired]
    [AllowAnonymous]
    public class LocationController : BaseController<LocationController>
    {
        public LocationController(IBusPublisher busPublisher, ILogger<LocationController> logger) : base(busPublisher, logger)
        {
        }

        [HttpGet("/location/status")]
        public async Task<IActionResult> Index()
        {
            return Ok($"You're using the '{HttpContext.Items["ApiKeyEnvironment"]}' API key.");
        }
    }

}