using MSECS.BuildingBlocks.Auth;
using MSECS.BuildingBlocks.Extensions;
using MSECS.BuildingBlocks.Messaging;
using MSECS.BuildingBlocks.Middleware;
using MSECS.Asset.Application;
using MSECS.Asset.Infrastructure;
using MSECS.Asset.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseMsecsSerilog(builder.Configuration, serviceName: "MSECS.Asset");

builder.Services.AddAssetApplication();
builder.Services.AddAssetInfrastructure(builder.Configuration);
builder.Services.AddMsecsRabbitMq(builder.Configuration);
builder.Services.AddMsecsJwtAuthentication(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddMsecsApiVersioning();
builder.Services.AddMsecsSwagger("MSECS Asset Service", "Solar equipment inventory: arrays, panels, inverters, batteries, meters, weather stations, and maintenance history.");
builder.Services.AddMsecsRateLimiting();
builder.Services.AddMsecsHealthChecks(postgresConnectionString: builder.Configuration.GetConnectionString("AssetDb"));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AssetDbContext>();
    await db.Database.MigrateAsync();
}

app.UseMsecsExceptionHandling();
app.UseCorrelationId();
app.UseMsecsRequestLogging();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(o => o.SwaggerEndpoint("/swagger/v1/swagger.json", "MSECS Asset v1"));
}

app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapMsecsHealthChecks();

app.Run();

public partial class Program { }
