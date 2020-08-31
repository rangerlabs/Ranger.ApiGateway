using System;
using FluentValidation;
using FluentValidation.TestHelper;
using Xunit;

namespace Ranger.ApiGateway.Tests
{
    [Collection("Validation collection")]
    public class UserFormValidationTests
    {
        private readonly IValidator<UserForm> organizationPutFormModelValidator;
        public UserFormValidationTests(ValidationFixture fixture)
        {
            this.organizationPutFormModelValidator = fixture.serviceProvider.GetRequiredServiceForTest<IValidator<UserForm>>();
        }

        [Fact]
        public void Email_Should_Have_Error_When_Empty()
        {
            this.organizationPutFormModelValidator.ShouldHaveValidationErrorFor(x => x.Email, "");
        }

        [Fact]
        public void Email_Should_Have_Error_When_No_Ampersand()
        {
            this.organizationPutFormModelValidator.ShouldHaveValidationErrorFor(x => x.Email, new string('a', 2));
        }

        [Fact]
        public void Email_Should_NOT_Have_Error_When_Has_Ampersand()
        {
            this.organizationPutFormModelValidator.ShouldNotHaveValidationErrorFor(x => x.Email, "a@a");
        }

        [Fact]
        public void FirstNameLastName_Should_Have_Error_When_Empty()
        {
            this.organizationPutFormModelValidator.ShouldHaveValidationErrorFor(x => x.FirstName, "");
            this.organizationPutFormModelValidator.ShouldHaveValidationErrorFor(x => x.LastName, "");
        }

        [Fact]
        public void FirstNameLastName_Should_Have_Error_When_Greater_Than_48_Characters()
        {
            this.organizationPutFormModelValidator.ShouldHaveValidationErrorFor(x => x.FirstName, new String('a', 49));
            this.organizationPutFormModelValidator.ShouldHaveValidationErrorFor(x => x.LastName, new String('a', 49));
        }

        [Theory]
        [InlineData("0!")]
        [InlineData("0")]
        [InlineData("N0")]
        [InlineData("N!")]
        [InlineData("N--")]
        [InlineData("N__")]
        [InlineData("N,,")]
        [InlineData("N''")]
        [InlineData("N..")]
        [InlineData("n ")]
        [InlineData("n N")]
        [InlineData(" N")]
        public void FirstNameLastName_Should_Have_Error_When_Fails_Regex(string name)
        {
            this.organizationPutFormModelValidator.ShouldHaveValidationErrorFor(x => x.FirstName, name);
            this.organizationPutFormModelValidator.ShouldHaveValidationErrorFor(x => x.LastName, name);
        }

        [Theory]
        [InlineData("n")]
        [InlineData("N")]
        [InlineData("N-")]
        [InlineData("N_")]
        [InlineData("N,")]
        [InlineData("N.")]
        [InlineData("N'")]
        public void FirstNameLastName_Should_NOT_Have_Error_When_Passes_Regex(string name)
        {
            this.organizationPutFormModelValidator.ShouldNotHaveValidationErrorFor(x => x.FirstName, name);
            this.organizationPutFormModelValidator.ShouldNotHaveValidationErrorFor(x => x.LastName, name);
        }

        [Fact]
        public void Password_Should_Have_Error_When_Empty()
        {
            this.organizationPutFormModelValidator.ShouldHaveValidationErrorFor(x => x.Password, "");
        }

        [Fact]
        public void Password_Should_Have_Error_When_Less_Than_8_Characters()
        {
            this.organizationPutFormModelValidator.ShouldHaveValidationErrorFor(x => x.Password, "Hello12");
        }

        [Fact]
        public void Password_Should_Have_Error_When_Missing_Special_Character()
        {
            this.organizationPutFormModelValidator.ShouldHaveValidationErrorFor(x => x.Password, "Hello123");
        }

        [Fact]
        public void Password_Should_NOT_Have_Error_When_Has_Special_Character()
        {
            this.organizationPutFormModelValidator.ShouldNotHaveValidationErrorFor(x => x.Password, "Hello123_");
        }

        [Fact]
        public void Password_Should_Have_Error_When_Missing_Number()
        {
            this.organizationPutFormModelValidator.ShouldHaveValidationErrorFor(x => x.Password, "Hello!!!");
        }

        [Fact]
        public void Password_Should_Have_Error_When_Missing_Uppercase()
        {
            this.organizationPutFormModelValidator.ShouldHaveValidationErrorFor(x => x.Password, "hello123!");
        }

        [Fact]
        public void Password_Should_Have_Error_When_Missing_Lowercase()
        {
            this.organizationPutFormModelValidator.ShouldHaveValidationErrorFor(x => x.Password, "HELLO123!");
        }

        [Fact]
        public void ConfirmPassword_Should_Have_Error_When_NOT_Equal_To_Password()
        {
            var model = new UserForm
            {
                Password = "Hello123!",
                ConfirmPassword = "GoodBye123!"
            };
            var result = this.organizationPutFormModelValidator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.ConfirmPassword);

        }

        [Fact]
        public void ConfirmPassword_Should_NOT_Have_Error_When_Equal_To_Password()
        {
            var model = new UserForm
            {
                Password = "Hello123!",
                ConfirmPassword = "Hello123!"
            };
            var result = this.organizationPutFormModelValidator.TestValidate(model);
            result.ShouldNotHaveValidationErrorFor(x => x.ConfirmPassword);
        }
    }
}