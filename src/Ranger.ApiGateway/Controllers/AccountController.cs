using System;
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
    [TenantDomainRequired]
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


        [HttpPut("/account/{email}")]
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

        [HttpDelete("/account/{email}")]
        public async Task<IActionResult> DeleteAccount([FromRoute] string email, AccountDeleteModel accountDeleteModel)
        {
            if (email.ToLowerInvariant() != User.UserFromClaims().Email.ToLowerInvariant())
            {
                return Forbid();
            }
            if (User.UserFromClaims().Role.ToLowerInvariant() == Enum.GetName(typeof(RolesEnum), RolesEnum.PrimaryOwner))
            {
                return Forbid("Primary Owners must transfer ownership of the domain before deleting their account.");
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

        [HttpPost("/account/transfer-primary-ownership")]
        public async Task<IActionResult> TransferPrimaryOwnership([FromBody] TransferPrimaryOwnershipModel model)
        {
            if (User.UserFromClaims().Role != Enum.GetName(typeof(RolesEnum), RolesEnum.PrimaryOwner))
            {
                return BadRequest("Only Primary Owners have the ability to transfer their role.");
            }

            return await Task.Run(() => Send(new TransferPrimaryOwnershipSagaInitializer(User.UserFromClaims().Email, model.Email, Domain)));
        }


        [HttpPost("/account/accept-primary-ownership")]
        public async Task<IActionResult> AcceptPrimaryOwnership([FromBody] PrimaryOwnershipAcceptModel model)
        {
            if (Guid.Equals(Guid.Empty, model.CorrelationId))
            {
                return BadRequest("The correlationId parameter cannot be an empty GUID.");
            }
            await Task.Run(() => busPublisher.Send(new AcceptPrimaryOwnershipTransfer(model.Token), CorrelationContext.FromId(model.CorrelationId)));
            return Accepted();
        }

        [HttpPost("/account/refuse-primary-ownership")]
        public async Task<IActionResult> RejectPrimaryOwnership([FromBody] PrimaryOwnershipRefuseModel model)
        {
            if (Guid.Equals(Guid.Empty, model.CorrelationId))
            {
                return BadRequest("The correlationId parameter cannot be an empty GUID.");
            }
            await Task.Run(() => busPublisher.Send(new RefusePrimaryOwnershipTransfer(), CorrelationContext.FromId(model.CorrelationId)));
            return Accepted();
        }

        [HttpPost("/account/cancel-ownership-transfer")]
        public async Task<IActionResult> CancelPrimaryOwnershipTransfer([FromBody] CancelPrimaryOwnershipModel model)
        {
            if (Guid.Equals(Guid.Empty, model.CorrelationId))
            {
                return BadRequest("The correlationId parameter cannot be an empty GUID.");
            }
            await Task.Run(() => busPublisher.Send(new CancelPrimaryOwnershipTransfer(), CorrelationContext.FromId(model.CorrelationId)));
            return Accepted();
        }
    }
}
