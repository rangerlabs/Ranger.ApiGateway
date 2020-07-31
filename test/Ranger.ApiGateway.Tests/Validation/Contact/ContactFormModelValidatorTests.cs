using FluentValidation;
using FluentValidation.TestHelper;
using Xunit;

namespace Ranger.ApiGateway.Tests.Validation.Users
{
    [Collection("Validation collection")]
    public class ContactFormModelValidatorTests
    {
        private readonly IValidator<ContactFormModel> contactFormModelValidator;
        public ContactFormModelValidatorTests(ValidationFixture fixture)
        {
            this.contactFormModelValidator = fixture.serviceProvider.GetRequiredServiceForTest<IValidator<ContactFormModel>>();
        }

        [Fact]
        public void Email_Should_Have_Error_When_Empty()
        {
            this.contactFormModelValidator.ShouldHaveValidationErrorFor(x => x.Email, "");
        }

        [Fact]
        public void Email_Should_Have_Error_When_No_Ampersand()
        {
            this.contactFormModelValidator.ShouldHaveValidationErrorFor(x => x.Email, "aa");
        }

        [Fact]
        public void Email_Should_NOT_Have_Error_When_Has_Ampersand()
        {
            this.contactFormModelValidator.ShouldNotHaveValidationErrorFor(x => x.Email, "a@a");
        }

        [Fact]
        public void Organization_Should_Have_Error_When_Empty()
        {
            this.contactFormModelValidator.ShouldHaveValidationErrorFor(x => x.Organization, "");
        }

        [Fact]
        public void Organization_Should_Not_Have_Error_When_Less_Than_Or_Equal_To_512_Characters()
        {
            this.contactFormModelValidator.ShouldNotHaveValidationErrorFor(x => x.Organization, new string('a', 512));
            this.contactFormModelValidator.ShouldNotHaveValidationErrorFor(x => x.Organization, new string('a', 511));
        }

        [Fact]
        public void Organization_Should_Have_Error_When_Greater_Than_512_Characters()
        {
            this.contactFormModelValidator.ShouldHaveValidationErrorFor(x => x.Organization, new string('a', 513));
        }

        [Fact]
        public void Name_Should_Have_Error_When_Empty()
        {
            this.contactFormModelValidator.ShouldHaveValidationErrorFor(x => x.Name, "");
        }

        [Fact]
        public void Name_Should_Not_Have_Error_When_Less_Than_Or_Equal_To_512_Characters()
        {
            this.contactFormModelValidator.ShouldNotHaveValidationErrorFor(x => x.Name, new string('a', 512));
            this.contactFormModelValidator.ShouldNotHaveValidationErrorFor(x => x.Name, new string('a', 511));
        }

        [Fact]
        public void Name_Should_Have_Error_When_Greater_Than_512_Characters()
        {
            this.contactFormModelValidator.ShouldHaveValidationErrorFor(x => x.Name, new string('a', 513));
        }


        [Fact]
        public void Message_Should_Have_Error_When_Empty()
        {
            this.contactFormModelValidator.ShouldHaveValidationErrorFor(x => x.Message, "");
        }

        [Fact]
        public void Message_Should_Not_Have_Error_When_Less_Than_Or_Equal_To_1000_Characters()
        {
            this.contactFormModelValidator.ShouldNotHaveValidationErrorFor(x => x.Message, new string('a', 1000));
            this.contactFormModelValidator.ShouldNotHaveValidationErrorFor(x => x.Message, new string('a', 999));
        }

        [Fact]
        public void Message_Should_Have_Error_When_Greater_Than_1000_Characters()
        {
            this.contactFormModelValidator.ShouldHaveValidationErrorFor(x => x.Message, new string('a', 1001));
        }
    }
}