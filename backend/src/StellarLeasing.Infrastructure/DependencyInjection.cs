using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StellarLeasing.Application.Abstractions;
using StellarLeasing.Infrastructure.Persistence;

namespace StellarLeasing.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IWorkflowDefinitionRepository, InMemoryWorkflowDefinitionRepository>();
        return services;
    }
}
