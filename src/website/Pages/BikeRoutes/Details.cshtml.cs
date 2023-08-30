using CCC.Entities;
using CCC.website.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Abstractions;

namespace CCC.website.Pages.BikeRoutes
{
    public class DetailsPageModel : PageModelBase
    {
        [BindProperty(SupportsGet = true)]
        public Guid Id { get; set; }

        public BikeRoute BikeRoute { get; set; } = new();
        public DetailsPageModel(ILogger<PageModelBase> logger, IDownstreamApi api) : base(logger, api)
        {
        }
        public async Task OnGetAsync()
        {
            Logger.LogTrace("Entering OnGetAsync Id {Id}", Id);
            try
            {
                BikeRoute = await API.GetForUserAsync<BikeRoute>("API", options =>
                {
                    options.RelativePath = $"BikeRoutes/{Id}";
                }) ?? new();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error getting entity Id {id}", Id);
                CurrentPageErrorMessage = $"Unable to get BikeRoute";
                CurrentPageAction = "BikeRoute/Details/OnGet";
            }
        }
    }
}