
using CCC.Entities;
using CCC.website.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Abstractions;
using Microsoft.Identity.Web;

namespace CCC.website.Pages.GroupRides
{
    public class IndexPageModel : PageModelBase
    {
        public IndexPageModel(ILogger<IndexPageModel> logger, IDownstreamApi api) : base(logger, api)
        {
        }

        public List<GroupRide> GroupRides { get; set; } = new ();

        public async Task OnGetAsync()
        {
            try
            {
                var result = await API.GetForUserAsync<List<GroupRide>>("API", options =>
                {
                    options.RelativePath = "GroupRides";
                });
                Logger.LogDebug("Result from API {Result}", result);
                GroupRides = result ?? new();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Exception trying to get GroupRides!");
                CurrentPageAction = "OnGetAsync";
                CurrentPageErrorMessage = ex.Message;
            }
        }
    }
}
