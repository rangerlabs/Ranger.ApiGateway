using FluentValidation;

namespace Ranger.ApiGateway.Validation.Projects
{
    public class PostProjectModelValidator : AbstractValidator<PostProjectModel>
    {
        public PostProjectModelValidator()
        {
            RuleFor(x => x.Name)
                .MinimumLength(3)
                .MaximumLength(128)
                .Matches(RegularExpressions.PROJECT_NAME)
                .WithMessage("Must begin, end, and contain alphanumeric characters. May contain the following (_) (-) ( ) (,) (') (.).");
            RuleFor(x => x.Description).MaximumLength(512);
        }
    }
}