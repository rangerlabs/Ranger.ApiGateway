using FluentValidation;
using FluentValidation.TestHelper;
using Shouldly;
using Xunit;

namespace Ranger.ApiGateway.Tests.Validation.Projects
{
    [Collection("Validation collection")]
    public class PostProjectModelValidationTests
    {
        private readonly IValidator<PostProjectModel> postProjectModelValidator;
        public PostProjectModelValidationTests(ValidationFixture fixture)
        {
            this.postProjectModelValidator = fixture.serviceProvider.GetRequiredServiceForTest<IValidator<PostProjectModel>>();
        }

        [Fact]
        public void Name_Should_Have_Error_When_Name_Less_Than_3_Characters()
        {
            this.postProjectModelValidator.ShouldHaveValidationErrorFor(x => x.Name, new string('a', 2));
        }

        [Fact]
        public void Name_Should_Have_Error_When_Name_Greater_Than_128_Characters()
        {
            this.postProjectModelValidator.ShouldHaveValidationErrorFor(x => x.Name, new string('a', 129));
        }

        [Fact]
        public void Name_Should_Not_Have_Error_When_Name_Between_3_And_128_Characters()
        {
            this.postProjectModelValidator.ShouldNotHaveValidationErrorFor(x => x.Name, new string('a', 100));
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
            this.postProjectModelValidator.ShouldHaveValidationErrorFor(x => x.Name, name);
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
            this.postProjectModelValidator.ShouldNotHaveValidationErrorFor(g => g.Name, name);
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
            this.postProjectModelValidator.ShouldHaveValidationErrorFor(x => x.Description, new string('a', 513));
        }

        [Fact]
        public void Description_Should_NOT_Have_Error_When_Less_Than_Or_Equal_To_512_Characters()
        {
            this.postProjectModelValidator.ShouldNotHaveValidationErrorFor(x => x.Description, new string('a', 512));
            this.postProjectModelValidator.ShouldNotHaveValidationErrorFor(x => x.Description, new string('a', 511));
        }


    }
}