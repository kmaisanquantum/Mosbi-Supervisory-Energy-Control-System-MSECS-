using MediatR;
using Microsoft.EntityFrameworkCore;
using MSECS.Asset.Application.Common.Interfaces;
using MSECS.Asset.Application.DTOs;
using MSECS.Asset.Domain.Enums;
using MSECS.SharedKernel.Exceptions;

namespace MSECS.Asset.Application.Assets.Commands.RegisterAsset;

public class RegisterAssetCommandHandler : IRequestHandler<RegisterAssetCommand, AssetDto>
{
    private readonly IAssetDbContext _db;

    public RegisterAssetCommandHandler(IAssetDbContext db) => _db = db;

    public async Task<AssetDto> Handle(RegisterAssetCommand request, CancellationToken cancellationToken)
    {
        var duplicateSerial = await _db.Assets.AnyAsync(a => a.SerialNumber == request.SerialNumber, cancellationToken);
        if (duplicateSerial)
            throw new ConflictException($"An asset with serial number '{request.SerialNumber}' is already registered.");

        var asset = Domain.Entities.Asset.Register(
            request.OrganizationId, request.SiteId, Enum.Parse<AssetType>(request.Type, true),
            request.Manufacturer, request.Model, request.SerialNumber, request.InstallationDate,
            request.RatedCapacityKw, request.ParentAssetId);

        await _db.Assets.AddAsync(asset, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);

        return Map(asset);
    }

    public static AssetDto Map(Domain.Entities.Asset asset) => new(
        asset.Id, asset.OrganizationId, asset.SiteId, asset.ParentAssetId, asset.Type.ToString(),
        asset.Manufacturer, asset.Model, asset.SerialNumber, asset.RatedCapacityKw, asset.FirmwareVersion,
        asset.InstallationDate, asset.Status.ToString(), asset.DeviceId);
}
