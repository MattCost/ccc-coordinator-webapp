
using CCC.Entities;
using CCC.website.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Abstractions;
using Microsoft.Identity.Web;

namespace CCC.website.Pages.RideEvents
{
    public class CreatePageModel : PageModelBase
    {
        public CreatePageModel(ILogger<CreatePageModel> logger, IDownstreamApi api) : base(logger, api)
        {
        }

        [BindProperty]
        public RideEventCreateModel CreateModel { get; set; } = new();

        public async Task<ActionResult> OnPostAsync()
        {
            // CreateModel.StartTime = DateTime.SpecifyKind(CreateModel.StartTime, DateTimeKind.Utc);
            try
            {
                var newModel = await API.PostForUserAsync<RideEventCreateModel, RideEvent>("API", CreateModel, options =>
                {
                    options.RelativePath = "RideEvents";
                });

                if(newModel is null || newModel.Id == Guid.Empty)
                {
                    Logger.LogError("Unable to create RideEvent. API Returned null");
                    PreviousPageAction = "RideEvents/Create/OnPost";
                    PreviousPageErrorMessage = "API returned null when trying to create event";
                    return RedirectToPage("Index");
                }
                //todo redirect to page where we can add groupRides to this event  (group ride create page?)
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Unable to create Ride Event");
                PreviousPageAction = "RideEvents/Create/OnPost";
                PreviousPageErrorMessage = "Error when trying to create ride event";
            }
            return RedirectToPage("Index");

        }
    }
}
