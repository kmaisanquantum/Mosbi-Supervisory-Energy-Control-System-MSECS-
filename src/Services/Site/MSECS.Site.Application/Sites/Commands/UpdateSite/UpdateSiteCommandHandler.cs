using MediatR;
using Microsoft.EntityFrameworkCore;
using MSECS.Site.Application.Common.Interfaces;
using MSECS.Site.Application.DTOs;
using MSECS.Site.Application.Sites.Commands.CreateSite;
using MSECS.SharedKernel.Exceptions;

namespace MSECS.Site.Application.Sites.Commands.UpdateSite;

public class UpdateSiteCommandHandler : IRequestHandler<UpdateSiteCommand, SiteDto>
{
    private readonly ISiteDbContext _db;

    public UpdateSiteCommandHandler(ISiteDbContext db) => _db = db;

    public async Task<SiteDto> Handle(UpdateSiteCommand request, CancellationToken cancellationToken)
    {
        var site = await _db.Sites.FirstOrDefaultAsync(s => s.Id == request.SiteId, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.SolarSite), request.SiteId);

        site.Rename(request.Name);
        site.UpdateCapacity(request.InstalledCapacityKw);

        await _db.SaveChangesAsync(cancellationToken);

        return CreateSiteCommandHandler.Map(site);
    }
}
