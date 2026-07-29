using MediatR;
using MSECS.Asset.Application.DTOs;

namespace MSECS.Asset.Application.Assets.Queries.GetAsset;

public record GetAssetQuery(Guid AssetId) : IRequest<AssetDto>;
