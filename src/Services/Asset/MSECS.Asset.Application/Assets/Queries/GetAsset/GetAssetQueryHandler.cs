using MediatR;
using Microsoft.EntityFrameworkCore;
using MSECS.Asset.Application.Assets.Commands.RegisterAsset;
using MSECS.Asset.Application.Common.Interfaces;
using MSECS.Asset.Application.DTOs;
using MSECS.SharedKernel.Exceptions;

namespace MSECS.Asset.Application.Assets.Queries.GetAsset;

public class GetAssetQueryHandler : IRequestHandler<GetAssetQuery, AssetDto>
{
    private readonly IAssetDbContext _db;
    public GetAssetQueryHandler(IAssetDbContext db) => _db = db;

    public async Task<AssetDto> Handle(GetAssetQuery request, CancellationToken cancellationToken)
    {
        var asset = await _db.Assets.AsNoTracking().FirstOrDefaultAsync(a => a.Id == request.AssetId, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.Asset), request.AssetId);
        return RegisterAssetCommandHandler.Map(asset);
    }
}
