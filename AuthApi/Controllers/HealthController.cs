using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    [AllowAnonymous]
    [HttpGet("status")]
    public IActionResult GetStatus()
    {
        return Ok(new { status = "healthy", timestamp = DateTime.UtcNow });
    }
}
