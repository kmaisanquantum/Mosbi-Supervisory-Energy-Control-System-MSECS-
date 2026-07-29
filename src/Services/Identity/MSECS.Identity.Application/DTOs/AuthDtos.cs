namespace MSECS.Identity.Application.DTOs;

public record UserDto(
    Guid Id,
    Guid OrganizationId,
    string Email,
    string FirstName,
    string LastName,
    IReadOnlyList<string> Roles,
    bool IsEmailVerified,
    DateTimeOffset? LastLoginAtUtc);

public record AuthResultDto(
    string AccessToken,
    DateTimeOffset AccessTokenExpiresAtUtc,
    string RefreshToken,
    DateTimeOffset RefreshTokenExpiresAtUtc,
    UserDto User);

public record OrganizationDto(
    Guid Id,
    string Name,
    string Type,
    bool IsActive,
    DateTimeOffset CreatedAtUtc);
