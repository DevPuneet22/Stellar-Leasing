namespace StellarLeasing.Application.Abstractions;

public interface ICurrentTenantAccessor
{
    Guid GetCurrentTenantId();
}
