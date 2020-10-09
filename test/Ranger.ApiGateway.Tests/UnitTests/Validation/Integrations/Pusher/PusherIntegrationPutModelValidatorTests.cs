using System;
using System.Collections.Generic;
using FluentValidation;
using FluentValidation.TestHelper;
using Shouldly;
using Xunit;

namespace Ranger.ApiGateway.Tests.Validation.Integrations.Pushers
{
    [Collection("Validation collection")]
    public class PusherIntegrationPutModelValidatorTests
    {
        private readonly IValidator<PusherIntegrationPutModel> pusherIntegrationPutModelValidatorTests;
        public PusherIntegrationPutModelValidatorTests(ValidationFixture fixture)
        {
            this.pusherIntegrationPutModelValidatorTests = fixture.serviceProvider.GetRequiredServiceForTest<IValidator<PusherIntegrationPutModel>>();
        }

        [Fact]
        public void IntegrationId_Should_NOT_Have_Error_When_Empty()
        {
            this.pusherIntegrationPutModelValidatorTests.ShouldNotHaveValidationErrorFor(x => x.Id, default(Guid));
        }

        [Fact]
        public void Name_Should_Have_Error_When_Less_Than_2_Characters()
        {
            this.pusherIntegrationPutModelValidatorTests.ShouldHaveValidationErrorFor(g => g.Name, "a");
        }

        [Fact]
        public void Name_Should_Have_Error_When_Empty()
        {
            this.pusherIntegrationPutModelValidatorTests.ShouldHaveValidationErrorFor(g => g.Name, "");
        }

        [Fact]
        public void Name_Should_Have_Error_When_Greater_Than_128_Characters()
        {

            this.pusherIntegrationPutModelValidatorTests.ShouldHaveValidationErrorFor(g => g.Name, new string('a', 129));
        }

        [Fact]
        public void Name_Should_Have_No_Error_When_128_Characters_Or_Less()
        {
            this.pusherIntegrationPutModelValidatorTests.ShouldNotHaveValidationErrorFor(g => g.Name, new string('a', 128));
        }

        [Theory]
        [InlineData("-")]
        [InlineData("-a0-")]
        [InlineData("-A0-")]
        [InlineData("-a!0-")]
        [InlineData(" a!0-")]
        [InlineData("a!0 ")]
        public void Name_Should_Have_Error_When_Fails_Regex(string name)
        {
            this.pusherIntegrationPutModelValidatorTests.ShouldHaveValidationErrorFor(g => g.Name, name);
        }

        [Theory]
        [InlineData("a-0")]
        [InlineData("0-a")]
        [InlineData("aa0")]
        [InlineData("a00")]
        [InlineData("0aa")]
        [InlineData("00a")]
        public void Name_Should_NOT_Have_Error_When_Passes_Regex(string name)
        {
            this.pusherIntegrationPutModelValidatorTests.ShouldNotHaveValidationErrorFor(g => g.Name, name);
        }

        [Fact]
        public void MetaData_AND_HEADER_Should_Have_Error_When_More_Than_16_Elements()
        {
            var metadata = new List<KeyValuePair<string, string>>()
                {
                    new KeyValuePair<string, string>("a","a"),
                    new KeyValuePair<string, string>("a","a"),
                    new KeyValuePair<string, string>("a","a"),
                    new KeyValuePair<string, string>("a","a"),
                    new KeyValuePair<string, string>("a","a"),
                    new KeyValuePair<string, string>("a","a"),
                    new KeyValuePair<string, string>("a","a"),
                    new KeyValuePair<string, string>("a","a"),
                    new KeyValuePair<string, string>("a","a"),
                    new KeyValuePair<string, string>("a","a"),
                    new KeyValuePair<string, string>("a","a"),
                    new KeyValuePair<string, string>("a","a"),
                    new KeyValuePair<string, string>("a","a"),
                    new KeyValuePair<string, string>("a","a"),
                    new KeyValuePair<string, string>("a","a"),
                    new KeyValuePair<string, string>("a","a"),

                    new KeyValuePair<string, string>("a","a"),
                };
            this.pusherIntegrationPutModelValidatorTests.ShouldHaveValidationErrorFor(x => x.Metadata, metadata);
        }

        [Fact]
        public void MetaData_AND_Headers_Should_NOT_Have_Error_When_Less_Than_Equal_To_16_Elements()
        {
            var metadata = new List<KeyValuePair<string, string>>()
                {
                    new KeyValuePair<string, string>("a","a"),
                    new KeyValuePair<string, string>("a","a"),
                    new KeyValuePair<string, string>("a","a"),
                    new KeyValuePair<string, string>("a","a"),
                    new KeyValuePair<string, string>("a","a"),
                    new KeyValuePair<string, string>("a","a"),
                    new KeyValuePair<string, string>("a","a"),
                    new KeyValuePair<string, string>("a","a"),
                    new KeyValuePair<string, string>("a","a"),
                    new KeyValuePair<string, string>("a","a"),
                    new KeyValuePair<string, string>("a","a"),
                    new KeyValuePair<string, string>("a","a"),
                    new KeyValuePair<string, string>("a","a"),
                    new KeyValuePair<string, string>("a","a"),
                    new KeyValuePair<string, string>("a","a"),
                    new KeyValuePair<string, string>("a","a"),
               };
            this.pusherIntegrationPutModelValidatorTests.ShouldNotHaveValidationErrorFor(x => x.Metadata, metadata);
        }

        [Fact]
        public void Metadata_AND_Headers_Should_Have_Child_Validator()
        {
            this.pusherIntegrationPutModelValidatorTests.ShouldHaveChildValidator(x => x.Metadata, typeof(IValidator<KeyValuePair<string, string>>));
        }

        [Fact]
        public void Enabled_Should_Default_To_True()
        {
            var model = new PusherIntegrationPostModel();
            model.Enabled.ShouldBeTrue();
        }

        [Fact]
        public void IsDefault_Should_Default_To_False()
        {
            var model = new PusherIntegrationPostModel();
            model.IsDefault.ShouldBeFalse();
        }

       
        [Fact]
        public void AppId_Should_Have_Error_When_Empty()
        {
            this.pusherIntegrationPutModelValidatorTests.ShouldHaveValidationErrorFor(x => x.AppId, "");
        }

        [Fact]
        public void AppId_Should_Have_Error_When_Greater_Than_32_Characters()
        {
            this.pusherIntegrationPutModelValidatorTests.ShouldHaveValidationErrorFor(x => x.AppId, new string('a', 33));
        }

        [Fact]
        public void AppId_Should_Not_Have_Error_When_Less_Than_Or_Equal_To_32_Characters()
        {
            this.pusherIntegrationPutModelValidatorTests.ShouldNotHaveValidationErrorFor(x => x.AppId, new string('a', 32));
            this.pusherIntegrationPutModelValidatorTests.ShouldNotHaveValidationErrorFor(x => x.AppId, new string('a', 31));
        }


        [Fact]
        public void Key_Should_Have_Error_When_Empty()
        {
            this.pusherIntegrationPutModelValidatorTests.ShouldHaveValidationErrorFor(x => x.Key, "");
        }

        [Fact]
        public void Key_Should_Have_Error_When_Greater_Than_64_Characters()
        {
            this.pusherIntegrationPutModelValidatorTests.ShouldHaveValidationErrorFor(x => x.Key, new string('a', 65));
        }

        [Fact]
        public void Key_Should_Not_Have_Error_When_Less_Than_Or_Equal_To_64_Characters()
        {
            this.pusherIntegrationPutModelValidatorTests.ShouldNotHaveValidationErrorFor(x => x.Key, new string('a', 64));
            this.pusherIntegrationPutModelValidatorTests.ShouldNotHaveValidationErrorFor(x => x.Key, new string('a', 63));
        }

        [Fact]
        public void Secret_Should_Have_Error_When_Empty()
        {
            this.pusherIntegrationPutModelValidatorTests.ShouldHaveValidationErrorFor(x => x.Secret, "");
        }

        [Fact]
        public void Secret_Should_Have_Error_When_Greater_Than_64_Characters()
        {
            this.pusherIntegrationPutModelValidatorTests.ShouldHaveValidationErrorFor(x => x.Secret, new string('a', 65));
        }

        [Fact]
        public void Secret_Should_Not_Have_Error_When_Less_Than_Or_Equal_To_64_Characters()
        {
            this.pusherIntegrationPutModelValidatorTests.ShouldNotHaveValidationErrorFor(x => x.Secret, new string('a', 64));
            this.pusherIntegrationPutModelValidatorTests.ShouldNotHaveValidationErrorFor(x => x.Secret, new string('a', 63));
        }

        [Fact]
        public void Cluster_Should_Have_Error_When_Empty()
        {
            this.pusherIntegrationPutModelValidatorTests.ShouldHaveValidationErrorFor(x => x.Cluster, "");
        }

        [Fact]
        public void Cluster_Should_Have_Error_When_Greater_Than_8_Characters()
        {
            this.pusherIntegrationPutModelValidatorTests.ShouldHaveValidationErrorFor(x => x.Cluster, new string('a', 9));
        }

        [Fact]
        public void Cluster_Should_Not_Have_Error_When_Less_Than_Or_Equal_To_8_Characters()
        {
            this.pusherIntegrationPutModelValidatorTests.ShouldNotHaveValidationErrorFor(x => x.Cluster, new string('a', 8));
            this.pusherIntegrationPutModelValidatorTests.ShouldNotHaveValidationErrorFor(x => x.Cluster, new string('a', 7));
        }

        [Fact]
        public void Description_Should_Have_Error_When_Greater_Than_512_Characters()
        {
            this.pusherIntegrationPutModelValidatorTests.ShouldHaveValidationErrorFor(x => x.Description, new string('a', 513));
        }

        [Fact]
        public void Description_Should_Not_Have_Error_When_Less_Than_Or_Equal_To_512_Characters()
        {
            this.pusherIntegrationPutModelValidatorTests.ShouldNotHaveValidationErrorFor(x => x.Description, new string('a', 512));
            this.pusherIntegrationPutModelValidatorTests.ShouldNotHaveValidationErrorFor(x => x.Description, new string('a', 511));
        }

        [Fact]
        public void Version_Should_Have_Error_When_Less_Than_1()
        {
            this.pusherIntegrationPutModelValidatorTests.ShouldHaveValidationErrorFor(x => x.Version, 0);
        }

        [Fact]
        public void Version_Should_Not_Have_Error_When_Greater_Than_Equal_To_1()
        {
            this.pusherIntegrationPutModelValidatorTests.ShouldNotHaveValidationErrorFor(x => x.Version, 1);
            this.pusherIntegrationPutModelValidatorTests.ShouldNotHaveValidationErrorFor(x => x.Version, 2);
        }
    }
}