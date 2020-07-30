using FluentValidation;

namespace Ranger.ApiGateway.Validation.Account
{
    public class CancelPrimaryOwnershipModelValidator : AbstractValidator<CancelPrimaryOwnershipModel>
    {
        public CancelPrimaryOwnershipModelValidator()
        {
            RuleFor(x => x.CorrelationId).NotEmpty();
        }
    }
}