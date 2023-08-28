
using CCC.Entities;
using CCC.website.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Abstractions;

namespace CCC.website.Areas.BikeRoutes.Pages
{
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
    }
}
