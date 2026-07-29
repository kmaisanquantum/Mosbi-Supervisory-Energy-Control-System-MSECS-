using MediatR;
using MSECS.Identity.Application.DTOs;

namespace MSECS.Identity.Application.Auth.Commands.Register;

/// <summary>
/// Self-service signup: creates a new Organization (as Installer or AssetOwner) plus its
/// first user, who is assigned OrgAdmin. Inviting additional users into an existing org
/// is a separate, permission-checked command (not included in this pass).
/// </summary>
public record RegisterUserCommand(
    string OrganizationName,
    string OrganizationType,
    string Email,
    string Password,
    string FirstName,
    string LastName) : IRequest<AuthResultDto>;
