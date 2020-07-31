using FluentValidation;

namespace Ranger.ApiGateway.Tests.Validation.Users
{
    public class EmailChangeModelValidator : AbstractValidator<EmailChangeModel>
    {
        public EmailChangeModelValidator()
        {
            RuleFor(x => x.Email).NotEmpty().EmailAddress();
        }
    }
}