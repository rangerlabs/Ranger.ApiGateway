using System;
using System.Collections.Generic;
using FluentValidation;
using FluentValidation.TestHelper;
using Shouldly;
using Xunit;

namespace Ranger.ApiGateway.Tests.Validation.Integrations.Webhooks
{
    [Collection("Validation collection")]
    public class WebhookIntegrationPutModelValidatorTests
    {
        private readonly IValidator<WebhookIntegrationPutModel> webhookIntegrationPutModelValidatorTests;
        public WebhookIntegrationPutModelValidatorTests(ValidationFixture fixture)
        {
            this.webhookIntegrationPutModelValidatorTests = fixture.serviceProvider.GetRequiredServiceForTest<IValidator<WebhookIntegrationPutModel>>();
        }

        [Fact]
        public void IntegrationId_Should_Have_Error_When_Empty()
        {
            this.webhookIntegrationPutModelValidatorTests.ShouldHaveValidationErrorFor(x => x.IntegrationId, default(Guid));
        }

        [Fact]
        public void IntegrationId_Should_Not_Have_Error_When_Not_Empty()
        {
            this.webhookIntegrationPutModelValidatorTests.ShouldNotHaveValidationErrorFor(x => x.IntegrationId, Guid.NewGuid());
        }

        [Fact]
        public void Name_Should_Have_Error_When_Less_Than_2_Characters()
        {
            this.webhookIntegrationPutModelValidatorTests.ShouldHaveValidationErrorFor(g => g.Name, "a");
        }

        [Fact]
        public void Name_Should_Have_Error_When_Empty()
        {
            this.webhookIntegrationPutModelValidatorTests.ShouldHaveValidationErrorFor(g => g.Name, "");
        }

        [Fact]
        public void Name_Should_Have_Error_When_Greater_Than_128_Characters()
        {

            this.webhookIntegrationPutModelValidatorTests.ShouldHaveValidationErrorFor(g => g.Name, new string('a', 129));
        }

        [Fact]
        public void Name_Should_Have_No_Error_When_128_Characters_Or_Less()
        {
            this.webhookIntegrationPutModelValidatorTests.ShouldNotHaveValidationErrorFor(g => g.Name, new string('a', 128));
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
            this.webhookIntegrationPutModelValidatorTests.ShouldHaveValidationErrorFor(g => g.Name, name);
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
            this.webhookIntegrationPutModelValidatorTests.ShouldNotHaveValidationErrorFor(g => g.Name, name);
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
            this.webhookIntegrationPutModelValidatorTests.ShouldHaveValidationErrorFor(x => x.Metadata, metadata);
            this.webhookIntegrationPutModelValidatorTests.ShouldHaveValidationErrorFor(x => x.Headers, metadata);
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
            this.webhookIntegrationPutModelValidatorTests.ShouldNotHaveValidationErrorFor(x => x.Metadata, metadata);
            this.webhookIntegrationPutModelValidatorTests.ShouldNotHaveValidationErrorFor(x => x.Headers, metadata);
        }

        [Fact]
        public void Metadata_AND_Headers_Should_Have_Child_Validator()
        {
            this.webhookIntegrationPutModelValidatorTests.ShouldHaveChildValidator(x => x.Metadata, typeof(IValidator<KeyValuePair<string, string>>));
            this.webhookIntegrationPutModelValidatorTests.ShouldHaveChildValidator(x => x.Headers, typeof(IValidator<KeyValuePair<string, string>>));
        }

        [Fact]
        public void Enabled_Should_Default_To_True()
        {
            var model = new WebhookIntegrationPostModel();
            model.Enabled.ShouldBeTrue();
        }

        [Fact]
        public void Url_Should_Have_Error_When_Empty()
        {
            this.webhookIntegrationPutModelValidatorTests.ShouldHaveValidationErrorFor(x => x.Url, "");
        }

        [Fact]
        public void Url_Should_Not_Have_Error_When_HTTPS_Url()
        {
            this.webhookIntegrationPutModelValidatorTests.ShouldNotHaveValidationErrorFor(x => x.Url, "https://google.com");
        }

        [Fact]
        public void Description_Should_Have_Error_When_Greater_Than_512_Characters()
        {
            this.webhookIntegrationPutModelValidatorTests.ShouldHaveValidationErrorFor(x => x.Description, new string('a', 513));
        }

        [Fact]
        public void Description_Should_Not_Have_Error_When_Less_Than_Or_Equal_To_512_Characters()
        {
            this.webhookIntegrationPutModelValidatorTests.ShouldNotHaveValidationErrorFor(x => x.Description, new string('a', 512));
            this.webhookIntegrationPutModelValidatorTests.ShouldNotHaveValidationErrorFor(x => x.Description, new string('a', 511));
        }

        [Fact]
        public void Version_Should_Have_Error_When_Less_Than_1()
        {
            this.webhookIntegrationPutModelValidatorTests.ShouldHaveValidationErrorFor(x => x.Version, 0);
        }

        [Fact]
        public void Version_Should_Not_Have_Error_When_Greater_Than_Equal_To_1()
        {
            this.webhookIntegrationPutModelValidatorTests.ShouldNotHaveValidationErrorFor(x => x.Version, 1);
            this.webhookIntegrationPutModelValidatorTests.ShouldNotHaveValidationErrorFor(x => x.Version, 2);
        }
    }
}