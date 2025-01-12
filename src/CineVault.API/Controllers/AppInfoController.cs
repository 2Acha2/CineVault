using Microsoft.AspNetCore.Mvc;

namespace CineVault.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AppInfoController : ControllerBase
    {
        private readonly IWebHostEnvironment _env;

        // Інжекція залежностей для отримання середовища
        public AppInfoController(IWebHostEnvironment env)
        {
            _env = env;
        }

        // Ендпоінт для отримання поточного середовища
        [HttpGet("environment")]
        public IActionResult GetEnvironment()
        {
            return Ok(new { Environment = _env.EnvironmentName });
        }
    }
}