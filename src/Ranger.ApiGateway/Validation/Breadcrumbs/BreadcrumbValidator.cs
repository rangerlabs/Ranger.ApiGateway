using System;
using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using Ranger.Common;

namespace Ranger.ApiGateway
{
    public class BreadcrumbValidator : AbstractValidator<BreadcrumbModel>
    {
        public BreadcrumbValidator(IValidator<LngLat> lngLatValidator, IValidator<KeyValuePair<string, string>> keyValuePairValidator)
        {
            RuleFor(x => x.DeviceId).NotEmpty().MaximumLength(64);
            RuleFor(x => x.ExternalUserId).MaximumLength(64);
            RuleFor(x => x.Position).SetValidator(lngLatValidator);
            RuleFor(x => x.Accuracy).GreaterThanOrEqualTo(0);
            RuleFor(x => x.RecordedAt).NotEqual(default(DateTime)).NotEqual(DateTime.MinValue).NotEqual(DateTime.MaxValue);
            RuleFor(g => g.Metadata)
               .Custom((m, c) =>
               {
                   if (!(m is null) && m.Count() > 16)
                   {
                       c.AddFailure("Up to 16 metadata allowed");
                   }
               });
            RuleForEach(g => g.Metadata).SetValidator(keyValuePairValidator).WithMessage("Metadata {CollectionIndex} is invalid");
        }
    }
}