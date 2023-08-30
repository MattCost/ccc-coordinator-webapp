
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
                await API.PostForUserAsync("API", CreateModel, options =>
                {
                    options.RelativePath = "RideEvents";
                });
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
