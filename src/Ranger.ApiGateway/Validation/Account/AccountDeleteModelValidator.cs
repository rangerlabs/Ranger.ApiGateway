using FluentValidation;

namespace Ranger.ApiGateway.Validation.Account
{
    public class AccountDeleteModelValidator : AbstractValidator<AccountDeleteModel>
    {
        public AccountDeleteModelValidator()
        {
            RuleFor(x => x.Password).NotEmpty();
        }
    }
}