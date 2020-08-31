using System;
using System.Collections.Generic;
using FluentValidation;
using FluentValidation.TestHelper;
using Xunit;

namespace Ranger.ApiGateway.Tests
{
    [Collection("Validation collection")]
    public class PostApplicationUserModelValidatorTests
    {
        private readonly IValidator<PostApplicationUserModel> postApplicationUserModelValidator;
        public PostApplicationUserModelValidatorTests(ValidationFixture fixture)
        {
            this.postApplicationUserModelValidator = fixture.serviceProvider.GetRequiredServiceForTest<IValidator<PostApplicationUserModel>>();
        }

        [Fact]
        public void Email_Should_Have_Error_When_Empty()
        {
            this.postApplicationUserModelValidator.ShouldHaveValidationErrorFor(x => x.Email, "");
        }

        [Fact]
        public void Email_Should_Have_Error_When_No_Ampersand()
        {
            this.postApplicationUserModelValidator.ShouldHaveValidationErrorFor(x => x.Email, new string('a', 2));
        }

        [Fact]
        public void Email_Should_NOT_Have_Error_When_Has_Ampersand()
        {
            this.postApplicationUserModelValidator.ShouldNotHaveValidationErrorFor(x => x.Email, "a@a");
        }

        [Fact]
        public void FirstNameLastName_Should_Have_Error_When_Empty()
        {
            this.postApplicationUserModelValidator.ShouldHaveValidationErrorFor(x => x.FirstName, "");
            this.postApplicationUserModelValidator.ShouldHaveValidationErrorFor(x => x.LastName, "");
        }

        [Fact]
        public void FirstNameLastName_Should_Have_Error_When_Greater_Than_48_Characters()
        {
            this.postApplicationUserModelValidator.ShouldHaveValidationErrorFor(x => x.FirstName, new String('a', 49));
            this.postApplicationUserModelValidator.ShouldHaveValidationErrorFor(x => x.LastName, new String('a', 49));
        }

        [Theory]
        [InlineData("0!")]
        [InlineData("0")]
        [InlineData("N0")]
        [InlineData("N!")]
        [InlineData("N--")]
        [InlineData("N__")]
        [InlineData("N,,")]
        [InlineData("N''")]
        [InlineData("N..")]
        [InlineData("n ")]
        [InlineData("n N")]
        [InlineData(" N")]
        public void FirstNameLastName_Should_Have_Error_When_Fails_Regex(string name)
        {
            this.postApplicationUserModelValidator.ShouldHaveValidationErrorFor(x => x.FirstName, name);
            this.postApplicationUserModelValidator.ShouldHaveValidationErrorFor(x => x.LastName, name);
        }

        [Theory]
        [InlineData("n")]
        [InlineData("N")]
        [InlineData("N-")]
        [InlineData("N_")]
        [InlineData("N,")]
        [InlineData("N.")]
        [InlineData("N'")]
        public void FirstNameLastName_Should_NOT_Have_Error_When_Passes_Regex(string name)
        {
            this.postApplicationUserModelValidator.ShouldNotHaveValidationErrorFor(x => x.FirstName, name);
            this.postApplicationUserModelValidator.ShouldNotHaveValidationErrorFor(x => x.LastName, name);
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

            this.postApplicationUserModelValidator.ShouldHaveValidationErrorFor(x => x.AuthorizedProjects, authorizedProjects);
        }

        [Fact]
        public void AuthorizedProjects_Should_Not_Have_Error_When_Has_No_Duplicate_Ids()
        {
            var authorizedProjects = new List<Guid>()
            {
                Guid.NewGuid()
            };

            this.postApplicationUserModelValidator.ShouldNotHaveValidationErrorFor(x => x.AuthorizedProjects, authorizedProjects);
        }
    }
}