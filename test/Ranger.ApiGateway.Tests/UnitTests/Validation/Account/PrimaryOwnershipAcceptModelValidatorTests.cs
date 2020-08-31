using System;
using FluentValidation;
using FluentValidation.TestHelper;
using Xunit;

namespace Ranger.ApiGateway.Tests
{
    [Collection("Validation collection")]
    public class PrimaryOwnershipAcceptModelValidatorTests
    {
        private readonly IValidator<PrimaryOwnershipAcceptModel> primaryOwnershipAcceptModelValidator;
        public PrimaryOwnershipAcceptModelValidatorTests(ValidationFixture fixture)
        {
            this.primaryOwnershipAcceptModelValidator = fixture.serviceProvider.GetRequiredServiceForTest<IValidator<PrimaryOwnershipAcceptModel>>();
        }

        [Fact]
        public void CorrelationId_Should_Have_Error_When_Empty()
        {
            this.primaryOwnershipAcceptModelValidator.ShouldHaveValidationErrorFor(x => x.CorrelationId, default(Guid));
        }

        [Fact]
        public void CorrelationId_Should_NOT_Have_Error_When_GUID()
        {
            this.primaryOwnershipAcceptModelValidator.ShouldNotHaveValidationErrorFor(x => x.CorrelationId, Guid.NewGuid());
        }

        [Fact]
        public void Token_Should_Have_Error_When_Empty()
        {
            this.primaryOwnershipAcceptModelValidator.ShouldHaveValidationErrorFor(x => x.Token, "");
        }

        [Fact]
        public void Token_Should_NOT_Have_Error_When_Not_Empty()
        {
            this.primaryOwnershipAcceptModelValidator.ShouldNotHaveValidationErrorFor(x => x.Token, "a");
        }
    }
}