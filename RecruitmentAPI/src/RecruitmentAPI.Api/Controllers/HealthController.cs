using Microsoft.AspNetCore.Mvc;

namespace RecruitmentAPI.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok(new { Status = "Healthy", Service = "RecruitmentAPI", Timestamp = DateTime.UtcNow });
}
