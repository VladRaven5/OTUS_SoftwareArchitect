using Microsoft.AspNetCore.Mvc;
using SoftwareArchitectHW1.DTO;

namespace SoftwareArchitectHW1.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class HealthController : ControllerBase
    {
        [HttpGet]
        public HealthcheckResult CheckHealth()
        {
            return HealthcheckResult.Ok();
        }
    }
}