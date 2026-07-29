using MediatR;
using MSECS.Site.Application.DTOs;

namespace MSECS.Site.Application.Sites.Queries.GetSite;

public record GetSiteQuery(Guid SiteId) : IRequest<SiteDto>;
