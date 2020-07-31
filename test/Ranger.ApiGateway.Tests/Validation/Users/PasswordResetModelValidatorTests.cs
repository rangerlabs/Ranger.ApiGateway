using System;
using FluentValidation;
using FluentValidation.TestHelper;
using Xunit;

namespace Ranger.ApiGateway.Tests
{
    [Collection("Validation collection")]
    public class PasswordResetModelValidatorTests
    {
        private readonly IValidator<PasswordResetModel> passwordResetModelValidator;
        public PasswordResetModelValidatorTests(ValidationFixture fixture)
        {
            this.passwordResetModelValidator = fixture.serviceProvider.GetRequiredServiceForTest<IValidator<PasswordResetModel>>();
        }

        [Fact]
        public void Password_Should_Have_Error_When_Empty()
        {
            this.passwordResetModelValidator.ShouldHaveValidationErrorFor(x => x.Password, "");
        }

        [Fact]
        public void Password_Should_Not_Have_Error_When_Not_Empty()
        {
            this.passwordResetModelValidator.ShouldNotHaveValidationErrorFor(x => x.Password, "a");
        }
    }
}