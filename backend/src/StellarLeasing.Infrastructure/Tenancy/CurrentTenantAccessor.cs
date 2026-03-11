using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using StellarLeasing.Application.Abstractions;

namespace StellarLeasing.Infrastructure.Tenancy;

public sealed class CurrentTenantAccessor : ICurrentTenantAccessor
{
    private const string TenantClaimType = "tenant_id";
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly TenantAccessOptions _options;

    public CurrentTenantAccessor(
        IHttpContextAccessor httpContextAccessor,
        IOptions<TenantAccessOptions> options)
    {
        _httpContextAccessor = httpContextAccessor;
        _options = options.Value;
    }

    public Guid GetCurrentTenantId()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        var claimValue = user?.FindFirst(TenantClaimType)?.Value;
        if (!string.IsNullOrWhiteSpace(claimValue))
        {
            if (Guid.TryParse(claimValue, out var claimTenantId))
            {
                return claimTenantId;
            }

            throw new InvalidOperationException("Authenticated user does not have a valid tenant claim.");
        }

        var value = _httpContextAccessor.HttpContext?.Request.Headers[_options.HeaderName].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(value))
        {
            return _options.DefaultTenantId;
        }

        if (Guid.TryParse(value, out var tenantId))
        {
            return tenantId;
        }

        throw new ArgumentException(
            $"Tenant header '{_options.HeaderName}' must contain a valid GUID value.");
    }
}
