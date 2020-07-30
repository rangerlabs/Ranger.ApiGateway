using FluentValidation;

namespace Ranger.ApiGateway.Validation.Tenants
{
    public class TenantConfirmModelValidator : AbstractValidator<TenantConfirmModel>
    {
        public TenantConfirmModelValidator()
        {
            RuleFor(x => x.Token).NotEmpty();
        }
    }
}