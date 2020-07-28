using FluentValidation;
using NodaTime;
using Ranger.Common;

namespace Ranger.ApiGateway
{
    public class ScheduleValidator : AbstractValidator<Schedule>
    {
        public ScheduleValidator(IValidator<DailySchedule> dailyScheduleValidator)
        {
            RuleFor(s => s.TimeZoneId).NotEmpty().Custom((t, c) =>
            {
                if (DateTimeZoneProviders.Tzdb.GetZoneOrNull(t) is null)
                {
                    c.AddFailure("TimezoneId was invalid");
                }
            });
            RuleFor(s => s.Sunday).SetValidator(dailyScheduleValidator);
            RuleFor(s => s.Monday).SetValidator(dailyScheduleValidator);
            RuleFor(s => s.Tuesday).SetValidator(dailyScheduleValidator);
            RuleFor(s => s.Wednesday).SetValidator(dailyScheduleValidator);
            RuleFor(s => s.Thursday).SetValidator(dailyScheduleValidator);
            RuleFor(s => s.Friday).SetValidator(dailyScheduleValidator);
            RuleFor(s => s.Saturday).SetValidator(dailyScheduleValidator);
        }
    }
}