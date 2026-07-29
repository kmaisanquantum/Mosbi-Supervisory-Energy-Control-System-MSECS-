using MediatR;
using MSECS.Identity.Application.DTOs;

namespace MSECS.Identity.Application.Organizations.Commands.CreateOrganization;

/// <summary>Used by an OrgAdmin/Installer to create a sub-organization (e.g. a customer account).</summary>
public record CreateOrganizationCommand(
    string Name,
    string Type,
    string? ContactEmail,
    Guid? ParentOrganizationId) : IRequest<OrganizationDto>;
