using MediatR;
using Microsoft.EntityFrameworkCore;
using MSECS.Site.Application.Common.Interfaces;
using MSECS.Site.Application.DTOs;
using MSECS.Site.Application.Sites.Commands.CreateSite;
using MSECS.SharedKernel.Exceptions;

namespace MSECS.Site.Application.Sites.Queries.GetSite;

public class GetSiteQueryHandler : IRequestHandler<GetSiteQuery, SiteDto>
{
    private readonly ISiteDbContext _db;

    public GetSiteQueryHandler(ISiteDbContext db) => _db = db;

    public async Task<SiteDto> Handle(GetSiteQuery request, CancellationToken cancellationToken)
    {
        var site = await _db.Sites.AsNoTracking().FirstOrDefaultAsync(s => s.Id == request.SiteId, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.SolarSite), request.SiteId);

        return CreateSiteCommandHandler.Map(site);
    }
}
