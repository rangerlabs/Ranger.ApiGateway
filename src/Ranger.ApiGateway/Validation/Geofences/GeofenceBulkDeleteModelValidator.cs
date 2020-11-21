using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using FluentValidation;
using Ranger.Common;

namespace Ranger.ApiGateway
{
    public class GeofenceBulkDeleteModelValidator : AbstractValidator<GeofenceBulkDeleteModel>
    {
        public GeofenceBulkDeleteModelValidator(IValidator<LngLat> lngLatValidator, IValidator<Schedule> scheduleValidator, IValidator<KeyValuePair<string, string>> keyValuePairValidator)
        {
            RuleFor(g => g.ExternalIds)
                .NotEmpty()
                .Custom((e, c) =>
                {
                    if (!(e is null))
                    {
                        var count = e.Count();
                        if (count > 500)
                        {
                            c.AddFailure(c.PropertyName, $"Max 500 External Ids permitted for bulk delete operation. Contained '{count}'.");
                        }
                    }
                })
                .Custom((e, c) =>
                {
                    if (!(e is null))
                    {
                        var invalidExternalIds = e.Where(id => !Regex.IsMatch(id, RegularExpressions.GEOFENCE_INTEGRATION_NAME) || id.Length < 3 || id.Length > 128);
                        if (invalidExternalIds.Any())
                        {
                            c.AddFailure(c.PropertyName, $"The collection contained External Ids which do not conform to the naming standards. External Ids must begin, end, and contain lowercase alphanumeric characters. May contain ( - ). Between 3 and 128 characters. The following were invalid: {String.Join(',', invalidExternalIds)}");
                        }
                    }
                });
        }
    }
}