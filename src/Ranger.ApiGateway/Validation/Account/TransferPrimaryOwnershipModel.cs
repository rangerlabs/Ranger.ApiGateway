using FluentValidation;

namespace Ranger.ApiGateway.Validation.Account
{
    public class TransferPrimaryOwnershipModelValidator : AbstractValidator<TransferPrimaryOwnershipModel>
    {
        public TransferPrimaryOwnershipModelValidator()
        {
            RuleFor(x => x.Email).NotEmpty().EmailAddress();
        }
    }
}