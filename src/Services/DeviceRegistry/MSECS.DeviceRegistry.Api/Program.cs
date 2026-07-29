using MSECS.BuildingBlocks.Auth;
using MSECS.BuildingBlocks.Extensions;
using MSECS.BuildingBlocks.Messaging;
using MSECS.BuildingBlocks.Middleware;
using MSECS.DeviceRegistry.Application;
using MSECS.DeviceRegistry.Infrastructure;
using MSECS.DeviceRegistry.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseMsecsSerilog(builder.Configuration, serviceName: "MSECS.DeviceRegistry");

builder.Services.AddDeviceRegistryApplication();
builder.Services.AddDeviceRegistryInfrastructure(builder.Configuration);
builder.Services.AddMsecsRabbitMq(builder.Configuration);
builder.Services.AddMsecsJwtAuthentication(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddMsecsApiVersioning();
builder.Services.AddMsecsSwagger("MSECS Device Registry", "Device provisioning, credentials, protocol configuration, and health status.");
builder.Services.AddMsecsRateLimiting();
builder.Services.AddMsecsHealthChecks(postgresConnectionString: builder.Configuration.GetConnectionString("DeviceRegistryDb"));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<DeviceDbContext>();
    await db.Database.MigrateAsync();
}

app.UseMsecsExceptionHandling();
app.UseCorrelationId();
app.UseMsecsRequestLogging();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(o => o.SwaggerEndpoint("/swagger/v1/swagger.json", "MSECS Device Registry v1"));
}

app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapMsecsHealthChecks();

app.Run();

public partial class Program { }
