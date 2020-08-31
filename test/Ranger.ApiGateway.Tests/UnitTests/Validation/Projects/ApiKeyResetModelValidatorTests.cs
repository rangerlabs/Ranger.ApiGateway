using FluentValidation;
using FluentValidation.TestHelper;
using Shouldly;
using Xunit;

namespace Ranger.ApiGateway.Tests.Validation.Projects
{
    [Collection("Validation collection")]
    public class ApiKeyResetModelValidatorTests
    {
        private readonly IValidator<ApiKeyResetModel> putProjectModelValidator;
        public ApiKeyResetModelValidatorTests(ValidationFixture fixture)
        {
            this.putProjectModelValidator = fixture.serviceProvider.GetRequiredServiceForTest<IValidator<ApiKeyResetModel>>();
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