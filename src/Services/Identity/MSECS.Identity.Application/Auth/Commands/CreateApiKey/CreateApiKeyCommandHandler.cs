using System.Security.Cryptography;
using MediatR;
using MSECS.Identity.Application.Common.Interfaces;
using MSECS.Identity.Domain.Entities;

namespace MSECS.Identity.Application.Auth.Commands.CreateApiKey;

public class CreateApiKeyCommandHandler : IRequestHandler<CreateApiKeyCommand, CreateApiKeyResult>
{
    private readonly IIdentityDbContext _db;

    public CreateApiKeyCommandHandler(IIdentityDbContext db) => _db = db;

    public async Task<CreateApiKeyResult> Handle(CreateApiKeyCommand request, CancellationToken cancellationToken)
    {
        var rawKeyBytes = RandomNumberGenerator.GetBytes(32);
        var plaintextKey = "msk_" + Convert.ToHexString(rawKeyBytes).ToLowerInvariant();
        var keyPrefix = plaintextKey[..12];
        var keyHash = Convert.ToHexString(SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(plaintextKey))).ToLowerInvariant();

        var expiresAt = request.ExpiresInDays.HasValue
            ? DateTimeOffset.UtcNow.AddDays(request.ExpiresInDays.Value)
            : (DateTimeOffset?)null;

        var apiKey = ApiKey.Issue(request.OrganizationId, request.Name, keyHash, keyPrefix, request.Scopes, expiresAt);

        await _db.ApiKeys.AddAsync(apiKey, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);

        return new CreateApiKeyResult(apiKey.Id, plaintextKey, keyPrefix, expiresAt);
    }
}
