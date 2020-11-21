using System;
using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using FluentValidation.TestHelper;
using NodaTime;
using Ranger.Common;
using Shouldly;
using Xunit;

namespace Ranger.ApiGateway.Tests
{

    [Collection("Validation collection")]
    public class GeofenceBulkDeleteModelValidationTests
    {
        private readonly IValidator<GeofenceBulkDeleteModel> paramsValidator;
        public GeofenceBulkDeleteModelValidationTests(ValidationFixture fixture)
        {
            this.paramsValidator = fixture.serviceProvider.GetRequiredServiceForTest<IValidator<GeofenceBulkDeleteModel>>();
        }
        [Fact]
        public void ExternalIds_ShouldHaveError_WhenEmpty()
        {
            var geofenceBulkDeleteModel = new GeofenceBulkDeleteModel();
            var result = paramsValidator.TestValidate(geofenceBulkDeleteModel);
            result.ShouldHaveValidationErrorFor(r => r.ExternalIds);
        }

        [Fact]
        public void ExternalIds_ShouldNOTHaveError_WhenNOTEmptyAndValid()
        {
            var geofenceBulkDeleteModel = new GeofenceBulkDeleteModel()
            {
                ExternalIds = new List<string> { "geofence" }
            };
            var result = paramsValidator.TestValidate(geofenceBulkDeleteModel);
            result.ShouldNotHaveValidationErrorFor(r => r.ExternalIds);
        }

        [Fact]
        public void ExternalIds_ShouldNOTHaveError_WhenLessThan500Entries()
        {
            var ids = new List<string>();
            for (int i = 0; i < 499; i++)
            {
                ids.Add("geofence");
            }
            var geofenceBulkDeleteModel = new GeofenceBulkDeleteModel() { ExternalIds = ids };
            var result = paramsValidator.TestValidate(geofenceBulkDeleteModel);
            result.ShouldNotHaveValidationErrorFor(r => r.ExternalIds);
        }

        [Fact]
        public void ExternalIds_ShouldNOTHaveError_When500Entries()
        {
            var ids = new List<string>();
            for (int i = 0; i < 500; i++)
            {
                ids.Add("geofence");
            }
            var geofenceBulkDeleteModel = new GeofenceBulkDeleteModel() { ExternalIds = ids };
            var result = paramsValidator.TestValidate(geofenceBulkDeleteModel);
            result.ShouldNotHaveValidationErrorFor(r => r.ExternalIds);
        }

        [Fact]
        public void ExternalIds_ShouldHaveError_WhenMoreThan500Entries()
        {
            var ids = new List<string>();
            for (int i = 0; i <= 500; i++)
            {
                ids.Add("geofence");
            }
            var geofenceBulkDeleteModel = new GeofenceBulkDeleteModel() { ExternalIds = ids };
            var result = paramsValidator.TestValidate(geofenceBulkDeleteModel);
            result.ShouldHaveValidationErrorFor(r => r.ExternalIds);
            result.Errors.Where(e => e.PropertyName == nameof(geofenceBulkDeleteModel.ExternalIds)).FirstOrDefault().ErrorMessage.StartsWith("Max 500 External Ids permitted").ShouldBeTrue();
        }

        [Fact]
        public void ExternalIds_ShouldNOTHaveError_WhenAllAre128CharactersOrLess()
        {
            var geofenceBulkDeleteModel = new GeofenceBulkDeleteModel()
            {
                ExternalIds = new List<string>() { new string('a', 128), new string('a', 128) }
            };
            var result = paramsValidator.TestValidate(geofenceBulkDeleteModel);
            result.ShouldNotHaveValidationErrorFor(r => r.ExternalIds);
        }

        [Fact]
        public void ExternalIds_ShouldNOTHaveError_WhenAllAre3CharactersOrMore()
        {
            var geofenceBulkDeleteModel = new GeofenceBulkDeleteModel()
            {
                ExternalIds = new List<string>() { new string('a', 3), new string('a', 4) }
            };
            var result = paramsValidator.TestValidate(geofenceBulkDeleteModel);
            result.ShouldNotHaveValidationErrorFor(r => r.ExternalIds);
        }



        [Fact]
        public void ExternalIds_ShouldHaveError_WhenAnyAreMoreThan128Characters()
        {
            var geofenceBulkDeleteModel = new GeofenceBulkDeleteModel()
            {
                ExternalIds = new List<string>() { new string('a', 128), new string('a', 129) }
            };
            var result = paramsValidator.TestValidate(geofenceBulkDeleteModel);
            result.ShouldHaveValidationErrorFor(r => r.ExternalIds);
            result.Errors.Where(e => e.PropertyName == nameof(geofenceBulkDeleteModel.ExternalIds)).FirstOrDefault().ErrorMessage.StartsWith("The collection contained External Ids which do not conform").ShouldBeTrue();
        }

        [Fact]
        public void ExternalIds_ShouldHaveError_WhenAnyAreLessThan3Characters()
        {
            var geofenceBulkDeleteModel = new GeofenceBulkDeleteModel()
            {
                ExternalIds = new List<string>() { new string('a', 3), new string('a', 2) }
            };
            var result = paramsValidator.TestValidate(geofenceBulkDeleteModel);
            result.ShouldHaveValidationErrorFor(r => r.ExternalIds);
            result.Errors.Where(e => e.PropertyName == nameof(geofenceBulkDeleteModel.ExternalIds)).FirstOrDefault().ErrorMessage.StartsWith("The collection contained External Ids which do not conform").ShouldBeTrue();
        }

        [Theory]
        [InlineData("-")]
        [InlineData("-a0-")]
        [InlineData("-A0-")]
        [InlineData("-a!0-")]
        [InlineData(" a!0-")]
        [InlineData("a!0 ")]
        public void ExternalId_ShouldHaveError_WhenAnyFailsRegex(string externalId)
        {
            var geofenceBulkDeleteModel = new GeofenceBulkDeleteModel()
            {
                ExternalIds = new List<string> {
                    "geofence",
                    externalId
                }
            };
            var result = paramsValidator.TestValidate(geofenceBulkDeleteModel);
            result.ShouldHaveValidationErrorFor(r => r.ExternalIds);
            result.Errors.Where(e => e.PropertyName == nameof(geofenceBulkDeleteModel.ExternalIds)).FirstOrDefault().ErrorMessage.StartsWith("The collection contained External Ids which do not conform").ShouldBeTrue();
        }

        [Theory]
        [InlineData("a-0")]
        [InlineData("0-a")]
        [InlineData("aa0")]
        [InlineData("a00")]
        [InlineData("0aa")]
        [InlineData("00a")]
        public void ExternalId_ShouldNOTHaveError_WhenAllPassRegex(string externalId)
        {
            var geofenceBulkDeleteModel = new GeofenceBulkDeleteModel()
            {
                ExternalIds = new List<string> {
                    "geofence",
                    externalId
                }
            };
            var result = paramsValidator.TestValidate(geofenceBulkDeleteModel);
            result.ShouldNotHaveValidationErrorFor(r => r.ExternalIds);
        }
    }
}
