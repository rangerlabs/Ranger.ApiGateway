using FluentValidation;
using FluentValidation.TestHelper;
using Xunit;

namespace Ranger.ApiGateway.Tests.Validation.Users
{
    [Collection("Validation collection")]
    public class DeleteUserModelValidatorTests
    {
        private readonly IValidator<DeleteUserModel> deleteUserModelValidator;
        public DeleteUserModelValidatorTests(ValidationFixture fixture)
        {
            this.deleteUserModelValidator = fixture.serviceProvider.GetRequiredServiceForTest<IValidator<DeleteUserModel>>();
        }

        [Fact]
        public void CommandingUserEmail_Should_Have_Error_When_Empty()
        {
            this.deleteUserModelValidator.ShouldHaveValidationErrorFor(x => x.CommandingUserEmail, "");
        }

        [Fact]
        public void CommandingUserEmail_Should_Have_Error_When_No_Ampersand()
        {
            this.deleteUserModelValidator.ShouldHaveValidationErrorFor(x => x.CommandingUserEmail, "aa");
        }

        [Fact]
        public void CommandingUserEmail_Should_NOT_Have_Error_When_Has_Ampersand()
        {
            this.deleteUserModelValidator.ShouldNotHaveValidationErrorFor(x => x.CommandingUserEmail, "a@a");
        }
    }
}