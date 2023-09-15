using CCC.Common.Authorization;
using CCC.Entities;
using CCC.Exceptions;
using CCC.Services.EntityProvider;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CCC.API.Controllers;

public class FavoritesController : EntityProviderBaseController
{
    public FavoritesController(ILogger<EntityProviderBaseController> logger, IEntityProvider entityProvider) : base(logger, entityProvider)
    {
    }

    #region BikeRoutes
    [HttpGet("BikeRoutes")]
    public async Task<ActionResult<List<Guid>>> GetFavoriteRoutes()
    {
        var userId = User.NameIdentifier();
        return await EntityProvider.GetFavoriteRoutes(userId);
    }

    [HttpPost("BikeRoutes/{bikeRouteId:guid}")]
    public async Task<ActionResult> AddFavoriteRoute(Guid bikeRouteId)
    {
        try
        {
            var route = EntityProvider.GetBikeRoute(bikeRouteId);
        }
        catch(EntityNotFoundException)
        {
            return new NotFoundResult();
        }
        catch(Exception ex)
        {
            Logger.LogError(ex, "Exception while trying to validate bikeRouteId");
            return Problem("Unable to validate bikeRouteId");
        }
        
        var userId = User.NameIdentifier();
        await EntityProvider.AddFavoriteRoute(userId, bikeRouteId);
        return Ok();
        
    }


    [HttpDelete("BikeRoutes/{bikeRouteId:guid}")]
    public async Task<ActionResult> RemoveFavoriteRoute(Guid bikeRouteId)
    {
        var userId = User.NameIdentifier();
        await EntityProvider.RemoveFavoriteRoute(userId, bikeRouteId);
        return Ok();
    }


    #endregion
}
