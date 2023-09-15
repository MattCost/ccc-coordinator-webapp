
using CCC.Entities;
using CCC.ViewModels;
using CCC.website.Models;
using Microsoft.AspNetCore.Mvc;
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
        }

        public List<BikeRoute> Routes { get; set; } = new ();

        public async Task OnGetAsync()
        {
            try
            {
                var result = await API.GetForUserAsync<List<BikeRoute>>("API", options =>
                {
                    options.RelativePath = "BikeRoutes";
                });
                Logger.LogDebug("Result from API {Result}", result);
                Routes = result ?? new();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Exception trying to get bike routes!");
                CurrentPageAction = "OnGetAsync";
                CurrentPageErrorMessage = ex.Message;
            }
        }

        public async Task<JsonResult> OnGetFetchBikeRoutes()
        {
            Logger.LogTrace("Entering OnGetFetchBikeRoutes");
            try
            {
                var routes = await API.GetForUserAsync<List<BikeRoute>>("API", options =>
                {
                    options.RelativePath = "BikeRoutes";
                });

                if(routes == null) 
                    return new JsonResult(new {});

                Logger.LogDebug("Result from API {Result}", routes);

                var favorites = await API.GetForUserAsync<List<Guid>>("API", options => {
                    options.RelativePath = "Favorites/BikeRoutes";
                });

                var viewModels = routes.Select(route => new BikeRouteViewModel(route){ IsFavorite = favorites != null && favorites.Contains(route.Id)}).ToList();
                return new JsonResult(viewModels);
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
