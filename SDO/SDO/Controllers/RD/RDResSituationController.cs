using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SDO.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class RDResSituationController : ControllerBase
    {
        
    }
}
