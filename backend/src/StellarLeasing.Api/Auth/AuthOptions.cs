namespace StellarLeasing.Api.Auth;

public sealed class AuthOptions
{
    public const string SectionName = "Auth";

    public string Issuer { get; set; } = "stellar-leasing-api";

    public string Audience { get; set; } = "stellar-leasing-frontend";

    public string SigningKey { get; set; } = "change-this-local-demo-signing-key-before-production";

    public int TokenLifetimeMinutes { get; set; } = 480;

    public List<DemoUserOptions> DemoUsers { get; set; } = [];
}
