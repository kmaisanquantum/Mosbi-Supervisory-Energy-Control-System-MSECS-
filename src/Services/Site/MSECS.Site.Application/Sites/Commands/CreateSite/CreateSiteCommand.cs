using MediatR;
using MSECS.Site.Application.DTOs;

namespace MSECS.Site.Application.Sites.Commands.CreateSite;

public record CreateSiteCommand(
    Guid OrganizationId,
    string Name,
    double Latitude,
    double Longitude,
    string WeatherZone,
    string Timezone,
    decimal InstalledCapacityKw,
    DateOnly InstallationDate,
    string? Address) : IRequest<SiteDto>;
