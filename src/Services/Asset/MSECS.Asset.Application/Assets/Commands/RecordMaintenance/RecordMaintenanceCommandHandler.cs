using MediatR;
using Microsoft.EntityFrameworkCore;
using MSECS.Asset.Application.Common.Interfaces;
using MSECS.Asset.Application.DTOs;
using MSECS.Asset.Domain.Enums;
using MSECS.SharedKernel.Exceptions;

namespace MSECS.Asset.Application.Assets.Commands.RecordMaintenance;

public class RecordMaintenanceCommandHandler : IRequestHandler<RecordMaintenanceCommand, MaintenanceRecordDto>
{
    private readonly IAssetDbContext _db;

    public RecordMaintenanceCommandHandler(IAssetDbContext db) => _db = db;

    public async Task<MaintenanceRecordDto> Handle(RecordMaintenanceCommand request, CancellationToken cancellationToken)
    {
        var asset = await _db.Assets.Include(a => a.MaintenanceHistory)
            .FirstOrDefaultAsync(a => a.Id == request.AssetId, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.Asset), request.AssetId);

        var record = asset.RecordMaintenance(
            Enum.Parse<MaintenanceType>(request.Type, true), request.Description, request.PerformedBy, request.PerformedAtUtc);

        await _db.SaveChangesAsync(cancellationToken);

        return new MaintenanceRecordDto(record.Id, record.AssetId, request.Type, record.Description, record.PerformedBy, record.PerformedAtUtc);
    }
}
