using MediatR;
using Microsoft.EntityFrameworkCore;
using MSECS.Asset.Application.Assets.Commands.RegisterAsset;
using MSECS.Asset.Application.Common.Interfaces;
using MSECS.Asset.Application.DTOs;
using MSECS.Asset.Domain.Enums;

namespace MSECS.Asset.Application.Assets.Queries.ListAssetsBySite;

public class ListAssetsBySiteQueryHandler : IRequestHandler<ListAssetsBySiteQuery, IReadOnlyList<AssetDto>>
{
    private readonly IAssetDbContext _db;
    public ListAssetsBySiteQueryHandler(IAssetDbContext db) => _db = db;

    public async Task<IReadOnlyList<AssetDto>> Handle(ListAssetsBySiteQuery request, CancellationToken cancellationToken)
    {
        var query = _db.Assets.AsNoTracking().Where(a => a.SiteId == request.SiteId);

        if (!string.IsNullOrWhiteSpace(request.TypeFilter) && Enum.TryParse<AssetType>(request.TypeFilter, true, out var type))
            query = query.Where(a => a.Type == type);

        var assets = await query.OrderBy(a => a.Type).ThenBy(a => a.SerialNumber).ToListAsync(cancellationToken);
        return assets.Select(RegisterAssetCommandHandler.Map).ToList();
    }
}
