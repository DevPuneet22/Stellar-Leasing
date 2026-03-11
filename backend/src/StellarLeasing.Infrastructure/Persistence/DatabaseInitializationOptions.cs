namespace StellarLeasing.Infrastructure.Persistence;

public sealed class DatabaseInitializationOptions
{
    public bool ApplyMigrationsOnStartup { get; set; }

    public bool SeedDemoData { get; set; }
}
