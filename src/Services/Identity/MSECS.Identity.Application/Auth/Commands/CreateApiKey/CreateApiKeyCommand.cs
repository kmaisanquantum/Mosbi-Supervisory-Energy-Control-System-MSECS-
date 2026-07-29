using MediatR;

namespace MSECS.Identity.Application.Auth.Commands.CreateApiKey;

/// <summary>Returns the plaintext key exactly once; only its hash is persisted.</summary>
public record CreateApiKeyCommand(Guid OrganizationId, string Name, List<string> Scopes, int? ExpiresInDays)
    : IRequest<CreateApiKeyResult>;

public record CreateApiKeyResult(Guid ApiKeyId, string PlaintextKey, string KeyPrefix, DateTimeOffset? ExpiresAtUtc);
