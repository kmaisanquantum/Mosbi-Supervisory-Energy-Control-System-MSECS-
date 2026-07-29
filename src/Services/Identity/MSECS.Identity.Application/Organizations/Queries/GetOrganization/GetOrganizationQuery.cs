using MediatR;
using MSECS.Identity.Application.DTOs;

namespace MSECS.Identity.Application.Organizations.Queries.GetOrganization;

public record GetOrganizationQuery(Guid OrganizationId) : IRequest<OrganizationDto>;
