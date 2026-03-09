using StellarLeasing.Application;
using StellarLeasing.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddSimpleConsole();
builder.Logging.AddDebug();

builder.Services.AddProblemDetails();
builder.Services.AddControllers();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

app.UseExceptionHandler();
app.UseAuthorization();

app.MapGet("/", () => Results.Ok(new
{
    service = "stellar-leasing-api",
    status = "ready",
    docs = "/api/system/info"
}));

app.MapControllers();

app.Run();
