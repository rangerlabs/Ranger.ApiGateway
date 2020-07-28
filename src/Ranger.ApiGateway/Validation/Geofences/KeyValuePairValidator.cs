using System.Collections.Generic;
using FluentValidation;

namespace Ranger.ApiGateway
{
    public class KeyValuePairValidator : AbstractValidator<KeyValuePair<string, string>>
    {
        public KeyValuePairValidator()
        {
            RuleFor(k => k.Key).NotEmpty().MaximumLength(128);
            RuleFor(k => k.Value).NotEmpty().MaximumLength(128);
        }
    }
}