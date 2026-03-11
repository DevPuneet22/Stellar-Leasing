using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using StellarLeasing.Application;
using StellarLeasing.Infrastructure;
using StellarLeasing.Infrastructure.Persistence;
using StellarLeasing.Worker.Services;

var builder = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings
{
    Args = args,
    ContentRootPath = AppContext.BaseDirectory
});
var databaseInitialization = builder.Configuration
    .GetSection("DatabaseInitialization")
    .Get<DatabaseInitializationOptions>() ?? new DatabaseInitializationOptions();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddHostedService<WorkflowHeartbeatService>();

var host = builder.Build();
await host.Services.InitializeStellarLeasingDatabaseAsync(databaseInitialization);
await host.RunAsync();
