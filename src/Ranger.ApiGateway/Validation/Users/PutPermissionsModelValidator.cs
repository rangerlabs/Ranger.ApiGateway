using System.Linq;
using FluentValidation;

namespace Ranger.ApiGateway.Validation.Users
{
    public class PutPermissionsModelValidator : AbstractValidator<PutPermissionsModel>
    {
        public PutPermissionsModelValidator()
        {
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