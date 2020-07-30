using System;
using FluentValidation;
using FluentValidation.TestHelper;
using Xunit;

namespace Ranger.ApiGateway.Tests
{
    [Collection("Validation collection")]
    public class NewTenantPostModelValidationTests
    {
        private readonly IValidator<NewTenantPostModel> organizationPutFormModelValidator;
        public NewTenantPostModelValidationTests(ValidationFixture fixture)
        {
            this.organizationPutFormModelValidator = fixture.serviceProvider.GetRequiredServiceForTest<IValidator<NewTenantPostModel>>();
        }

        [Fact]
        public void OrganizationForm_Should_Have_Child_Validator()
        {
            this.organizationPutFormModelValidator.ShouldHaveChildValidator(x => x.OrganizationForm, typeof(IValidator<OrganizationFormPostModel>));
        }

        [Fact]
        public void UserForm_Should_Have_Child_Validator()
        {
            this.organizationPutFormModelValidator.ShouldHaveChildValidator(x => x.UserForm, typeof(IValidator<UserForm>));
        }
    }
}