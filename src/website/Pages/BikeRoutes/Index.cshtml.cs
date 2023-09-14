
using CCC.Entities;
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
                var result = await API.GetForUserAsync<List<BikeRoute>>("API", options =>
                {
                    options.RelativePath = "BikeRoutes";
                });
                Logger.LogDebug("Result from API {Result}", result);
                return new JsonResult(result);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Exception trying to Fetch bike routes!");
                return new JsonResult(new {});
            }
        } 
    }
}
