using FluentValidation;

namespace Ranger.ApiGateway.Validation.Projects
{
    public class ApiKeyResetModelValidator : AbstractValidator<ApiKeyResetModel>
    {
        public ApiKeyResetModelValidator()
        {
            RuleFor(x => x.Version).GreaterThanOrEqualTo(1);
        }
    }
}