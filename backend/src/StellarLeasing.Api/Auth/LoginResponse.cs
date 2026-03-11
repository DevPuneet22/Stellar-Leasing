namespace StellarLeasing.Api.Auth;

public sealed record LoginResponse(
    string AccessToken,
    DateTimeOffset ExpiresAtUtc,
    string Email,
    string DisplayName,
    string Role,
    Guid TenantId);
