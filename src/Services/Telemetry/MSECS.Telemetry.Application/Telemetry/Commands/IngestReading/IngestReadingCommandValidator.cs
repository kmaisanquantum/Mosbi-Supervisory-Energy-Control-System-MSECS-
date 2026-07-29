using FluentValidation;
using MSECS.Telemetry.Domain.Enums;

namespace MSECS.Telemetry.Application.Telemetry.Commands.IngestReading;

public class IngestReadingCommandValidator : AbstractValidator<IngestReadingCommand>
{
    public IngestReadingCommandValidator()
    {
        RuleFor(x => x.OrganizationId).NotEmpty();
        RuleFor(x => x.SiteId).NotEmpty();
        RuleFor(x => x.AssetId).NotEmpty();
        RuleFor(x => x.DeviceId).NotEmpty();
        RuleFor(x => x.SourceProtocol).NotEmpty();
        RuleFor(x => x.Readings).NotEmpty().Must(r => r.Count <= 200)
            .WithMessage("A single ingestion call cannot contain more than 200 readings.");

        RuleForEach(x => x.Readings).ChildRules(reading =>
        {
            reading.RuleFor(r => r.MetricType).NotEmpty()
                .Must(m => Enum.TryParse<TelemetryMetricType>(m, true, out _))
                .WithMessage("MetricType must be a recognized TelemetryMetricType (VoltageV, CurrentA, PowerKw, ...).");
        });
    }
}
