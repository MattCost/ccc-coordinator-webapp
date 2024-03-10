
using CCC.Entities;
using CCC.website.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Abstractions;
using Microsoft.Identity.Web;

namespace CCC.website.Pages.Events
{
    public class IndexPageModel : PageModelBase
    {
        public IndexPageModel(ILogger<IndexPageModel> logger, IDownstreamApi api) : base(logger, api)
        {
        }

        public List<RideEvent> RideEvents { get; set; } = new ();

        public async Task<JsonResult> OnGetFetchRideEvents()
        {
            try
            {
                var result = await API.GetForUserAsync<List<RideEvent>>("API", options =>
                {
                    options.RelativePath = "RideEvents";
                });
                Logger.LogDebug("Result from API {Result}", result);
                return new JsonResult(result ?? new List<RideEvent>() );

            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Exception trying to get RideEvents!");
                return new JsonResult(new {});
            }

        }

    }
}
