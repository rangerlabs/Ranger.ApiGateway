using System;
using System.Collections.Generic;
using FluentValidation;
using FluentValidation.TestHelper;
using Ranger.Common;
using Xunit;

namespace Ranger.ApiGateway.Tests
{
    [Collection("Validation collection")]
    public class BreadcrumbValidationTests
    {
        private IValidator<BreadcrumbModel> breadcrumbValidator;

        public BreadcrumbValidationTests(ValidationFixture fixture)
        {
            this.breadcrumbValidator = fixture.serviceProvider.GetRequiredServiceForTest<IValidator<BreadcrumbModel>>();
        }

        [Fact]
        public void DeviceId_Should_Have_Error_When_Empty()
        {
            this.breadcrumbValidator.ShouldHaveValidationErrorFor(x => x.DeviceId, "");
        }

        [Fact]
        public void DeviceId_Should_Have_Error_When_Greater_Than_128_Characters()
        {
            this.breadcrumbValidator.ShouldHaveValidationErrorFor(x => x.DeviceId, new string('a', 129));
        }

        [Fact]
        public void DeviceId_Should_NOT_Have_Error_When_Less_Than_Equal_To_128_Characters()
        {
            this.breadcrumbValidator.ShouldNotHaveValidationErrorFor(x => x.DeviceId, new string('a', 127));
            this.breadcrumbValidator.ShouldNotHaveValidationErrorFor(x => x.DeviceId, new string('a', 128));
        }

        [Fact]
        public void ExternalUserId_Should_NOT_Have_Error_When_Empty()
        {
            this.breadcrumbValidator.ShouldNotHaveValidationErrorFor(x => x.ExternalUserId, "");
        }

        [Fact]
        public void ExternalUserId_Should_Have_Error_When_Greater_Than_128_Characters()
        {
            this.breadcrumbValidator.ShouldHaveValidationErrorFor(x => x.ExternalUserId, new string('a', 129));
        }

        [Fact]
        public void ExternalUserId_Should_NOT_Have_Error_When_Less_Than_Equal_To_128_Characters()
        {
            this.breadcrumbValidator.ShouldNotHaveValidationErrorFor(x => x.ExternalUserId, new string('a', 127));
            this.breadcrumbValidator.ShouldNotHaveValidationErrorFor(x => x.ExternalUserId, new string('a', 128));
        }

        [Fact]
        public void Position_Should_Have_Error_When_Latitude_Out_Of_Range()
        {
            this.breadcrumbValidator.ShouldHaveChildValidator(b => b.Position, typeof(AbstractValidator<LngLat>));
        }

        [Fact]
        public void Accuracy_Should_Have_Error_When_Less_Than_0()
        {
            this.breadcrumbValidator.ShouldHaveValidationErrorFor(b => b.Accuracy, -1);
        }

        [Fact]
        public void Accuracy_Should_NOT_Have_Error_When_GreaterThanEqualTo_0()
        {
            this.breadcrumbValidator.ShouldNotHaveValidationErrorFor(b => b.Accuracy, 0);
            this.breadcrumbValidator.ShouldNotHaveValidationErrorFor(b => b.Accuracy, 1);
        }

        [Fact]
        public void RecordedAt_Should_Have_Error_When_Default()
        {
            this.breadcrumbValidator.ShouldHaveValidationErrorFor(b => b.RecordedAt, default(DateTime));
        }

        [Fact]
        public void RecordedAt_Should_Have_Error_When_MinValue()
        {
            this.breadcrumbValidator.ShouldHaveValidationErrorFor(b => b.RecordedAt, DateTime.MinValue);
        }

        [Fact]
        public void RecordedAt_Should_Have_Error_When_MaxValue()
        {
            this.breadcrumbValidator.ShouldHaveValidationErrorFor(b => b.RecordedAt, DateTime.MaxValue);
        }

        [Fact]
        public void RecordedAt_Should_NOT_Have_Error_When_Valid()
        {
            this.breadcrumbValidator.ShouldNotHaveValidationErrorFor(b => b.RecordedAt, DateTime.UtcNow);
        }

        [Fact]
        public void MetaData_Should_Have_Error_When_More_Than_16_Elements()
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
            this.breadcrumbValidator.ShouldHaveValidationErrorFor(x => x.Metadata, metadata);
        }

        [Fact]
        public void MetaData_Should_NOT_Have_Error_When_Less_Than_Equal_To_16_Elements()
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
            this.breadcrumbValidator.ShouldNotHaveValidationErrorFor(x => x.Metadata, metadata);
        }

        [Fact]
        public void MetaData_Should_Have_Error_When_Key_Empty()
        {
            var model = new BreadcrumbModel()
            {
                Metadata = new List<KeyValuePair<string, string>>()
                    {
                        new KeyValuePair<string, string>("","a"),
                    }
            };
            var result = this.breadcrumbValidator.TestValidate(model);
            result.ShouldHaveValidationErrorFor("Metadata[0].Key");
        }

        [Fact]
        public void MetaData_Should_Have_Error_When_Value_Empty()
        {
            var model = new BreadcrumbModel()
            {
                Metadata = new List<KeyValuePair<string, string>>()
                    {
                        new KeyValuePair<string, string>("a",""),
                    }
            };
            var result = this.breadcrumbValidator.TestValidate(model);
            result.ShouldHaveValidationErrorFor("Metadata[0].Value");
        }

        [Fact]
        public void MetaData_Should_NOT_Have_Error_When_Values_Populated()
        {
            var model = new BreadcrumbModel()
            {
                Metadata = new List<KeyValuePair<string, string>>()
                    {
                        new KeyValuePair<string, string>("a","s"),
                    }
            };
            var result = this.breadcrumbValidator.TestValidate(model);
            result.ShouldNotHaveValidationErrorFor("Metadata[0].Key");
            result.ShouldNotHaveValidationErrorFor("Metadata[0].Value");
        }
    }
}