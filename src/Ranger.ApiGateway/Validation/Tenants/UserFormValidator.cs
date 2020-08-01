using System.Text.RegularExpressions;
using FluentValidation;

namespace Ranger.ApiGateway
{
    public class UserFormValidator : AbstractValidator<UserForm>
    {
        public UserFormValidator()
        {
            RuleFor(u => u.Email).NotEmpty().EmailAddress();
            RuleFor(x => x.FirstName).NotEmpty().MaximumLength(48).Matches(RegularExpressions.NAME).WithMessage("Must begin with and contain alphabetic character. May contain one of ( - ) ( , ) ( ' ) ( . ).");
            RuleFor(x => x.LastName).NotEmpty().MaximumLength(48).Matches(RegularExpressions.NAME).WithMessage("Must begin with and contain alphabetic character. May contain one of ( - ) ( , ) ( ' ) ( . ).");
            RuleFor(u => u.Password)
                .MinimumLength(8)
                .MaximumLength(64)
                .Matches(RegularExpressions.PASSWORD_SPECIAL_CHARACTER).WithMessage("Must contain at least 1 special character")
                .Matches(RegularExpressions.PASSWORD_NUMBER).WithMessage("Must contain at least 1 number")
                .Matches(RegularExpressions.PASSWORD_LOWERCASE_LETTER).WithMessage("Must contain at least 1 lowercase letter")
                .Matches(RegularExpressions.PASSWORD_UPPERCASE_LETTER).WithMessage("Must contain at least 1 uppercase letter");
            RuleFor(u => u.ConfirmPassword).Equal(u => u.Password);
        }
    }
}