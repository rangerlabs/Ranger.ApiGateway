using FluentValidation;

namespace Ranger.ApiGateway.Validation.Projects
{
    public class PutProjectModelValidator : AbstractValidator<PutProjectModel>
    {
        public PutProjectModelValidator()
        {
            RuleFor(x => x.Name)
                .MinimumLength(3)
                .MaximumLength(128)
                .Matches(RegularExpressions.PROJECT_NAME)
                .WithMessage("Must begin, end, and contain alphanumeric characters. May contain ( _ ) ( - ) ( , ) ( ' ) ( . ) and whitespace.");
            RuleFor(x => x.Description).MaximumLength(512);
            RuleFor(x => x.Version).GreaterThanOrEqualTo(1);
        }
    }
}