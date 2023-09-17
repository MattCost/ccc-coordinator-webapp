
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
                var newModel = await API.PostForUserAsync<BikeRouteCreateModel, BikeRoute>("API", CreateModel, options =>
                {
                    options.RelativePath = "BikeRoutes";
                });

                if(newModel is null || newModel.Id == Guid.Empty)
                {
                    Logger.LogError("Unable to create bikeRoute. API Returned null");
                    PreviousPageAction = "BikeRoute/Create/OnPost";
                    PreviousPageErrorMessage = "API returned null when trying to create route";
                    return RedirectToPage("Index");
                }

                return RedirectToPage("Edit", new { id = newModel.Id});
            }
            catch (Exception ex)
            {
                //todo expose actual error
                Logger.LogError(ex, "Unable to create bikeRoute");
                PreviousPageAction = "BikeRoute/Create/OnPost";
                PreviousPageErrorMessage = "Error when trying to create route";
                return RedirectToPage("Index");
            }

        }
    }
}
