
using CCC.Entities;
using CCC.website.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Abstractions;
using Microsoft.Identity.Web;

namespace CCC.website.Pages.RideEvents
{
    public class IndexPageModel : PageModelBase
    {
        public IndexPageModel(ILogger<IndexPageModel> logger, IDownstreamApi api) : base(logger, api)
        {
        }

        // public List<RideEvent> RideEvents { get; set; } = new ();

        public async Task OnGetAsync()
        {
            try
            {
                var result = await API.GetForUserAsync<List<RideEvent>>("API", options =>
                {
                    options.RelativePath = "RideEvents";
                });
                Logger.LogDebug("Result from API {Result}", result);
                ViewData["RideEvents"] = result ?? new List<RideEvent>();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Exception trying to get RideEvents!");
                CurrentPageAction = "OnGetAsync";
                CurrentPageErrorMessage = ex.Message;
            }
        }
    }
}
