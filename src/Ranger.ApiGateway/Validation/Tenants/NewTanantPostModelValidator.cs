using FluentValidation;

namespace Ranger.ApiGateway.Validation.Tenants
{
    public class NewTanantPostModelValidator : AbstractValidator<NewTenantPostModel>
    {
        public NewTanantPostModelValidator(IValidator<OrganizationFormPostModel> organizationFormPostValidator, IValidator<UserForm> userFormValidator)
        {
            RuleFor(x => x.OrganizationForm).SetValidator(organizationFormPostValidator);
            RuleFor(x => x.UserForm).SetValidator(userFormValidator);
        }
    }
}