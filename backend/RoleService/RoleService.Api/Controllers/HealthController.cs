using RoleService.Shared.Models;
using RoleService.Shared.RateLimiting;
using Microsoft.AspNetCore.Mvc;

namespace RoleService.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
    {
        /// <summary>
        /// Verifica la salud del servicio (no tiene rate limiting)
        /// </summary>
        /// <returns>Estado del servicio</returns>
        [HttpGet]
        [AllowRateLimitBypass]
        public ActionResult<ApiResponse<string>> Get()
        {
            return ApiResponse<string>.Ok("RoleService is running");
        }
    }
}
