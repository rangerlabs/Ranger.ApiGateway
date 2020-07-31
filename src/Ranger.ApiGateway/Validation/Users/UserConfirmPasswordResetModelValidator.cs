using FluentValidation;

namespace Ranger.ApiGateway.Validation.Users
{
    public class UserConfirmPasswordResetModelValidator : AbstractValidator<UserConfirmPasswordResetModel>
    {
        public UserConfirmPasswordResetModelValidator()
        {
            RuleFor(x => x.Domain).NotEmpty();
            RuleFor(x => x.Token).NotEmpty();
            RuleFor(u => u.NewPassword)
               .MinimumLength(8)
               .MaximumLength(64)
               .Matches(RegularExpressions.PASSWORD_SPECIAL_CHARACTER).WithMessage("Must contain at least 1 special character")
               .Matches(RegularExpressions.PASSWORD_NUMBER).WithMessage("Must contain at least 1 number")
               .Matches(RegularExpressions.PASSWORD_LOWERCASE_LETTER).WithMessage("Must contain at least 1 lowercase letter")
               .Matches(RegularExpressions.PASSWORD_UPPERCASE_LETTER).WithMessage("Must contain at least 1 uppercase letter");
            RuleFor(u => u.ConfirmPassword).Equal(u => u.NewPassword);
        }
    }
}