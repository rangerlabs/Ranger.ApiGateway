using System;
using FluentValidation;
using FluentValidation.TestHelper;
using Xunit;

namespace Ranger.ApiGateway.Tests
{
    [Collection("Validation collection")]
    public class PrimaryOwnershipRefuseModelValidatorTests
    {
        private readonly IValidator<PrimaryOwnershipRefuseModel> primaryOwnershipRefuseModelValidator;
        public PrimaryOwnershipRefuseModelValidatorTests(ValidationFixture fixture)
        {
            this.primaryOwnershipRefuseModelValidator = fixture.serviceProvider.GetRequiredServiceForTest<IValidator<PrimaryOwnershipRefuseModel>>();
        }

        [Fact]
        public void CorrelationId_Should_Have_Error_When_Empty()
        {
            this.primaryOwnershipRefuseModelValidator.ShouldHaveValidationErrorFor(x => x.CorrelationId, default(Guid));
        }

        [Fact]
        public void CorrelationId_Should_NOT_Have_Error_When_GUID()
        {
            this.primaryOwnershipRefuseModelValidator.ShouldNotHaveValidationErrorFor(x => x.CorrelationId, Guid.NewGuid());
        }
    }
}