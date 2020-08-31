using FluentValidation;
using FluentValidation.TestHelper;
using Xunit;

namespace Ranger.ApiGateway.Tests
{
    [Collection("Validation collection")]
    public class TenantConfirmModelValidationTests
    {
        private readonly IValidator<TenantConfirmModel> confirmModelValidator;
        public TenantConfirmModelValidationTests(ValidationFixture fixture)
        {
            this.confirmModelValidator = fixture.serviceProvider.GetRequiredServiceForTest<IValidator<TenantConfirmModel>>();
        }

        [Fact]
        public void Token_Should_Have_Error_When_Empty()
        {
            this.confirmModelValidator.ShouldHaveValidationErrorFor(x => x.Token, "");
        }

        [Fact]
        public void Token_Should_NOT_Have_Error_When_NOT_Empty()
        {
            this.confirmModelValidator.ShouldNotHaveValidationErrorFor(x => x.Token, "a");
        }
    }
}