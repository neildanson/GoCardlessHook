using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GoCardlessHook.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HealthCheckController : ControllerBase
    {
        [HttpGet(Name = "health")]
        public void Get()
        {
            
        }
    }
}
