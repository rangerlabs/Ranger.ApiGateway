using FluentValidation;
using FluentValidation.TestHelper;
using Shouldly;
using Xunit;

namespace Ranger.ApiGateway.Tests.Validation.Projects
{
    [Collection("Validation collection")]
    public class PutProjectModelValidationTests
    {
        private readonly IValidator<PutProjectModel> putProjectModelValidator;
        public PutProjectModelValidationTests(ValidationFixture fixture)
        {
            this.putProjectModelValidator = fixture.serviceProvider.GetRequiredServiceForTest<IValidator<PutProjectModel>>();
        }

        [Fact]
        public void Name_Should_Have_Error_When_Name_Less_Than_3_Characters()
        {
            this.putProjectModelValidator.ShouldHaveValidationErrorFor(x => x.Name, new string('a', 2));
        }

        [Fact]
        public void Name_Should_Have_Error_When_Name_Greater_Than_128_Characters()
        {
            this.putProjectModelValidator.ShouldHaveValidationErrorFor(x => x.Name, new string('a', 129));
        }

        [Fact]
        public void Name_Should_Not_Have_Error_When_Name_Between_3_And_128_Characters()
        {
            this.putProjectModelValidator.ShouldNotHaveValidationErrorFor(x => x.Name, new string('a', 100));
        }

        [Theory]
        [InlineData("-")] //cannot be only special
        [InlineData("-a0-")] //cannot begin, end with special
        [InlineData("-A0-")] //cannot begin, end with special
        [InlineData("-a!0-")] //cannot being, end, invalid special
        [InlineData(" a!0-")] //cannot start with space
        [InlineData("a!0 ")] //cannot end with space
        [InlineData("a0_")] //cannot end with special
        [InlineData("_a0")] //cannot begin with special
        public void Name_Should_Have_Error_When_Fails_Regex(string name)
        {
            this.putProjectModelValidator.ShouldHaveValidationErrorFor(x => x.Name, name);
        }

        [Theory]
        [InlineData("a-0")]
        [InlineData("0-a")]
        [InlineData("0_a")]
        [InlineData("0 a")]
        [InlineData("aa0")]
        [InlineData("a00")]
        [InlineData("0aa")]
        [InlineData("00a")]
        public void Name_Should_NOT_Have_Error_When_Passes_Regex(string name)
        {
            this.putProjectModelValidator.ShouldNotHaveValidationErrorFor(g => g.Name, name);
        }

        [Fact]
        public void Enabled_Defaults_To_True()
        {
            var model = new PostProjectModel();
            model.Enabled.ShouldBeTrue();
        }

        [Fact]
        public void Description_Should_Have_Error_When_Greater_Than_512_Characters()
        {
            this.putProjectModelValidator.ShouldHaveValidationErrorFor(x => x.Description, new string('a', 513));
        }

        [Fact]
        public void Description_Should_NOT_Have_Error_When_Less_Than_Or_Equal_To_512_Characters()
        {
            this.putProjectModelValidator.ShouldNotHaveValidationErrorFor(x => x.Description, new string('a', 512));
            this.putProjectModelValidator.ShouldNotHaveValidationErrorFor(x => x.Description, new string('a', 511));
        }

        [Fact]
        public void Version_Should_Have_Error_When_Less_Than_1()
        {
            this.putProjectModelValidator.ShouldHaveValidationErrorFor(x => x.Version, 0);
        }

        [Fact]
        public void Version_Should_NOT_Have_Error_When_Greater_Than_Equal_To_1()
        {
            this.putProjectModelValidator.ShouldNotHaveValidationErrorFor(x => x.Version, 1);
            this.putProjectModelValidator.ShouldNotHaveValidationErrorFor(x => x.Version, 2);
        }
    }
}