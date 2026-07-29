using MSECS.Identity.Domain.Entities;

namespace MSECS.Identity.Application.Common.Interfaces;

public record AccessTokenResult(string Token, DateTimeOffset ExpiresAtUtc);

public interface IJwtTokenGenerator
{
    AccessTokenResult GenerateAccessToken(User user, IEnumerable<string> roleNames, IEnumerable<string> permissionKeys);
    string GenerateRefreshToken();
}
