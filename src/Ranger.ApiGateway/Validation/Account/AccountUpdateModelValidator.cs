using System.Text.RegularExpressions;
using FluentValidation;

namespace Ranger.ApiGateway
{
    public class AccountUpdateModelValidator : AbstractValidator<AccountUpdateModel>
    {
        public AccountUpdateModelValidator()
        {
            RuleFor(x => x.FirstName).NotEmpty().MaximumLength(48).Matches(RegularExpressions.NAME).WithMessage("Must begin with and contain alphabetic character. May contain one of the following (-) (,) (') (.).");
            RuleFor(x => x.LastName).NotEmpty().MaximumLength(48).Matches(RegularExpressions.NAME).WithMessage("Must begin with and contain alphabetic character. May contain one of the following (-) (,) (') (.).");
        }
    }
}