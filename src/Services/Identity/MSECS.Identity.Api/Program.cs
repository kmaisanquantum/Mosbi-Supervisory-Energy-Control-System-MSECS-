using System.Threading.RateLimiting;
using MSECS.BuildingBlocks.Auth;
using MSECS.BuildingBlocks.Extensions;
using MSECS.BuildingBlocks.Messaging;
using MSECS.BuildingBlocks.Middleware;
using MSECS.Identity.Application;
using MSECS.Identity.Infrastructure;
using MSECS.Identity.Infrastructure.Persistence;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseMsecsSerilog(builder.Configuration, serviceName: "MSECS.Identity");

// ---- Application & Infrastructure ----
builder.Services.AddIdentityApplication();
builder.Services.AddIdentityInfrastructure(builder.Configuration);
builder.Services.AddMsecsRabbitMq(builder.Configuration);

var redisConnection = builder.Configuration.GetConnectionString("Redis");
if (!string.IsNullOrWhiteSpace(redisConnection))
{
    builder.Services.AddMsecsRedis(redisConnection);
}

// ---- Auth ----
builder.Services.AddMsecsJwtAuthentication(builder.Configuration);

// ---- API plumbing ----
builder.Services.AddControllers();
builder.Services.AddMsecsApiVersioning();
builder.Services.AddMsecsSwagger("MSECS Identity Service", "Authentication, authorization, organizations, and multi-tenant identity for the Mosbi Supervisory Energy Control System.");
builder.Services.AddMsecsRateLimiting();
builder.Services.AddMsecsHealthChecks(
    postgresConnectionString: builder.Configuration.GetConnectionString("IdentityDb"),
    redisConnectionString: redisConnection,
    rabbitMqConnectionString: $"amqp://{builder.Configuration["RabbitMq:UserName"]}:{builder.Configuration["RabbitMq:Password"]}@{builder.Configuration["RabbitMq:HostName"]}:{builder.Configuration["RabbitMq:Port"]}{builder.Configuration["RabbitMq:VirtualHost"]}");

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins(builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>())
              .AllowAnyHeader()
              .AllowAnyMethod());
});

var app = builder.Build();

// ---- Apply migrations + seed on startup (dev/prototype convenience) ----
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
    await db.Database.MigrateAsync();
    await IdentitySeedData.SeedAsync(db);
}

app.UseMsecsExceptionHandling();
app.UseCorrelationId();
app.UseMsecsRequestLogging();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/swagger/v1/swagger.json", "MSECS Identity v1"));
}

app.UseCors();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapMsecsHealthChecks();

app.Run();

// Exposed for WebApplicationFactory-based integration tests.
public partial class Program { }
