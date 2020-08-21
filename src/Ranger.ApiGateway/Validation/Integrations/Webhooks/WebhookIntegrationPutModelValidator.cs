using System;
using System.Collections.Generic;
using System.Linq;
using FluentValidation;

namespace Ranger.ApiGateway.Validation.Integrations.Webhooks
{
    public class WebhookIntegrationPutModelValidator : AbstractValidator<WebhookIntegrationPutModel>
    {
        public WebhookIntegrationPutModelValidator(IValidator<KeyValuePair<string, string>> keyValuePairValidator)
        {
            RuleFor(x => x.IntegrationId).NotEmpty();
            RuleFor(x => x.Name)
                .MinimumLength(3)
                .MaximumLength(128)
                .Matches(RegularExpressions.GEOFENCE_INTEGRATION_NAME)
                .WithMessage("Must begin, end, and contain lowercase alphanumeric characters. May contain ( - ).");
            RuleFor(x => x.Description)
                .MaximumLength(512);
            RuleFor(x => x.Url).NotEmpty().Must(x =>
            {
                Uri uri;
                return Uri.TryCreate(x, UriKind.Absolute, out uri) && uri.Scheme == "https";
            }).WithMessage("Must be a valid HTTPS Url");
            RuleFor(x => x.Environment).IsInEnum();
            RuleFor(x => x.Headers)
                .Custom((h, c) =>
                {
                    if (!(h is null) && h.Count() > 16)
                    {
                        c.AddFailure("Up to 16 headers allowed");
                    }
                });
            RuleForEach(x => x.Headers).SetValidator(keyValuePairValidator).WithMessage("Metadata {CollectionIndex} is invalid");
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