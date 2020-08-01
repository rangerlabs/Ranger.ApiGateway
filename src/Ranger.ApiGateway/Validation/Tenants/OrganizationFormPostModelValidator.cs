using FluentValidation;

namespace Ranger.ApiGateway.Validation.Tenants
{
    public class OrganizationFormPostModelValidator : AbstractValidator<OrganizationFormPostModel>
    {
        public OrganizationFormPostModelValidator()
        {
            RuleFor(x => x.Domain).MinimumLength(3).MaximumLength(28).Matches(RegularExpressions.ORGANIZATION_DOMAIN).WithMessage("Must begin, end, and contain alphanumeric characters. May contain ( - ).");
            RuleFor(x => x.OrganizationName).MinimumLength(3).MaximumLength(48).Matches(RegularExpressions.ORGANIZATION_NAME).WithMessage("Must begin, end, and contain alphanumeric characters. May contain ( _ ) ( - ) ( ' ) and whitespace.");
        }
    }
}