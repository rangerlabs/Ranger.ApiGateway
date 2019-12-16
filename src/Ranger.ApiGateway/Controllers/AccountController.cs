using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Ranger.ApiUtilities;
using Ranger.InternalHttpClient;
using Ranger.RabbitMQ;

namespace Ranger.ApiGateway
{
    [ApiVersion("1.0")]
    [ApiController]
    public class AccountController : BaseController<AccountController>
    {
        private readonly IIdentityClient identityClient;
        private readonly ILogger<AccountController> logger;
        private readonly IProjectsClient projectsClient;

        public AccountController(IBusPublisher busPublisher, IIdentityClient identityClient, IProjectsClient projectsClient, ILogger<AccountController> logger) : base(busPublisher, logger)
        {
            this.identityClient = identityClient;
            this.logger = logger;
        }

        [HttpPut("/account/{email}")]
        [TenantDomainRequired]
        public async Task<IActionResult> AccountUpdate([FromRoute] string email, AccountUpdateModel accountUpdateModel)
        {
            try
            {
                await identityClient.UpdateUserAsync(Domain, email, JsonConvert.SerializeObject(accountUpdateModel));
            }
            catch (HttpClientException ex)
            {
                if ((int)ex.ApiResponse.StatusCode == StatusCodes.Status404NotFound)
                {
                    return NotFound();
                }
            }
            return Ok();
        }
    }
}