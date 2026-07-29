using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace MSECS.BuildingBlocks.Auth;

public static class JwtAuthExtensions
{
    /// <summary>
    /// Wires up JWT bearer authentication consistently across every MSECS service.
    /// The Identity Service issues the tokens; every other service only validates them,
    /// which is why SigningKey/Issuer/Audience must be identical across all appsettings.
    /// </summary>
    public static IServiceCollection AddMsecsJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtOptions = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
            ?? throw new InvalidOperationException("Jwt configuration section is missing.");

        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = false; // terminated at ingress in production
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = jwtOptions.Issuer,
                ValidateAudience = true,
                ValidAudience = jwtOptions.Audience,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SigningKey)),
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromSeconds(30)
            };
        });

        services.AddAuthorization(options =>
        {
            options.AddPolicy("RequireAdmin", policy => policy.RequireRole("OrgAdmin", "SuperAdmin"));
            options.AddPolicy("RequireInstaller", policy => policy.RequireRole("Installer", "OrgAdmin", "SuperAdmin"));

            // One claim-based policy per fine-grained permission key issued by the Identity
            // Service (see SystemPermissions in MSECS.Identity.Domain). Every downstream
            // service authorizes against these policy names without needing a reference to
            // the Identity domain project — only the string values must stay in sync.
            foreach (var permission in PermissionPolicies.All)
            {
                options.AddPolicy(permission.PolicyName, policy => policy.RequireClaim("permission", permission.ClaimValue));
            }
        });

        return services;
    }
}
