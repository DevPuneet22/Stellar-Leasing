namespace StellarLeasing.Infrastructure.Tenancy;

public sealed class TenantAccessOptions
{
    public const string SectionName = "TenantAccess";

    public string HeaderName { get; set; } = "X-Tenant-Id";

    public Guid DefaultTenantId { get; set; } = Guid.Parse("11111111-1111-1111-1111-111111111111");
}
