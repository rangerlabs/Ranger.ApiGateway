using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using Ranger.Common;

namespace Ranger.ApiGateway
{
    public class GeofenceRequestModelValidator : AbstractValidator<GeofenceRequestModel>
    {
        public GeofenceRequestModelValidator(IValidator<LngLat> lngLatValidator, IValidator<Schedule> scheduleValidator, IValidator<KeyValuePair<string, string>> keyValuePairValidator)
        {
            RuleFor(g => g.ExternalId)
                .NotEmpty()
                .MaximumLength(128)
                .Matches("^[a-z0-9]+[a-z0-9-]*[a-z0-9]+$")
                .WithMessage("Valid External Id characters are lowercase alphanumeric and dashes ('-'). Must begin and end with alphanumeric characters.");
            RuleFor(g => g.Coordinates)
                .NotEmpty()
                .Custom((coords, c) =>
                {
                    if (!(coords is null))
                    {
                        if ((c.InstanceToValidate as GeofenceRequestModel).Shape == GeofenceShapeEnum.Circle)
                        {
                            if (coords.Count() > 1)
                            {
                                c.AddFailure("Coordinates array must contain exactly 1 LngLat object for Circular geofences.");
                            }
                        }
                        else
                        {
                            if (coords.Count() < 3)
                            {
                                {
                                    c.AddFailure("Coordinates array must contain atleast 3 LngLat objects for Polygon geofences.");
                                }
                            }
                            else if (coords.First().Equals(coords.Last()))
                            {
                                c.AddFailure("The first and last coordinates in a polygon are implicitly connected. Remove the explicit closure.");
                            }
                        }
                    }
                });
            RuleFor(g => g.Coordinates)
                .ForEach(a => a.SetValidator(lngLatValidator));
            RuleFor(g => g.Description)
                .MaximumLength(512);
            RuleFor(g => g.Shape)
                .NotNull().
                IsInEnum();
            RuleFor(g => g.Radius)
                .Custom((r, c) =>
                {
                    if ((c.InstanceToValidate as GeofenceRequestModel).Shape == GeofenceShapeEnum.Circle)
                    {
                        if (r < 50)
                        {
                            c.AddFailure("Radius must be greater than or equal to 50 meters for Circular geofences");
                        }
                    }
                });
            RuleFor(g => g.Schedule).SetValidator(scheduleValidator);
            RuleFor(g => g.Metadata)
                .Custom((m, c) =>
                {
                    if (!(m is null) && m.Count() > 10)
                    {
                        c.AddFailure("Up to 10 metadata allowed");
                    }
                });
            RuleForEach(g => g.Metadata).SetValidator(keyValuePairValidator).WithMessage("Metadata {CollectionIndex} is invalid");
        }
    }
}