using FluentValidation;
using MSECS.Telemetry.Domain.Enums;

namespace MSECS.Telemetry.Application.Telemetry.Queries.GetReadingHistory;

public class GetReadingHistoryQueryValidator : AbstractValidator<GetReadingHistoryQuery>
{
    public GetReadingHistoryQueryValidator()
    {
        RuleFor(x => x.AssetId).NotEmpty();
        RuleFor(x => x.MetricType).NotEmpty().Must(m => Enum.TryParse<TelemetryMetricType>(m, true, out _));
        RuleFor(x => x.ToUtc).GreaterThan(x => x.FromUtc);
        RuleFor(x => x.MaxPoints).InclusiveBetween(1, 10000);
    }
}
