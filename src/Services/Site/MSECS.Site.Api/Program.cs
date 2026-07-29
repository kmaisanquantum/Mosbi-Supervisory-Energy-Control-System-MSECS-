using MSECS.BuildingBlocks.Auth;
using MSECS.BuildingBlocks.Extensions;
using MSECS.BuildingBlocks.Messaging;
using MSECS.BuildingBlocks.Middleware;
using MSECS.Site.Application;
using MSECS.Site.Infrastructure;
using MSECS.Site.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseMsecsSerilog(builder.Configuration, serviceName: "MSECS.Site");

builder.Services.AddSiteApplication();
builder.Services.AddSiteInfrastructure(builder.Configuration);
builder.Services.AddMsecsRabbitMq(builder.Configuration);

builder.Services.AddMsecsJwtAuthentication(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddMsecsApiVersioning();
builder.Services.AddMsecsSwagger("MSECS Site Service", "Solar site management: locations, weather zones, capacity, and commissioning status.");
builder.Services.AddMsecsRateLimiting();
builder.Services.AddMsecsHealthChecks(postgresConnectionString: builder.Configuration.GetConnectionString("SiteDb"));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<SiteDbContext>();
    await db.Database.MigrateAsync();
}

app.UseMsecsExceptionHandling();
app.UseCorrelationId();
app.UseMsecsRequestLogging();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(o => o.SwaggerEndpoint("/swagger/v1/swagger.json", "MSECS Site v1"));
}

app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapMsecsHealthChecks();

app.Run();

public partial class Program { }
