using MediatR;
using MSECS.Asset.Application.DTOs;

namespace MSECS.Asset.Application.Assets.Commands.RecordMaintenance;

public record RecordMaintenanceCommand(
    Guid AssetId, string Type, string Description, string PerformedBy, DateTimeOffset PerformedAtUtc) : IRequest<MaintenanceRecordDto>;
