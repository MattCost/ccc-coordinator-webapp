
using CCC.Entities;
using CCC.ViewModels;
using CCC.website.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Graph.Models;
using Microsoft.Identity.Abstractions;
using Microsoft.Identity.Web;

namespace CCC.website.Pages.BikeRoutes
{
   
    // [AuthorizeForScopes(Scopes = new string[] {"https://cccwebapp.onmicrosoft.com/ccc-webapp-api/API.Access"})]
    // AuthorizeForScopes not needed to trigger incremental consent, as we only have the 1 scope in this app
    public class IndexPageModel : PageModelBase
    {
        public IndexPageModel(ILogger<IndexPageModel> logger, IDownstreamApi api) : base(logger, api)
        {
            Logger.LogTrace("BikeRoutes Index Ctor Trace");
            Logger.LogDebug("BikeRoutes Index Ctor Debug");
            Logger.LogInformation("BikeRoutes Index Ctor Information");
        }

        public async Task<JsonResult> OnGetFetchBikeRoutes()
        {
            Logger.LogTrace("Entering OnGetFetchBikeRoutes");
            try
            {
                var routes = await API.GetForUserAsync<List<BikeRouteViewModel>>("API", options => {
                    options.RelativePath = "ViewModels/BikeRoutes";
                });
                return new JsonResult(routes);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Exception trying to Fetch bike routes!");
                return new JsonResult(new {});
            }
        }

        public async Task OnPostAddFavoriteRoute(Guid routeId)
        {
            Logger.LogTrace("Adding Route {RouteId} to favorites", routeId);
            try
            {
                await API.PostForUserAsync("API", string.Empty, options => {
                    options.RelativePath = $"Favorites/BikeRoutes/{routeId}";
                });
            }
            catch(Exception ex)
            {
                Logger.LogError(ex, "Exception trying to add route to favorites");
            }
        }

        public async Task OnPostRemoveFavoriteRoute(Guid routeId)
        {
            Logger.LogTrace("Removing Route {RouteId} from favorites", routeId);
            try
            {
                await API.DeleteForUserAsync("API", string.Empty, options => {
                    options.RelativePath = $"Favorites/BikeRoutes/{routeId}";
                });
            }
            catch(Exception ex)
            {
                Logger.LogError(ex, "Exception trying to remove route from favorites");
            }
        }

        
    }
}
