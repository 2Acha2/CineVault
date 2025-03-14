using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using CineVault.API.Models.Api;

namespace CineVault.API.Controllers.V2
{
    [ApiVersion(2)]
    [Route("api/[controller]")]
    [ApiController]
    public class AppInfoController : ControllerBase
    {
        private readonly IWebHostEnvironment _env;

        public AppInfoController(IWebHostEnvironment env)
        {
            _env = env;
        }

        [HttpGet("environment")]
        public IActionResult GetEnvironment()
        {
            var response = ApiResponse<string>.Success(_env.EnvironmentName, "Environment retrieved successfully");
            return Ok(response);
        }
    }
}
