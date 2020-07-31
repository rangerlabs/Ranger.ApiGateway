using FluentValidation;

namespace Ranger.ApiGateway.Validation.Users
{
    public class UserConfirmEmailChangeModelValidator : AbstractValidator<UserConfirmEmailChangeModel>
    {
        public UserConfirmEmailChangeModelValidator()
        {
            RuleFor(x => x.Email).NotEmpty().EmailAddress();
            RuleFor(x => x.Domain).NotEmpty();
            RuleFor(x => x.Token).NotEmpty();
        }
    }
}