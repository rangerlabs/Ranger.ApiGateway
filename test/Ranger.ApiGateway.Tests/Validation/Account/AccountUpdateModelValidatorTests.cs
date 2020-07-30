using System;
using FluentValidation;
using FluentValidation.TestHelper;
using Xunit;

namespace Ranger.ApiGateway.Tests
{
    [Collection("Validation collection")]
    public class AccountUpdateModelValidatorTests
    {
        private readonly IValidator<AccountUpdateModel> accountUpdateModelValidator;
        public AccountUpdateModelValidatorTests(ValidationFixture fixture)
        {
            this.accountUpdateModelValidator = fixture.serviceProvider.GetRequiredServiceForTest<IValidator<AccountUpdateModel>>();
        }
        [Fact]
        public void FirstNameLastName_Should_Have_Error_When_Empty()
        {
            this.accountUpdateModelValidator.ShouldHaveValidationErrorFor(x => x.FirstName, "");
            this.accountUpdateModelValidator.ShouldHaveValidationErrorFor(x => x.LastName, "");
        }

        [Fact]
        public void FirstNameLastName_Should_Have_Error_When_Greater_Than_48_Characters()
        {
            this.accountUpdateModelValidator.ShouldHaveValidationErrorFor(x => x.FirstName, new String('a', 49));
            this.accountUpdateModelValidator.ShouldHaveValidationErrorFor(x => x.LastName, new String('a', 49));
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
            this.accountUpdateModelValidator.ShouldHaveValidationErrorFor(x => x.FirstName, name);
            this.accountUpdateModelValidator.ShouldHaveValidationErrorFor(x => x.LastName, name);
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
            this.accountUpdateModelValidator.ShouldNotHaveValidationErrorFor(x => x.FirstName, name);
            this.accountUpdateModelValidator.ShouldNotHaveValidationErrorFor(x => x.LastName, name);
        }
    }
}