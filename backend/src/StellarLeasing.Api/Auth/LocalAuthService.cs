using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Options;

namespace StellarLeasing.Api.Auth;

public sealed class LocalAuthService
{
    public const string TenantClaimType = "tenant_id";

    private readonly AuthOptions _options;
    private readonly SigningCredentials _signingCredentials;

    public LocalAuthService(IOptions<AuthOptions> options)
    {
        _options = options.Value;

        if (string.IsNullOrWhiteSpace(_options.SigningKey) || _options.SigningKey.Length < 32)
        {
            throw new InvalidOperationException("Auth signing key must be at least 32 characters long.");
        }

        _signingCredentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SigningKey)),
            SecurityAlgorithms.HmacSha256);
    }

    public LoginResponse Login(LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            throw new ArgumentException("Email and password are required.");
        }

        var matchedUser = _options.DemoUsers.SingleOrDefault(user =>
            string.Equals(user.Email, request.Email.Trim(), StringComparison.OrdinalIgnoreCase));

        if (matchedUser is null || !string.Equals(matchedUser.Password, request.Password, StringComparison.Ordinal))
        {
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        var expiresAtUtc = DateTimeOffset.UtcNow.AddMinutes(_options.TokenLifetimeMinutes);
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, matchedUser.Email),
            new Claim(JwtRegisteredClaimNames.Email, matchedUser.Email),
            new Claim("name", matchedUser.DisplayName),
            new Claim(ClaimTypes.Role, matchedUser.Role),
            new Claim(TenantClaimType, matchedUser.TenantId.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            expires: expiresAtUtc.UtcDateTime,
            signingCredentials: _signingCredentials);

        return new LoginResponse(
            new JwtSecurityTokenHandler().WriteToken(token),
            expiresAtUtc,
            matchedUser.Email,
            matchedUser.DisplayName,
            matchedUser.Role,
            matchedUser.TenantId);
    }
}
