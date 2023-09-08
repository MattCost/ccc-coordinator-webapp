using CCC.Entities;
using CCC.website.Models;
using Microsoft.Identity.Abstractions;

namespace website.Pages;

public class IndexModel : PageModelBase
{
    public IndexModel(ILogger<IndexModel> logger, IDownstreamApi api) : base(logger, api)
    {
    }

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
