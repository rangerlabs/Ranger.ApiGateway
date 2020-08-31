using FluentValidation;
using FluentValidation.TestHelper;
using Xunit;

namespace Ranger.ApiGateway.Tests.Validation.Users
{
    [Collection("Validation collection")]
    public class EmailChangeModelValidatorTests
    {
        private readonly IValidator<EmailChangeModel> deleteUserModelValidator;
        public EmailChangeModelValidatorTests(ValidationFixture fixture)
        {
            this.deleteUserModelValidator = fixture.serviceProvider.GetRequiredServiceForTest<IValidator<EmailChangeModel>>();
        }

        [Fact]
        public void Email_Should_Have_Error_When_Empty()
        {
            this.deleteUserModelValidator.ShouldHaveValidationErrorFor(x => x.Email, "");
        }

        [Fact]
        public void Email_Should_Have_Error_When_No_Ampersand()
        {
            this.deleteUserModelValidator.ShouldHaveValidationErrorFor(x => x.Email, "aa");
        }

        [Fact]
        public void Email_Should_NOT_Have_Error_When_Has_Ampersand()
        {
            this.deleteUserModelValidator.ShouldNotHaveValidationErrorFor(x => x.Email, "a@a");
        }
    }
}