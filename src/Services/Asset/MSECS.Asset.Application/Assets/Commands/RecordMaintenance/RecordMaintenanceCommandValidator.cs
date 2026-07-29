using FluentValidation;
using MSECS.Asset.Domain.Enums;

namespace MSECS.Asset.Application.Assets.Commands.RecordMaintenance;

public class RecordMaintenanceCommandValidator : AbstractValidator<RecordMaintenanceCommand>
{
    public RecordMaintenanceCommandValidator()
    {
        RuleFor(x => x.AssetId).NotEmpty();
        RuleFor(x => x.Type).NotEmpty().Must(t => Enum.TryParse<MaintenanceType>(t, true, out _))
            .WithMessage("Type must be one of: Scheduled, Corrective, Inspection, FirmwareUpdate.");
        RuleFor(x => x.Description).NotEmpty().MaximumLength(2000);
        RuleFor(x => x.PerformedBy).NotEmpty().MaximumLength(200);
    }
}
