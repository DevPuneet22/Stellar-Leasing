using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StellarLeasing.Application;
using StellarLeasing.Infrastructure;
using StellarLeasing.Worker.Services;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddHostedService<WorkflowHeartbeatService>();

var host = builder.Build();
await host.RunAsync();
