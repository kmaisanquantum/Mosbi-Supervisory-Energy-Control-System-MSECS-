using MediatR;
using Microsoft.EntityFrameworkCore;
using MSECS.SharedKernel.Common;
using MSECS.Site.Application.Common.Interfaces;
using MSECS.Site.Application.DTOs;
using MSECS.Site.Application.Sites.Commands.CreateSite;

namespace MSECS.Site.Application.Sites.Queries.ListSites;

public class ListSitesQueryHandler : IRequestHandler<ListSitesQuery, PagedList<SiteDto>>
{
    private readonly ISiteDbContext _db;

    public ListSitesQueryHandler(ISiteDbContext db) => _db = db;

    public async Task<PagedList<SiteDto>> Handle(ListSitesQuery request, CancellationToken cancellationToken)
    {
        var query = _db.Sites.AsNoTracking().Where(s => s.OrganizationId == request.OrganizationId).OrderBy(s => s.Name);

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedList<SiteDto>(items.Select(CreateSiteCommandHandler.Map).ToList(), total, request.PageNumber, request.PageSize);
    }
}
