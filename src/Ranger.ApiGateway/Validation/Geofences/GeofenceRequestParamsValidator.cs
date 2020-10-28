using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using Ranger.Common;
using Ranger.Services.Geofences;

namespace Ranger.ApiGateway
{
    public class GeofenceRequestParamsValidator : AbstractValidator<GeofenceRequestParams>
    {
        public GeofenceRequestParamsValidator()
        {
            RuleSet("Get", () => {
                RuleFor(x => x.GeofenceSortOrder)
                    .NotEmpty()
                    .Must((x) => 
                        GetOrderByOptions()
                        .Contains(x, StringComparer.InvariantCultureIgnoreCase))
                    .WithMessage($"OrderBy must be one of {String.Join(',', GetOrderByOptions())}");
                RuleFor(x => x.GeofenceSortOrder)
                    .NotEmpty()
                     .Must((x) => 
                        GetOrderByOptions()
                        .Contains(x, StringComparer.InvariantCultureIgnoreCase))
                    .WithMessage($"SortOrder must be one of {String.Join(',', GetGeofenceSortOrder())}");
                RuleFor(x => x.Page)
                    .NotEmpty()
                    .GreaterThan(0);
                RuleFor(x => x.PageCount)
                    .NotEmpty()
                    .GreaterThan(0);
            });
        }

        private IEnumerable<string> GetOrderByOptions() {
            var properties = typeof(OrderByOptions).GetProperties();
            foreach (var property in properties) {
                yield return property.GetValue(null) as string;
            }
        }

        private IEnumerable<string> GetGeofenceSortOrder() {
            var properties = typeof(GeofenceSortOrders).GetProperties();
            foreach (var property in properties) {
                yield return property.GetValue(null) as string;
            }
        }
    }


}