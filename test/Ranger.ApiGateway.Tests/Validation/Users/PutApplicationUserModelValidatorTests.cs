using System;
using System.Collections.Generic;
using FluentValidation;
using FluentValidation.TestHelper;
using Xunit;

namespace Ranger.ApiGateway.Tests
{
    [Collection("Validation collection")]
    public class PutApplicationUserModelValidatorTests
    {
        private readonly IValidator<PutPermissionsModel> putApplicationUserModelValidator;
        public PutApplicationUserModelValidatorTests(ValidationFixture fixture)
        {
            this.putApplicationUserModelValidator = fixture.serviceProvider.GetRequiredServiceForTest<IValidator<PutPermissionsModel>>();
        }


        [Fact]
        public void AuthorizedProjects_Should_Have_Error_When_Has_Duplicate_Ids()
        {
            var guid = Guid.NewGuid();
            var authorizedProjects = new List<Guid>()
            {
                guid,
                guid
            };

            this.putApplicationUserModelValidator.ShouldHaveValidationErrorFor(x => x.AuthorizedProjects, authorizedProjects);
        }

        [Fact]
        public void AuthorizedProjects_Should_Not_Have_Error_When_Has_No_Duplicate_Ids()
        {
            var authorizedProjects = new List<Guid>()
            {
                Guid.NewGuid()
            };

            this.putApplicationUserModelValidator.ShouldNotHaveValidationErrorFor(x => x.AuthorizedProjects, authorizedProjects);
        }
    }
}