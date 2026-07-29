using MediatR;
using MSECS.SharedKernel.Common;
using MSECS.Site.Application.DTOs;

namespace MSECS.Site.Application.Sites.Queries.ListSites;

public record ListSitesQuery(Guid OrganizationId, int PageNumber = 1, int PageSize = 25) : IRequest<PagedList<SiteDto>>;
