
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CCC.API.Controllers;

[AllowAnonymous]
[ApiController]
[Route("api/[controller]")]
public class HelloWorldController : ControllerBase
{
    [HttpGet]
    public IActionResult Hello()
    {
        return new JsonResult("Hello World");
    } 
}
