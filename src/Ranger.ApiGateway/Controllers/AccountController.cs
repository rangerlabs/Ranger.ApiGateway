using System;
using System.Threading;
using System.Threading.Tasks;
using AutoWrapper.Wrappers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Ranger.Common;
using Ranger.InternalHttpClient;
using Ranger.RabbitMQ;
using Ranger.RabbitMQ.BusPublisher;

namespace Ranger.ApiGateway
{
    [ApiVersion("1.0")]
    [ApiController]
    public class AccountController : BaseController
    {
        private readonly IIdentityHttpClient identityClient;
        private readonly ILogger<AccountController> logger;
        private readonly IBusPublisher busPublisher;

        public AccountController(IBusPublisher busPublisher, IIdentityHttpClient identityClient, ILogger<AccountController> logger) : base(busPublisher, logger)
        {
            this.busPublisher = busPublisher;
            this.identityClient = identityClient;
            this.logger = logger;
        }

        ///<summary>
        /// Updates a user's account
        ///</summary>
        ///<param name="accountUpdateModel">The account update model necessary to update the account</param>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPut("/account")]
        [Authorize(Policy = AuthorizationPolicyNames.TenantIdResolved)]
        public async Task<ApiResponse> AccountUpdate(AccountUpdateModel accountUpdateModel)
        {
            accountUpdateModel.FirstName = accountUpdateModel.FirstName.Trim();
            accountUpdateModel.LastName = accountUpdateModel.LastName.Trim();
            var apiResponse = await identityClient.UpdateUserOrAccountAsync(TenantId, UserFromClaims.Email, JsonConvert.SerializeObject(accountUpdateModel));
            return new ApiResponse("Successfully updated account");
        }

        ///<summary>
        /// Deletes a user's account
        ///</summary>
        ///<param name="accountDeleteModel">The account delete model necessary to delete the account</param>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpDelete("/account")]
        [Authorize(Policy = AuthorizationPolicyNames.TenantIdResolved)]
        public ApiResponse DeleteAccount(AccountDeleteModel accountDeleteModel)
        {
            if (UserFromClaims.Role.ToLowerInvariant() == Enum.GetName(typeof(RolesEnum), RolesEnum.PrimaryOwner))
            {
                throw new ApiException("Primary Owners must transfer ownership of the domain before deleting their account", StatusCodes.Status403Forbidden);
            }

            return base.SendAndAccept(new DeleteAccountSagaInitializer(TenantId, User.UserFromClaims().Email, accountDeleteModel.Password));
        }

        ///<summary>
        /// Deletes a user's account
        ///</summary>
        ///<param name="transferPrimaryOwnerModel">The transfer primary owner model necessary to transfer domain ownership</param>
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpPost("/account/transfer-primary-ownership")]
        [Authorize(Policy = AuthorizationPolicyNames.TenantIdResolved)]
        public async Task<ApiResponse> TransferPrimaryOwnership([FromBody] TransferPrimaryOwnershipModel transferPrimaryOwnerModel)
        {
            if (UserFromClaims.Role != Enum.GetName(typeof(RolesEnum), RolesEnum.PrimaryOwner))
            {
                throw new ApiException("Only Primary Owners have the ability to transfer their role", StatusCodes.Status400BadRequest);
            }

            return await Task.Run(() =>
               SendAndAccept(new TransferPrimaryOwnershipSagaInitializer(UserFromClaims.Email, transferPrimaryOwnerModel.Email, TenantId), "Successfully initiated owner transfer")
            );
        }

        ///<summary>
        /// Accepts a primary owner transfer
        ///</summary>
        ///<param name="acceptPrimaryOwnerModel">The accept primary owner transfer model necessary to accept domain ownership</param>
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpPost("/account/accept-primary-ownership")]
        [AllowAnonymous]
        public async Task<ApiResponse> AcceptPrimaryOwnership([FromBody] PrimaryOwnershipAcceptModel acceptPrimaryOwnerModel)
        {
            if (Guid.Equals(Guid.Empty, acceptPrimaryOwnerModel.CorrelationId))
            {
                throw new ApiException("The correlationId parameter cannot be an empty GUID", StatusCodes.Status400BadRequest);
            }
            await Task.Run(() => busPublisher.Send(new AcceptPrimaryOwnershipTransfer(acceptPrimaryOwnerModel.Token), CorrelationContext.FromId(acceptPrimaryOwnerModel.CorrelationId)));
            return new ApiResponse("Successfully accepted primary owner transfer response", statusCode: StatusCodes.Status202Accepted);
        }

        ///<summary>
        /// Rejects a primary owner transfer
        ///</summary>
        ///<param name="rejectPrimaryOwnerModel">The reject accept primary owner transfer model necessary to reject domain ownership</param>
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpPost("/account/refuse-primary-ownership")]
        [AllowAnonymous]
        public async Task<ApiResponse> RejectPrimaryOwnership([FromBody] PrimaryOwnershipRefuseModel rejectPrimaryOwnerModel)
        {
            if (Guid.Equals(Guid.Empty, rejectPrimaryOwnerModel.CorrelationId))
            {
                throw new ApiException("The correlationId parameter cannot be an empty GUID", StatusCodes.Status400BadRequest);
            }
            await Task.Run(() => busPublisher.Send(new RefusePrimaryOwnershipTransfer(), CorrelationContext.FromId(rejectPrimaryOwnerModel.CorrelationId)));
            return new ApiResponse("Successfully accepted primary owner transfer reject response", statusCode: StatusCodes.Status202Accepted);
        }

        ///<summary>
        /// Cancels a primary owner transfer
        ///</summary>
        ///<param name="cancelPrimaryOwnerModel">The cancel accept primary owner transfer model necessary to cance a transfer</param>
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpPost("/account/cancel-ownership-transfer")]
        [AllowAnonymous]
        public async Task<ApiResponse> CancelPrimaryOwnershipTransfer([FromBody] CancelPrimaryOwnershipModel cancelPrimaryOwnerModel)
        {
            if (Guid.Equals(Guid.Empty, cancelPrimaryOwnerModel.CorrelationId))
            {
                throw new ApiException("The correlationId parameter cannot be an empty GUID", StatusCodes.Status400BadRequest);
            }
            await Task.Run(() => busPublisher.Send(new CancelPrimaryOwnershipTransfer(), CorrelationContext.FromId(cancelPrimaryOwnerModel.CorrelationId)));
            return new ApiResponse("Successfully accepted primary owner transfer cancel response", statusCode: StatusCodes.Status202Accepted);
        }
    }
}