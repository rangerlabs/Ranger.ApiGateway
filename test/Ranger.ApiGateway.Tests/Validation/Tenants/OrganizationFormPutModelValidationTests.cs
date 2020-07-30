using System;
using FluentValidation;
using FluentValidation.TestHelper;
using Moq;
using Xunit;

namespace Ranger.ApiGateway.Tests {
    [Collection ("Validation collection")]
    public class OrganizationFormPutModelValidationTests {
        private readonly IValidator<OrganizationFormPutModel> organizationPutFormModelValidator;
        public OrganizationFormPutModelValidationTests (ValidationFixture fixture) {
            this.organizationPutFormModelValidator = fixture.serviceProvider.GetRequiredServiceForTest<IValidator<OrganizationFormPutModel>> ();
        }

        [Fact]
        public void Domain_Should_Have_Error_When_Less_Than_3_Characters () {
            this.organizationPutFormModelValidator.ShouldHaveValidationErrorFor (x => x.Domain, new string ('a', 2));
        }

        [Fact]
        public void Domain_Should_Have_Error_When_Greater_Than_28_Characters () {
            this.organizationPutFormModelValidator.ShouldHaveValidationErrorFor (x => x.Domain, new String ('a', 29));
        }

        [Fact]
        public void Domain_Should_NOT_Have_Error_When_Between_3_AND_28_Characters () {
            this.organizationPutFormModelValidator.ShouldNotHaveValidationErrorFor (x => x.Domain, new String ('a', 20));
        }

        [Theory]
        [InlineData ("-")]
        [InlineData ("-a0-")]
        [InlineData ("-A0-")]
        [InlineData ("-a!0-")]
        public void Domain_Should_Have_Error_When_Fails_Regex (string externalId) {
            this.organizationPutFormModelValidator.ShouldHaveValidationErrorFor (g => g.Domain, externalId);
        }

        [Theory]
        [InlineData ("a-0")]
        [InlineData ("0-a")]
        [InlineData ("aa0")]
        [InlineData ("a00")]
        [InlineData ("0aa")]
        [InlineData ("00a")]
        public void Domain_Should_NOT_Have_Error_When_Passes_Regex (string externalId) {
            this.organizationPutFormModelValidator.ShouldNotHaveValidationErrorFor (g => g.Domain, externalId);
        }

        [Fact]
        public void OrganizationName_Should_Have_Error_When_Less_Than_3_Characters () {
            this.organizationPutFormModelValidator.ShouldHaveValidationErrorFor (x => x.OrganizationName, new string ('a', 2));
        }

        [Fact]
        public void OrganizationName_Should_Have_Error_When_Greater_Than_48_Characters () {
            this.organizationPutFormModelValidator.ShouldHaveValidationErrorFor (x => x.OrganizationName, new String ('a', 49));
        }

        [Fact]
        public void OrganizationName_Should_NOT_Have_Error_When_Between_3_AND_48_Characters () {
            this.organizationPutFormModelValidator.ShouldNotHaveValidationErrorFor (x => x.OrganizationName, new String ('a', 20));
        }

        [Theory]
        [InlineData ("-")]
        [InlineData ("-a0-")]
        [InlineData ("-A0-")]
        [InlineData ("-a!0-")]
        [InlineData (" a!0-")]
        [InlineData ("a!0 ")]
        [InlineData ("a0_")]
        [InlineData ("_a0")]
        public void OrganizationName_Should_Have_Error_When_Fails_Regex (string externalId) {
            this.organizationPutFormModelValidator.ShouldHaveValidationErrorFor (g => g.OrganizationName, externalId);
        }

        [Theory]
        [InlineData ("a-0")]
        [InlineData ("0-a")]
        [InlineData ("0_a")]
        [InlineData ("0 a")]
        [InlineData ("aa0")]
        [InlineData ("a00")]
        [InlineData ("0aa")]
        [InlineData ("00a")]
        public void OrganizationName_Should_NOT_Have_Error_When_Passes_Regex (string externalId) {
            this.organizationPutFormModelValidator.ShouldNotHaveValidationErrorFor (g => g.OrganizationName, externalId);
        }

        [Fact]
        public void Version_Should_Have_Error_When_Less_Than_1 () {
            this.organizationPutFormModelValidator.ShouldHaveValidationErrorFor (x => x.Version, 0);
        }

        [Fact]
        public void Version_Should_Not_Have_Error_When_Greater_Than_Equal_To_1 () {
            this.organizationPutFormModelValidator.ShouldNotHaveValidationErrorFor (x => x.Version, 1);
            this.organizationPutFormModelValidator.ShouldNotHaveValidationErrorFor (x => x.Version, 2);
        }
    }
}