using FluentValidation;

namespace Ranger.ApiGateway.Validation.Account
{
    public class PrimaryOwnershipAcceptModelValidator : AbstractValidator<PrimaryOwnershipAcceptModel>
    {
        public PrimaryOwnershipAcceptModelValidator()
        {
            RuleFor(x => x.CorrelationId).NotEmpty();
            RuleFor(x => x.Token).NotEmpty();
        }
    }
}