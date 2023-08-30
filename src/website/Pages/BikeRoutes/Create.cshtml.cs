
using CCC.Entities;
using CCC.website.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Abstractions;
using Microsoft.Identity.Web;

namespace CCC.website.Pages.BikeRoutes
{
    public class CreatePageModel : PageModelBase
    {
        public CreatePageModel(ILogger<CreatePageModel> logger, IDownstreamApi api) : base(logger, api)
        {
        }

        [BindProperty]
        public BikeRouteCreateModel CreateModel { get; set; } = new();

        public async Task<ActionResult> OnPostAsync()
        {
            try
            {
                await API.PostForUserAsync("API", CreateModel, options =>
                {
                    options.RelativePath = "BikeRoutes";
                });
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Unable to create bikeRoute");
                PreviousPageAction = "BikeRoute/Create/OnPost";
                PreviousPageErrorMessage = "Error when trying to create route";
            }
            return RedirectToPage("Index");

        }
    }
}
