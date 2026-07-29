using MediatR;
using MSECS.Asset.Application.DTOs;

namespace MSECS.Asset.Application.Assets.Queries.ListAssetsBySite;

public record ListAssetsBySiteQuery(Guid SiteId, string? TypeFilter = null) : IRequest<IReadOnlyList<AssetDto>>;
