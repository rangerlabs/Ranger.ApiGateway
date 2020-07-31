using FluentValidation;
using FluentValidation.TestHelper;
using Xunit;

namespace Ranger.ApiGateway.Tests.Validation.Users
{
    [Collection("Validation collection")]
    public class UserConfirmEmailChangeModelValidatorTests
    {
        private readonly IValidator<UserConfirmEmailChangeModel> userConfirmEmailChangeModelValidator;
        public UserConfirmEmailChangeModelValidatorTests(ValidationFixture fixture)
        {
            this.userConfirmEmailChangeModelValidator = fixture.serviceProvider.GetRequiredServiceForTest<IValidator<UserConfirmEmailChangeModel>>();
        }

        [Fact]
        public void Email_Should_Have_Error_When_Empty()
        {
            this.userConfirmEmailChangeModelValidator.ShouldHaveValidationErrorFor(x => x.Email, "");
        }

        [Fact]
        public void Email_Should_Have_Error_When_No_Ampersand()
        {
            this.userConfirmEmailChangeModelValidator.ShouldHaveValidationErrorFor(x => x.Email, "aa");
        }

        [Fact]
        public void Email_Should_NOT_Have_Error_When_Has_Ampersand()
        {
            this.userConfirmEmailChangeModelValidator.ShouldNotHaveValidationErrorFor(x => x.Email, "a@a");
        }

        [Fact]
        public void Domain_Should_Have_Error_When_Empty()
        {
            this.userConfirmEmailChangeModelValidator.ShouldHaveValidationErrorFor(x => x.Domain, "");
        }

        [Fact]
        public void Domain_Should_Not_Have_Error_When_Empty()
        {
            this.userConfirmEmailChangeModelValidator.ShouldNotHaveValidationErrorFor(x => x.Domain, "a");
        }

        [Fact]
        public void Token_Should_Have_Error_When_Empty()
        {
            this.userConfirmEmailChangeModelValidator.ShouldHaveValidationErrorFor(x => x.Token, "");
        }

        [Fact]
        public void Token_Should_Not_Have_Error_When_Empty()
        {
            this.userConfirmEmailChangeModelValidator.ShouldNotHaveValidationErrorFor(x => x.Token, "a");
        }
    }
}