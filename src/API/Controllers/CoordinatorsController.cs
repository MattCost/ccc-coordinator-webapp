using CCC.Entities;
using CCC.Services.EntityProvider;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CCC.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class CoordinatorsController : EntityProviderBaseController
{
    public CoordinatorsController(ILogger<CoordinatorsController> logger, IEntityProvider entityProvider) : base(logger, entityProvider)
    {
    }
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Coordinator>>> GetAll()
    {
        Logger.LogTrace("Entering GetAll");
        // var authResult = await _authorizationService.AuthorizeAsync(User, null, SiteOperations.ListSites);
        // if(!authResult.Succeeded) return Forbid();
        return await EntityProviderActionHelper( EntityProvider.GetCoordinators, "Unable to get all coordinators");
    }    
}
