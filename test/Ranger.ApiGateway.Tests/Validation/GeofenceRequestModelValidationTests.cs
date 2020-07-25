using System;
using System.Collections.Generic;
using FluentValidation;
using FluentValidation.TestHelper;
using Microsoft.Extensions.DependencyInjection;
using NodaTime;
using Ranger.Common;
using Shouldly;
using Xunit;

namespace Ranger.ApiGateway.Tests
{
    public class GeofenceValidationFixture
    {
        public IServiceProvider serviceProvider;

        public GeofenceValidationFixture()
        {
            var services = new ServiceCollection();

            services.AddTransient<AbstractValidator<GeofenceRequestModel>, GeofenceRequestModelValidator>();
            services.AddTransient<AbstractValidator<Schedule>, ScheduleValidator>();
            services.AddTransient<AbstractValidator<DailySchedule>, DailyScheduleValidator>();
            services.AddTransient<AbstractValidator<KeyValuePair<string, string>>, KeyValuePairValidator>();
            services.AddTransient<AbstractValidator<LngLat>, LngLatValidator>();

            serviceProvider = services.BuildServiceProvider();
        }
    }
    public class GeofenceRequestModelValidationTests : IClassFixture<GeofenceValidationFixture>
    {
        private readonly AbstractValidator<GeofenceRequestModel> geofenceValidator;
        public GeofenceRequestModelValidationTests(GeofenceValidationFixture fixture)
        {
            this.geofenceValidator = fixture.serviceProvider.GetRequiredServiceForTest<AbstractValidator<GeofenceRequestModel>>();
        }

        [Fact]
        public void ExternalId_Should_Have_Error_When_Empty()
        {
            var model = new GeofenceRequestModel()
            {
                ExternalId = null
            };
            this.geofenceValidator.ShouldHaveValidationErrorFor(g => g.ExternalId, "");
        }

        [Fact]
        public void ExternalId_Should_Have_Error_When_Greater_Than_128_Characters()
        {

            this.geofenceValidator.ShouldHaveValidationErrorFor(g => g.ExternalId, "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa00");
        }

        [Theory]
        [InlineData("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa")]
        [InlineData("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa")]
        public void ExternalId_Should_Have_No_Error_When_128_Characters_Or_Less(string externalId)
        {
            this.geofenceValidator.ShouldNotHaveValidationErrorFor(g => g.ExternalId, externalId);
        }

        [Theory]
        [InlineData("-")]
        [InlineData("-a0-")]
        [InlineData("-A0-")]
        [InlineData("-a!0-")]
        public void ExternalId_Should_Have_Error_When_Fails_Regex(string externalId)
        {
            this.geofenceValidator.ShouldHaveValidationErrorFor(g => g.ExternalId, externalId);
        }

        [Theory]
        [InlineData("a-0")]
        [InlineData("0-a")]
        [InlineData("a0")]
        [InlineData("0a")]
        [InlineData("aa0")]
        [InlineData("a00")]
        [InlineData("0aa")]
        [InlineData("00a")]
        public void ExternalId_Should_Have_No_Error_When_Fails_Regex(string externalId)
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
        public void Description_Should_Have_Error_When_Length_Greater_Than_512_Characters()
        {
            var char513 = @"abcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrs";
            this.geofenceValidator.ShouldHaveValidationErrorFor(x => x.Description, char513);
        }

        [Fact]
        public void Description_Should_NOT_Have_Error_When_Length_Less_Or_Equal_To_512_Characters()
        {
            var char512 = @"abcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqr";
            this.geofenceValidator.ShouldNotHaveValidationErrorFor(x => x.Description, char512);
        }

        [Fact]
        public void MetaData_Should_Have_Error_When_More_Than_10_Elements()
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
                };
            this.geofenceValidator.ShouldHaveValidationErrorFor(x => x.Metadata, metadata);
        }

        [Fact]
        public void MetaData_Should_NOT_Have_Error_When_Less_Than_Equal_To_10_Elements()
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
               };
            this.geofenceValidator.ShouldNotHaveValidationErrorFor(x => x.Metadata, metadata);
        }

        [Fact]
        public void MetaData_Should_Have_Error_When_Key_Empty()
        {
            var model = new GeofenceRequestModel()
            {
                Metadata = new List<KeyValuePair<string, string>>()
                    {
                        new KeyValuePair<string, string>("","a"),
                    }
            };
            var result = this.geofenceValidator.TestValidate(model);
            result.ShouldHaveValidationErrorFor("Metadata[0].Key");
        }

        [Fact]
        public void MetaData_Should_Have_Error_When_Value_Empty()
        {
            var model = new GeofenceRequestModel()
            {
                Metadata = new List<KeyValuePair<string, string>>()
                    {
                        new KeyValuePair<string, string>("a",""),
                    }
            };
            var result = this.geofenceValidator.TestValidate(model);
            result.ShouldHaveValidationErrorFor("Metadata[0].Value");
        }

        [Fact]
        public void MetaData_Should_NOT_Have_Error_When_Values_Populated()
        {
            var model = new GeofenceRequestModel()
            {
                Metadata = new List<KeyValuePair<string, string>>()
                    {
                        new KeyValuePair<string, string>("a","s"),
                    }
            };
            var result = this.geofenceValidator.TestValidate(model);
            result.ShouldNotHaveValidationErrorFor("Metadata[0].Key");
            result.ShouldNotHaveValidationErrorFor("Metadata[0].Value");
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
    }
}