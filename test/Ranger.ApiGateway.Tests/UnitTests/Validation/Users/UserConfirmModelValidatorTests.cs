using FluentValidation;
using FluentValidation.TestHelper;
using Xunit;

namespace Ranger.ApiGateway.Tests.Validation.Users
{
    [Collection("Validation collection")]
    public class UserConfirmEmailModelValidatorTests
    {
        private readonly IValidator<UserConfirmModel> userConfirmEmailChangeModelValidator;
        public UserConfirmEmailModelValidatorTests(ValidationFixture fixture)
        {
            this.userConfirmEmailChangeModelValidator = fixture.serviceProvider.GetRequiredServiceForTest<IValidator<UserConfirmModel>>();
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

        [Fact]
        public void Password_Should_Have_Error_When_Empty()
        {
            this.userConfirmEmailChangeModelValidator.ShouldHaveValidationErrorFor(x => x.NewPassword, "");
        }

        [Fact]
        public void Password_Should_Have_Error_When_Less_Than_8_Characters()
        {
            this.userConfirmEmailChangeModelValidator.ShouldHaveValidationErrorFor(x => x.NewPassword, "Hello12");
        }

        [Fact]
        public void Password_Should_Have_Error_When_Missing_Special_Character()
        {
            this.userConfirmEmailChangeModelValidator.ShouldHaveValidationErrorFor(x => x.NewPassword, "Hello123");
        }

        [Fact]
        public void Password_Should_NOT_Have_Error_When_Has_Special_Character()
        {
            this.userConfirmEmailChangeModelValidator.ShouldNotHaveValidationErrorFor(x => x.NewPassword, "Hello123_");
        }

        [Fact]
        public void Password_Should_Have_Error_When_Missing_Number()
        {
            this.userConfirmEmailChangeModelValidator.ShouldHaveValidationErrorFor(x => x.NewPassword, "Hello!!!");
        }

        [Fact]
        public void Password_Should_Have_Error_When_Missing_Uppercase()
        {
            this.userConfirmEmailChangeModelValidator.ShouldHaveValidationErrorFor(x => x.NewPassword, "hello123!");
        }

        [Fact]
        public void Password_Should_Have_Error_When_Missing_Lowercase()
        {
            this.userConfirmEmailChangeModelValidator.ShouldHaveValidationErrorFor(x => x.NewPassword, "HELLO123!");
        }

        [Fact]
        public void ConfirmPassword_Should_Have_Error_When_NOT_Equal_To_Password()
        {
            var model = new UserConfirmModel
            {
                NewPassword = "Hello123!",
                ConfirmPassword = "GoodBye123!"
            };
            var result = this.userConfirmEmailChangeModelValidator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.ConfirmPassword);

        }

        [Fact]
        public void ConfirmPassword_Should_NOT_Have_Error_When_Equal_To_Password()
        {
            var model = new UserConfirmModel
            {
                NewPassword = "Hello123!",
                ConfirmPassword = "Hello123!"
            };
            var result = this.userConfirmEmailChangeModelValidator.TestValidate(model);
            result.ShouldNotHaveValidationErrorFor(x => x.ConfirmPassword);
        }
    }
}