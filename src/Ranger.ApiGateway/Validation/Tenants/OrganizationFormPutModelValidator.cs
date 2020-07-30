using FluentValidation;

namespace Ranger.ApiGateway.Validation
{
    public class OrganizationFormPutModelValidator : AbstractValidator<OrganizationFormPutModel>
    {
        public OrganizationFormPutModelValidator()
        {
            RuleFor(x => x.Domain).MinimumLength(3).MaximumLength(28).Matches(RegularExpressions.ORGANIZATION_DOMAIN).WithMessage("Must begin, end, and contain alphanumeric characters. May contain the following (-).");
            RuleFor(x => x.OrganizationName).MinimumLength(3).MaximumLength(48).Matches(RegularExpressions.ORGANIZATION_NAME).WithMessage("Must begin, end, and contain alphanumeric characters. May contain the following ( ) (_) (-) (').");
            RuleFor(x => x.Version).GreaterThanOrEqualTo(1);
        }
    }
}