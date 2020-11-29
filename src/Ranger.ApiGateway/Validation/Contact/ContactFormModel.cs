using FluentValidation;

namespace Ranger.ApiGateway.Validation.Contact
{
    public class ContactFormModelValidator : AbstractValidator<ContactFormModel>
    {
        public ContactFormModelValidator()
        {
            RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(512);
            RuleFor(x => x.Organization).NotEmpty().MaximumLength(512);
            RuleFor(x => x.Name).NotEmpty().MaximumLength(512);
            RuleFor(x => x.Message).NotEmpty().MaximumLength(1000);
            RuleFor(x => x.ReCaptchaToken).NotEmpty();
        }
    }
}