using MediatR;
using Microsoft.EntityFrameworkCore;
using MSECS.Identity.Application.Common.Interfaces;
using MSECS.Identity.Application.DTOs;
using MSECS.SharedKernel.Exceptions;

namespace MSECS.Identity.Application.Organizations.Queries.GetOrganization;

public class GetOrganizationQueryHandler : IRequestHandler<GetOrganizationQuery, OrganizationDto>
{
    private readonly IIdentityDbContext _db;

    public GetOrganizationQueryHandler(IIdentityDbContext db) => _db = db;

    public async Task<OrganizationDto> Handle(GetOrganizationQuery request, CancellationToken cancellationToken)
    {
        var org = await _db.Organizations.AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == request.OrganizationId, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.Organization), request.OrganizationId);

        return new OrganizationDto(org.Id, org.Name, org.Type.ToString(), org.IsActive, org.CreatedAtUtc);
    }
}
