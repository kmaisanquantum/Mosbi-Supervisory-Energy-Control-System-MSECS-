using MediatR;
using Microsoft.EntityFrameworkCore;
using MSECS.Identity.Application.Common.Interfaces;
using MSECS.Identity.Application.DTOs;
using MSECS.SharedKernel.Exceptions;

namespace MSECS.Identity.Application.Users.Queries.GetUser;

public class GetUserQueryHandler : IRequestHandler<GetUserQuery, UserDto>
{
    private readonly IIdentityDbContext _db;

    public GetUserQueryHandler(IIdentityDbContext db) => _db = db;

    public async Task<UserDto> Handle(GetUserQuery request, CancellationToken cancellationToken)
    {
        var user = await _db.Users.AsNoTracking()
            .Include(u => u.Roles)
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.User), request.UserId);

        var roleIds = user.Roles.Select(r => r.RoleId).ToList();
        var roleNames = await _db.Roles.AsNoTracking()
            .Where(r => roleIds.Contains(r.Id))
            .Select(r => r.Name)
            .ToListAsync(cancellationToken);

        return new UserDto(user.Id, user.OrganizationId, user.Email, user.FirstName, user.LastName,
            roleNames, user.IsEmailVerified, user.LastLoginAtUtc);
    }
}
