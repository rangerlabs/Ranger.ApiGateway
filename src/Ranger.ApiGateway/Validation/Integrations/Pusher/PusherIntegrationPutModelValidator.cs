using System;
using System.Collections.Generic;
using System.Linq;
using FluentValidation;

namespace Ranger.ApiGateway.Validation.Integrations.Pushers
{
    public class PusherIntegrationPutModelValidator : AbstractValidator<PusherIntegrationPutModel>
    {
        public PusherIntegrationPutModelValidator(IValidator<KeyValuePair<string, string>> keyValuePairValidator)
        {
            RuleFor(x => x.Name)
                .MinimumLength(3)
                .MaximumLength(128)
                .Matches(RegularExpressions.GEOFENCE_INTEGRATION_NAME)
                .WithMessage("Must begin, end, and contain lowercase alphanumeric characters. May contain ( - ).");
            RuleFor(x => x.Description).MaximumLength(512);
            RuleFor(x => x.AppId).NotEmpty().MaximumLength(32);
            RuleFor(x => x.Key).NotEmpty().MaximumLength(64);
            RuleFor(x => x.Secret).NotEmpty().MaximumLength(64);
            RuleFor(x => x.Cluster).NotEmpty().MaximumLength(8);
            RuleFor(x => x.Environment).IsInEnum();
            RuleFor(x => x.Metadata)
                .Custom((m, c) =>
                {
                    if (!(m is null) && m.Count() > 16)
                    {
                        c.AddFailure("Up to 16 metadata allowed");
                    }
                });
            RuleForEach(x => x.Metadata).SetValidator(keyValuePairValidator).WithMessage("Metadata {CollectionIndex} is invalid");
            RuleFor(x => x.Version).GreaterThanOrEqualTo(1);
        }
    }
}