using ErrorService.Shared;
using ErrorService.Shared.RateLimiting;
using Microsoft.AspNetCore.Mvc;

namespace ErrorService.Api.Controllers
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
            return ApiResponse<string>.Ok("ErrorService is running");
        }
    }
}