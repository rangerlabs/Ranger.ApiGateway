using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ranger.ApiGateway;

namespace Ranger.ApiGateway
{
    [ApiController]
    [Authorize(Roles = "User")]
    public class IntegrationBaseController<T> : ControllerBase
    {
        //     public async Task<IActionResult> Index (string name) {

        //     }

        //     [HttpGet ("/all")]
        //     public async Task<IActionResult> All (string email) {

        //     }

        //     [HttpPost ("")]
        //     public async Task<IActionResult> Post (ApplicationModel applicationModel) { }

    }
}