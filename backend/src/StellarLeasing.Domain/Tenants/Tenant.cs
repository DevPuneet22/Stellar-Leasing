using StellarLeasing.Domain.Common;

namespace StellarLeasing.Domain.Tenants;

public sealed class Tenant : Entity
{
    public Tenant(string name, string slug)
    {
        Name = name;
        Slug = slug;
    }

    public string Name { get; private set; }

    public string Slug { get; private set; }
}
