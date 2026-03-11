using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using System.Security.Claims;
using StellarLeasing.Api.Auth;

namespace StellarLeasing.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly LocalAuthService _localAuthService;

    public AuthController(LocalAuthService localAuthService)
    {
        _localAuthService = localAuthService;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public ActionResult<LoginResponse> Login([FromBody] LoginRequest request)
    {
        try
        {
            return Ok(_localAuthService.Login(request));
        }
        catch (ArgumentException exception)
        {
            return CreateProblem(400, exception.Message);
        }
        catch (UnauthorizedAccessException exception)
        {
            return CreateProblem(401, exception.Message);
        }
    }

    [Authorize]
    [HttpGet("me")]
    public IActionResult Me()
    {
        return Ok(new
        {
            email = User.FindFirst(ClaimTypes.Email)?.Value ?? User.FindFirst("email")?.Value,
            displayName = User.FindFirst(ClaimTypes.Name)?.Value ?? User.FindFirst("name")?.Value,
            role = User.FindFirst(ClaimTypes.Role)?.Value,
            tenantId = User.FindFirst(LocalAuthService.TenantClaimType)?.Value
        });
    }

    private ActionResult CreateProblem(int statusCode, string detail)
    {
        return Problem(
            statusCode: statusCode,
            title: ReasonPhrases.GetReasonPhrase(statusCode),
            detail: detail);
    }
}
