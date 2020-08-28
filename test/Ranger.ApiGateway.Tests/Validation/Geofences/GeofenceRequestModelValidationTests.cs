using System;
using System.Collections.Generic;
using FluentValidation;
using FluentValidation.TestHelper;
using NodaTime;
using Ranger.Common;
using Shouldly;
using Xunit;

namespace Ranger.ApiGateway.Tests
{

    [Collection("Validation collection")]
    public class GeofenceRequestModelValidationTests
    {
        private readonly IValidator<GeofenceRequestModel> geofenceValidator;
        public GeofenceRequestModelValidationTests(ValidationFixture fixture)
        {
            this.geofenceValidator = fixture.serviceProvider.GetRequiredServiceForTest<IValidator<GeofenceRequestModel>>();
        }

        [Fact]
        public void ExternalId_Should_Have_Error_When_Less_Than_2_Characters()
        {
            this.geofenceValidator.ShouldHaveValidationErrorFor(g => g.ExternalId, "a");
        }

        [Fact]
        public void ExternalId_Should_Have_Error_When_Empty()
        {
            this.geofenceValidator.ShouldHaveValidationErrorFor(g => g.ExternalId, "");
        }

        [Fact]
        public void ExternalId_Should_Have_Error_When_Greater_Than_128_Characters()
        {

            this.geofenceValidator.ShouldHaveValidationErrorFor(g => g.ExternalId, new string('a', 129));
        }

        [Fact]
        public void ExternalId_Should_Have_No_Error_When_128_Characters_Or_Less()
        {
            this.geofenceValidator.ShouldNotHaveValidationErrorFor(g => g.ExternalId, new string('a', 128));
        }

        [Theory]
        [InlineData("-")]
        [InlineData("-a0-")]
        [InlineData("-A0-")]
        [InlineData("-a!0-")]
        [InlineData(" a!0-")]
        [InlineData("a!0 ")]
        public void ExternalId_Should_Have_Error_When_Fails_Regex(string externalId)
        {
            this.geofenceValidator.ShouldHaveValidationErrorFor(g => g.ExternalId, externalId);
        }

        [Theory]
        [InlineData("a-0")]
        [InlineData("0-a")]
        [InlineData("aa0")]
        [InlineData("a00")]
        [InlineData("0aa")]
        [InlineData("00a")]
        public void ExternalId_Should_NOT_Have_Error_When_Passes_Regex(string externalId)
        {
            this.geofenceValidator.ShouldNotHaveValidationErrorFor(g => g.ExternalId, externalId);
        }

        [Fact]
        public void Coordinates_Should_Have_Error_When_Coordinates_Has_No_Elements()
        {
            this.geofenceValidator.ShouldHaveValidationErrorFor(g => g.Coordinates, new List<LngLat>());
        }

        [Fact]
        public void Coordinates_Should_Have_Error_When_Latitude_Invalid()
        {
            var model = new GeofenceRequestModel()
            {
                Coordinates = new List<LngLat>() { new LngLat(0, 91) },
                Shape = GeofenceShapeEnum.Circle
            };
            var result = this.geofenceValidator.TestValidate(model);
            result.ShouldHaveValidationErrorFor("Coordinates[0].Lat");
        }

        [Fact]
        public void Coordinates_Should_Have_Error_When_Longitude_Invalid()
        {
            var model = new GeofenceRequestModel()
            {
                Coordinates = new List<LngLat>() { new LngLat(181, 0) },
                Shape = GeofenceShapeEnum.Circle
            };
            var result = this.geofenceValidator.TestValidate(model);
            result.ShouldHaveValidationErrorFor("Coordinates[0].Lng");
        }

        [Fact]
        public void Coordinates_Should_NOT_Have_Error_When_Longitude_Invalid()
        {
            var model = new GeofenceRequestModel()
            {
                Coordinates = new List<LngLat>() { new LngLat(0, 0) },
                Shape = GeofenceShapeEnum.Circle
            };
            var result = this.geofenceValidator.TestValidate(model);
            result.ShouldNotHaveValidationErrorFor("Coordinates[0].Lng");
            result.ShouldNotHaveValidationErrorFor("Coordinates[0].Lat");
        }

        [Fact]
        public void Coordinates_Should_Have_Error_When_Shape_Is_Circle_And_Coordinates_Has_Greater_Than_1_Element()
        {
            var model = new GeofenceRequestModel()
            {
                Coordinates = new List<LngLat>() { new LngLat(0, 0), new LngLat(0, 0) },
                Shape = GeofenceShapeEnum.Circle
            };
            var result = this.geofenceValidator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(g => g.Coordinates);
        }

        [Fact]
        public void Coordinates_Should_Have_Error_When_Shape_Is_Polygon_And_Coordinates_Has_Less_Than_3_Elements()
        {
            var model = new GeofenceRequestModel()
            {
                Coordinates = new List<LngLat>() { new LngLat(0, 0), new LngLat(0, 0) },
                Shape = GeofenceShapeEnum.Polygon
            };
            var result = this.geofenceValidator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.Coordinates);
        }

        [Fact]
        public void Coordinates_Should_Have_Error_When_Shape_Is_Polygon_And_Coordinates_Has_Greater_Than_512_Elements()
        {
            var coordinates = new List<LngLat>();
            for (int i = 0; i < 513; i++)
            {
                coordinates.Add(new LngLat(0, 0));
            }

            var model = new GeofenceRequestModel()
            {
                Coordinates = coordinates,
                Shape = GeofenceShapeEnum.Polygon
            };
            var result = this.geofenceValidator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.Coordinates);
        }

        [Fact]
        public void Coordinates_Should_Have_Error_When_Shape_Is_Polygon_AND_Coordinates_Has_Greater_Than_OR_Equal_3_Elements_AND_First_Last_Coordinates_Are_Equal()
        {
            var model = new GeofenceRequestModel()
            {
                Coordinates = new List<LngLat>() { new LngLat(0, 0), new LngLat(0, 0), new LngLat(0, 0) },
                Shape = GeofenceShapeEnum.Polygon
            };
            var result = this.geofenceValidator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.Coordinates);
        }

        [Fact]
        public void IntegrationIds_Should_Have_Error_When_Contains_Duplicate_Id()
        {
            var guid = Guid.NewGuid();
            var model = new GeofenceRequestModel()
            {
                IntegrationIds = new List<Guid>()
                    {
                        guid,
                        guid
                    }
            };
            var result = this.geofenceValidator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.IntegrationIds);
        }

        [Fact]
        public void IntegrationIds_Should_NOT_Have_Error_When_Contains_Unique_Ids()
        {
            var model = new GeofenceRequestModel()
            {
                IntegrationIds = new List<Guid>()
                    {
                        Guid.NewGuid(),
                        Guid.NewGuid()
                    }
            };
            var result = this.geofenceValidator.TestValidate(model);
            result.ShouldNotHaveValidationErrorFor(x => x.IntegrationIds);
        }

        [Fact]
        public void Schedule_Should_Have_Error_When_TimeZoneId_Not_In_TimeZoneDatabase()
        {
            var model = new GeofenceRequestModel()
            {
                Schedule = new Schedule(
                new DailySchedule(new LocalTime(0, 0, 0, 000), new LocalTime(23, 59, 59, 999)),
                new DailySchedule(new LocalTime(0, 0, 0, 000), new LocalTime(23, 59, 59, 999)),
                new DailySchedule(new LocalTime(0, 0, 0, 000), new LocalTime(23, 59, 59, 999)),
                new DailySchedule(new LocalTime(0, 0, 0, 000), new LocalTime(23, 59, 59, 999)),
                new DailySchedule(new LocalTime(0, 0, 0, 000), new LocalTime(23, 59, 59, 999)),
                new DailySchedule(new LocalTime(0, 0, 0, 000), new LocalTime(23, 59, 59, 999)),
                new DailySchedule(new LocalTime(0, 0, 0, 000), new LocalTime(23, 59, 59, 999)),
                "Hello")
            };
            var result = this.geofenceValidator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.Schedule.TimeZoneId);
        }

        [Fact]
        public void Schedule_Should_NOT_Have_Error_When_TimeZoneId_In_TimeZoneDatabase()
        {
            var model = new GeofenceRequestModel()
            {
                Schedule = new Schedule(
                new DailySchedule(new LocalTime(0, 0, 0, 000), new LocalTime(23, 59, 59, 999)),
                new DailySchedule(new LocalTime(0, 0, 0, 000), new LocalTime(23, 59, 59, 999)),
                new DailySchedule(new LocalTime(0, 0, 0, 000), new LocalTime(23, 59, 59, 999)),
                new DailySchedule(new LocalTime(0, 0, 0, 000), new LocalTime(23, 59, 59, 999)),
                new DailySchedule(new LocalTime(0, 0, 0, 000), new LocalTime(23, 59, 59, 999)),
                new DailySchedule(new LocalTime(0, 0, 0, 000), new LocalTime(23, 59, 59, 999)),
                new DailySchedule(new LocalTime(0, 0, 0, 000), new LocalTime(23, 59, 59, 999)),
                "US/Eastern")
            };
            var result = this.geofenceValidator.TestValidate(model);
            result.ShouldNotHaveValidationErrorFor(x => x.Schedule.TimeZoneId);
        }

        [Fact]
        public void Schedule_Should_Have_Error_When_DailySchedule_StartTime_After_EndTime()
        {
            var model = new GeofenceRequestModel()
            {
                Schedule = new Schedule(
                new DailySchedule(new LocalTime(23, 59, 59, 999), new LocalTime(0, 0, 0, 000)),
                new DailySchedule(new LocalTime(0, 0, 0, 000), new LocalTime(23, 59, 59, 999)),
                new DailySchedule(new LocalTime(0, 0, 0, 000), new LocalTime(23, 59, 59, 999)),
                new DailySchedule(new LocalTime(0, 0, 0, 000), new LocalTime(23, 59, 59, 999)),
                new DailySchedule(new LocalTime(0, 0, 0, 000), new LocalTime(23, 59, 59, 999)),
                new DailySchedule(new LocalTime(0, 0, 0, 000), new LocalTime(23, 59, 59, 999)),
                new DailySchedule(new LocalTime(0, 0, 0, 000), new LocalTime(23, 59, 59, 999)),
                "US/Eastern")
            };
            var result = this.geofenceValidator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.Schedule.Sunday);
        }

        [Fact]
        public void Schedule_Should_NOT_Have_Error_When_DailySchedule_StartTime_After_EndTime()
        {
            var model = new GeofenceRequestModel()
            {
                Schedule = new Schedule(
                new DailySchedule(new LocalTime(23, 59, 59, 999), new LocalTime(0, 0, 0, 000)),
                new DailySchedule(new LocalTime(0, 0, 0, 000), new LocalTime(23, 59, 59, 999)),
                new DailySchedule(new LocalTime(0, 0, 0, 000), new LocalTime(23, 59, 59, 999)),
                new DailySchedule(new LocalTime(0, 0, 0, 000), new LocalTime(23, 59, 59, 999)),
                new DailySchedule(new LocalTime(0, 0, 0, 000), new LocalTime(23, 59, 59, 999)),
                new DailySchedule(new LocalTime(0, 0, 0, 000), new LocalTime(23, 59, 59, 999)),
                new DailySchedule(new LocalTime(0, 0, 0, 000), new LocalTime(23, 59, 59, 999)),
                "US/Eastern")
            };
            var result = this.geofenceValidator.TestValidate(model);
            result.ShouldNotHaveValidationErrorFor(x => x.Schedule.Monday);
        }

        [Fact]
        public void Radius_Should_Have_Error_When_Shape_Is_Circle_And_Radius_Negative()
        {
            var model = new GeofenceRequestModel()
            {
                Shape = GeofenceShapeEnum.Circle,
                Radius = -1
            };
            var result = this.geofenceValidator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.Radius);
        }

        [Fact]
        public void Radius_Should_Have_Error_When_Shape_Is_Circle_And_Less_Than_50()
        {
            var model = new GeofenceRequestModel()
            {
                Shape = GeofenceShapeEnum.Circle,
                Radius = 49
            };
            var result = this.geofenceValidator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.Radius);
        }


        [Fact]
        public void Radius_Should_NOT_Have_Error_When_Shape_Is_Circle_And_Equal_To_0()
        {
            var model = new GeofenceRequestModel()
            {
                Shape = GeofenceShapeEnum.Circle,
                Radius = 0
            };
            var result = this.geofenceValidator.TestValidate(model);
            result.ShouldNotHaveValidationErrorFor(x => x.Radius);
        }

        [Fact]
        public void Radius_Should_NOT_Have_Error_When_Shape_Is_Circle_And_Equal_To_50()
        {
            var model50 = new GeofenceRequestModel()
            {
                Shape = GeofenceShapeEnum.Circle,
                Radius = 50
            };
            var model51 = new GeofenceRequestModel()
            {
                Shape = GeofenceShapeEnum.Circle,
                Radius = 51
            };
            var result50 = this.geofenceValidator.TestValidate(model50);
            var result51 = this.geofenceValidator.TestValidate(model51);
            result50.ShouldNotHaveValidationErrorFor(x => x.Radius);
            result51.ShouldNotHaveValidationErrorFor(x => x.Radius);
        }

        [Fact]
        public void Description_Should_Have_Error_When_Length_Greater_Than_512_Characters()
        {
            this.geofenceValidator.ShouldHaveValidationErrorFor(x => x.Description, new string('a', 513));
        }

        [Fact]
        public void Description_Should_NOT_Have_Error_When_Length_Less_Or_Equal_To_512_Characters()
        {
            this.geofenceValidator.ShouldNotHaveValidationErrorFor(x => x.Description, new string('a', 512));
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
            this.geofenceValidator.ShouldHaveValidationErrorFor(x => x.Metadata, metadata);
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
            this.geofenceValidator.ShouldNotHaveValidationErrorFor(x => x.Metadata, metadata);
        }

        [Fact]
        public void Metadata_AND_Headers_Should_Have_Child_Validator()
        {
            this.geofenceValidator.ShouldHaveChildValidator(x => x.Metadata, typeof(IValidator<KeyValuePair<string, string>>));
        }

        [Fact]
        public void OnEnter_Defaults_To_True()
        {
            var model = new GeofenceRequestModel();
            model.OnEnter.ShouldBeTrue();
        }

        [Fact]
        public void OnExit_Defaults_To_True()
        {
            var model = new GeofenceRequestModel();
            model.OnExit.ShouldBeTrue();
        }

        [Fact]
        public void OnDwell_Defaults_To_False()
        {
            var model = new GeofenceRequestModel();
            model.OnDwell.ShouldBeFalse();
        }

        [Fact]
        public void Enabled_Defaults_To_True()
        {
            var model = new GeofenceRequestModel();
            model.Enabled.ShouldBeTrue();
        }

        [Fact]
        public void Radius_Should_Have_Error_When_Circle_Geofence_AND_Less_Than_50()
        {
            var model = new GeofenceRequestModel()
            {
                Radius = 49,
                Shape = GeofenceShapeEnum.Circle
            };
            var result = this.geofenceValidator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.Radius);
        }


        [Fact]
        public void Radius_Should_NOT_Have_Error_When_Circle_Geofence_AND_Greater_Than_Or_Equal_To_50()
        {
            var model = new GeofenceRequestModel()
            {
                Radius = 50,
                Shape = GeofenceShapeEnum.Circle
            };
            var result = this.geofenceValidator.TestValidate(model);
            result.ShouldNotHaveValidationErrorFor(x => x.Radius);
        }


        [Fact]
        public void Radius_Should_NOT_Have_Error_When_Polygon_Geofence()
        {
            var model = new GeofenceRequestModel()
            {
                Shape = GeofenceShapeEnum.Polygon
            };
            var result = this.geofenceValidator.TestValidate(model);
            result.ShouldNotHaveValidationErrorFor(x => x.Radius);
        }
    }
}