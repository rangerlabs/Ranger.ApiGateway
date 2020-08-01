using System.Linq;
using FluentValidation;

namespace Ranger.ApiGateway.Validation.Users
{
    public class PostApplicationUserModelValidator : AbstractValidator<PostApplicationUserModel>
    {
        public PostApplicationUserModelValidator()
        {
            RuleFor(u => u.Email).NotEmpty().EmailAddress();
            RuleFor(x => x.FirstName).NotEmpty().MaximumLength(48).Matches(RegularExpressions.NAME).WithMessage("Must begin with and contain alphabetic character. May contain one of ( - ) ( , ) ( ' ) ( . ).");
            RuleFor(x => x.LastName).NotEmpty().MaximumLength(48).Matches(RegularExpressions.NAME).WithMessage("Must begin with and contain alphabetic character. May contain one of ( - ) ( , ) ( ' ) ( . ).");
            RuleFor(x => x.Role).NotEmpty().IsInEnum();
            RuleFor(x => x.AuthorizedProjects).Custom((x, c) =>
            {
                var distinct = x.Distinct();
                if (distinct.Count() != x.Count())
                {
                    c.AddFailure("Contains duplicate Project Ids");
                }
            });
        }
    }
}