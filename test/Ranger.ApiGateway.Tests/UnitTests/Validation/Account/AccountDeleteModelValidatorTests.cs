using FluentValidation;
using FluentValidation.TestHelper;
using Xunit;

namespace Ranger.ApiGateway.Tests
{
    [Collection("Validation collection")]
    public class AccountDeleteModelValidatorTests
    {
        private readonly IValidator<AccountDeleteModel> accountDeleteValidator;
        public AccountDeleteModelValidatorTests(ValidationFixture fixture)
        {
            this.accountDeleteValidator = fixture.serviceProvider.GetRequiredServiceForTest<IValidator<AccountDeleteModel>>();
        }

        [Fact]
        public void Password_Should_Have_Error_When_Empty()
        {
            this.accountDeleteValidator.ShouldHaveValidationErrorFor(x => x.Password, "");
        }

        [Fact]
        public void Password_Should_NOT_Have_Error_When_Empty()
        {
            this.accountDeleteValidator.ShouldNotHaveValidationErrorFor(x => x.Password, "a");
        }
    }
}