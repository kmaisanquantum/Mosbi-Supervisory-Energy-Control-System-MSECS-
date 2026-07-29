using MediatR;
using MSECS.Site.Application.Common.Interfaces;
using MSECS.Site.Application.DTOs;
using MSECS.Site.Domain.Entities;

namespace MSECS.Site.Application.Sites.Commands.CreateSite;

public class CreateSiteCommandHandler : IRequestHandler<CreateSiteCommand, SiteDto>
{
    private readonly ISiteDbContext _db;

    public CreateSiteCommandHandler(ISiteDbContext db) => _db = db;

    public async Task<SiteDto> Handle(CreateSiteCommand request, CancellationToken cancellationToken)
    {
        var site = SolarSite.Commission(
            request.OrganizationId,
            request.Name,
            new GpsCoordinates(request.Latitude, request.Longitude),
            request.WeatherZone,
            request.Timezone,
            request.InstalledCapacityKw,
            request.InstallationDate,
            request.Address);

        await _db.Sites.AddAsync(site, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);

        return Map(site);
    }

    public static SiteDto Map(SolarSite site) => new(
        site.Id, site.OrganizationId, site.Name, site.Coordinates.Latitude, site.Coordinates.Longitude,
        site.WeatherZone, site.Timezone, site.InstalledCapacityKw, site.InstallationDate, site.Address,
        site.Status.ToString());
}
