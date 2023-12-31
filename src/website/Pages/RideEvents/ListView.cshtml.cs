
using CCC.Entities;
using CCC.website.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Abstractions;
using Microsoft.Identity.Web;

namespace CCC.website.Pages.RideEvents
{
    public class ListViewPageModel : PageModelBase
    {
        public ListViewPageModel(ILogger<ListViewPageModel> logger, IDownstreamApi api) : base(logger, api)
        {
        }

        public List<RideEvent> RideEvents { get; set; } = new ();

        public async Task OnGetAsync()
        {
            try
            {
                var result = await API.GetForUserAsync<List<RideEvent>>("API", options =>
                {
                    options.RelativePath = "RideEvents";
                });
                Logger.LogDebug("Result from API {Result}", result);
                RideEvents = result ?? new();
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
