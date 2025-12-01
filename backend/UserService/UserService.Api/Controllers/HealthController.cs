using UserService.Shared;
using UserService.Shared.RateLimiting;
using Microsoft.AspNetCore.Mvc;

namespace UserService.Api.Controllers
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
            return ApiResponse<string>.Ok("UserService is running");
        }
    }
}
