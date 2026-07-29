using MediatR;
using MSECS.Site.Application.DTOs;

namespace MSECS.Site.Application.Sites.Commands.UpdateSite;

public record UpdateSiteCommand(
    Guid SiteId,
    string Name,
    decimal InstalledCapacityKw) : IRequest<SiteDto>;
