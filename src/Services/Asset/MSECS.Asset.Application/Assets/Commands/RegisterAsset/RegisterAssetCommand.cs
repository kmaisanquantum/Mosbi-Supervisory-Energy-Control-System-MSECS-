using MediatR;
using MSECS.Asset.Application.DTOs;

namespace MSECS.Asset.Application.Assets.Commands.RegisterAsset;

public record RegisterAssetCommand(
    Guid OrganizationId,
    Guid SiteId,
    string Type,
    string Manufacturer,
    string Model,
    string SerialNumber,
    DateOnly InstallationDate,
    decimal? RatedCapacityKw,
    Guid? ParentAssetId) : IRequest<AssetDto>;
