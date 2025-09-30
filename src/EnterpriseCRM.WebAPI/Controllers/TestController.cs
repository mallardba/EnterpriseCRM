using Microsoft.AspNetCore.Mvc;

namespace EnterpriseCRM.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new { message = "Enterprise CRM API is working!", timestamp = DateTime.UtcNow });
    }

    [HttpGet("health")]
    public IActionResult Health()
    {
        return Ok(new { status = "healthy", service = "Enterprise CRM API" });
    }
}
