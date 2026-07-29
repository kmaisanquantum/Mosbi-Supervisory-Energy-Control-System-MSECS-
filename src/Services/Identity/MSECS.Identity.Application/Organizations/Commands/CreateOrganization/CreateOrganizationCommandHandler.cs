using MediatR;
using MSECS.Identity.Application.Common.Interfaces;
using MSECS.Identity.Application.DTOs;
using MSECS.Identity.Domain.Entities;
using MSECS.Identity.Domain.Enums;

namespace MSECS.Identity.Application.Organizations.Commands.CreateOrganization;

public class CreateOrganizationCommandHandler : IRequestHandler<CreateOrganizationCommand, OrganizationDto>
{
    private readonly IIdentityDbContext _db;

    public CreateOrganizationCommandHandler(IIdentityDbContext db) => _db = db;

    public async Task<OrganizationDto> Handle(CreateOrganizationCommand request, CancellationToken cancellationToken)
    {
        var org = Organization.Create(
            request.Name,
            Enum.Parse<OrganizationType>(request.Type, true),
            request.ContactEmail,
            request.ParentOrganizationId);

        await _db.Organizations.AddAsync(org, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);

        return new OrganizationDto(org.Id, org.Name, org.Type.ToString(), org.IsActive, org.CreatedAtUtc);
    }
}
