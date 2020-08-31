using System;
using FluentValidation;
using FluentValidation.TestHelper;
using Xunit;

namespace Ranger.ApiGateway.Tests
{
    [Collection("Validation collection")]
    public class TransferPrimaryOwnershipModelValidatorTests
    {
        private readonly IValidator<TransferPrimaryOwnershipModel> transferPrimaryOwnershipModelValidator;
        public TransferPrimaryOwnershipModelValidatorTests(ValidationFixture fixture)
        {
            this.transferPrimaryOwnershipModelValidator = fixture.serviceProvider.GetRequiredServiceForTest<IValidator<TransferPrimaryOwnershipModel>>();
        }

        [Fact]
        public void Email_Should_Have_Error_When_Empty()
        {
            this.transferPrimaryOwnershipModelValidator.ShouldHaveValidationErrorFor(x => x.Email, "");
        }

        [Fact]
        public void Email_Should_Have_Error_When_No_Ampersand()
        {
            this.transferPrimaryOwnershipModelValidator.ShouldHaveValidationErrorFor(x => x.Email, new string('a', 2));
        }

        [Fact]
        public void Email_Should_NOT_Have_Error_When_Has_Ampersand()
        {
            this.transferPrimaryOwnershipModelValidator.ShouldNotHaveValidationErrorFor(x => x.Email, "a@a");
        }

    }
}