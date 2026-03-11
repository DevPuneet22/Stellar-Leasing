namespace StellarLeasing.Api.Auth;

public sealed class DemoUserOptions
{
    public string Email { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public string Role { get; set; } = "Designer";

    public Guid TenantId { get; set; }
}
