using Microsoft.Extensions.DependencyInjection;
using StellarLeasing.Application.WorkflowDefinitions;

namespace StellarLeasing.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IWorkflowDefinitionService, WorkflowDefinitionService>();
        return services;
    }
}
