using System;
using FluentValidation;
using FluentValidation.TestHelper;
using Xunit;

namespace Ranger.ApiGateway.Tests
{
    [Collection("Validation collection")]
    public class CancelPrimaryOwnershipModelValidatorTests
    {
        private readonly IValidator<CancelPrimaryOwnershipModel> cancelPrimaryOwnershipValidator;
        public CancelPrimaryOwnershipModelValidatorTests(ValidationFixture fixture)
        {
            this.cancelPrimaryOwnershipValidator = fixture.serviceProvider.GetRequiredServiceForTest<IValidator<CancelPrimaryOwnershipModel>>();
        }

        [Fact]
        public void CorrelationId_Should_Have_Error_When_Empty()
        {
            this.cancelPrimaryOwnershipValidator.ShouldHaveValidationErrorFor(x => x.CorrelationId, default(Guid));
        }

        [Fact]
        public void Password_Should_NOT_Have_Error_When_GUID()
        {
            this.cancelPrimaryOwnershipValidator.ShouldNotHaveValidationErrorFor(x => x.CorrelationId, Guid.NewGuid());
        }
    }
}