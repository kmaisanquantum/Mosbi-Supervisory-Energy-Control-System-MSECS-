using MSECS.BuildingBlocks.Auth;
using MSECS.BuildingBlocks.Extensions;
using MSECS.BuildingBlocks.Messaging;
using MSECS.BuildingBlocks.Middleware;
using MSECS.Telemetry.Application;
using MSECS.Telemetry.Infrastructure;
using MSECS.Telemetry.Infrastructure.Persistence;
using MSECS.Telemetry.ProtocolAdapters;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseMsecsSerilog(builder.Configuration, serviceName: "MSECS.Telemetry");

builder.Services.AddTelemetryApplication();
builder.Services.AddTelemetryInfrastructure(builder.Configuration);
builder.Services.AddMsecsProtocolAdapters(builder.Configuration);
builder.Services.AddMsecsRabbitMq(builder.Configuration);
builder.Services.AddMsecsJwtAuthentication(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddMsecsApiVersioning();
builder.Services.AddMsecsSwagger("MSECS Telemetry Service", "High-volume solar telemetry ingestion (REST, MQTT, Modbus TCP) backed by TimescaleDB.");
builder.Services.AddMsecsRateLimiting();
builder.Services.AddMsecsHealthChecks(postgresConnectionString: builder.Configuration.GetConnectionString("TelemetryDb"));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<TelemetryDbContext>();
    await db.Database.MigrateAsync();
}
await app.Services.EnsureTimescaleHypertableAsync();

app.UseMsecsExceptionHandling();
app.UseCorrelationId();
app.UseMsecsRequestLogging();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(o => o.SwaggerEndpoint("/swagger/v1/swagger.json", "MSECS Telemetry v1"));
}

app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapMsecsHealthChecks();

app.Run();

public partial class Program { }
