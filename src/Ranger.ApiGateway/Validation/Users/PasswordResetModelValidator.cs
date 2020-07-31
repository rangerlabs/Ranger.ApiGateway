using FluentValidation;

namespace Ranger.ApiGateway.Validation.Users
{
    public class PasswordResetModelValidator : AbstractValidator<PasswordResetModel>
    {
        public PasswordResetModelValidator()
        {
            RuleFor(u => u.Password).NotEmpty();
        }
    }
}