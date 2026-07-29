using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Collections.Generic;
using FluentAssertions;
using MSECS.Identity.Application.Auth.Commands.Register;
using MSECS.Identity.Application.Common.Interfaces;
using MSECS.Identity.Domain.Entities;
using MSECS.Identity.Domain.Enums;
using MSECS.SharedKernel.Exceptions;
using Xunit;

namespace MSECS.Identity.UnitTests.Auth;

public class RegisterUserCommandHandlerTests
{
    private class FakePasswordHasher : IPasswordHasher
    {
        public string Hash(string password) => $"hashed:{password}";
        public bool Verify(string password, string hash) => hash == $"hashed:{password}";
    }

    private class FakeJwtGenerator : IJwtTokenGenerator
    {
        public AccessTokenResult GenerateAccessToken(User user, IEnumerable<string> roleNames, IEnumerable<string> permissionKeys) =>
            new("fake-access-token", DateTimeOffset.UtcNow.AddMinutes(15));

        public string GenerateRefreshToken() => "fake-refresh-token";
    }

    [Fact]
    public async Task Handle_ValidRequest_CreatesOrganizationAndOrgAdminUser()
    {
        // Arrange
        await using var db = TestDbContextFactory.Create();
        var orgAdminRole = Role.CreateSystemRole(SystemRoles.OrgAdmin);
        orgAdminRole.GrantPermission(SystemPermissions.SitesRead);
        await db.Roles.AddAsync(orgAdminRole);
        await db.SaveChangesAsync();

        var handler = new RegisterUserCommandHandler(db, new FakePasswordHasher(), new FakeJwtGenerator());
        var command = new RegisterUserCommand(
            "Acme Solar Installers", "Installer", "owner@acme-solar.test", "SuperSecret123", "Ada", "Owner");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.User.Email.Should().Be("owner@acme-solar.test");
        result.User.Roles.Should().Contain(SystemRoles.OrgAdmin);
        result.AccessToken.Should().Be("fake-access-token");

        (await db.Organizations.CountAsync()).Should().Be(1);
        (await db.Users.CountAsync()).Should().Be(1);
    }

    [Fact]
    public async Task Handle_DuplicateEmail_ThrowsConflictException()
    {
        await using var db = TestDbContextFactory.Create();
        var orgAdminRole = Role.CreateSystemRole(SystemRoles.OrgAdmin);
        await db.Roles.AddAsync(orgAdminRole);

        var org = Organization.Create("Existing Org", OrganizationType.AssetOwner);
        var existingUser = User.Register(org.Id, "taken@acme-solar.test", "hash", "Existing", "User");
        await db.Organizations.AddAsync(org);
        await db.Users.AddAsync(existingUser);
        await db.SaveChangesAsync();

        var handler = new RegisterUserCommandHandler(db, new FakePasswordHasher(), new FakeJwtGenerator());
        var command = new RegisterUserCommand(
            "New Org", "Installer", "taken@acme-solar.test", "SuperSecret123", "New", "User");

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>();
    }
}
