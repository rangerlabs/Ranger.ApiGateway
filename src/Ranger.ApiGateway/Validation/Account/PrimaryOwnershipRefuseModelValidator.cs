using FluentValidation;

namespace Ranger.ApiGateway.Validation.Account
{
    public class PrimaryOwnershipRefuseModelValidator : AbstractValidator<PrimaryOwnershipRefuseModel>
    {
        public PrimaryOwnershipRefuseModelValidator()
        {
            RuleFor(x => x.CorrelationId).NotEmpty();
        }
    }
}