using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Ranger.ApiUtilities;
using Ranger.Common;
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
        private readonly IBusPublisher busPublisher;

        public AccountController(IBusPublisher busPublisher, IIdentityClient identityClient, IProjectsClient projectsClient, ILogger<AccountController> logger) : base(busPublisher, logger)
        {
            this.busPublisher = busPublisher;
            this.identityClient = identityClient;
            this.logger = logger;
        }

        [HttpPut("/accounts/{email}")]
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

        [HttpDelete("/accounts/{email}")]
        [TenantDomainRequired]
        public async Task<IActionResult> DeleteAccount([FromRoute] string email, AccountDeleteModel accountDeleteModel)
        {
            if (email.ToLowerInvariant() != User.UserFromClaims().Email.ToLowerInvariant())
            {
                return Forbid();
            }

            try
            {
                await identityClient.DeleteAccountAsync(Domain, email, JsonConvert.SerializeObject(accountDeleteModel));
            }
            catch (HttpClientException ex)
            {
                if ((int)ex.ApiResponse.StatusCode == StatusCodes.Status400BadRequest)
                {
                    return BadRequest(ex.ApiResponse.Errors);
                }
                if ((int)ex.ApiResponse.StatusCode == StatusCodes.Status403Forbidden)
                {
                    return Forbid();
                }
                if ((int)ex.ApiResponse.StatusCode == StatusCodes.Status404NotFound)
                {
                    return NotFound();
                }
                return InternalServerError();
            }
            busPublisher.Send(new SendPusherDomainUserPredefinedNotification("ForceSignoutNotification", Domain, email), CorrelationContext.Empty);
            return NoContent();
        }
    }
}