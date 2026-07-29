using MediatR;
using MSECS.Identity.Application.DTOs;

namespace MSECS.Identity.Application.Users.Queries.GetUser;

public record GetUserQuery(Guid UserId) : IRequest<UserDto>;
