using Microsoft.AspNetCore.Mvc;

namespace StellarLeasing.Api.Controllers;

[ApiController]
[Route("api/system")]
public sealed class SystemController : ControllerBase
{
    [HttpGet("health")]
    public IActionResult Health()
    {
        return Ok(new
        {
            status = "ok",
            service = "stellar-leasing-api",
            utcTime = DateTimeOffset.UtcNow
        });
    }

    [HttpGet("info")]
    public IActionResult Info()
    {
        return Ok(new
        {
            product = "Stellar Leasing",
            architecture = "modular monolith",
            nextMilestones = new[]
            {
                "Add process runtime, task inbox, and approvals",
                "Add audit trail, comments, attachments, and reminders",
                "Replace local demo auth with provider-backed SSO and RBAC"
            }
        });
    }
}
