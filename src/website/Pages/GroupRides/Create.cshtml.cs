
using CCC.Entities;
using CCC.website.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Abstractions;
using Microsoft.Identity.Web;

namespace CCC.website.Pages.GroupRides
{
    public class CreatePageModel : PageModelBase
    {
        public CreatePageModel(ILogger<CreatePageModel> logger, IDownstreamApi api) : base(logger, api)
        {
        }

        [BindProperty]
        public GroupRideCreateModel CreateModel { get; set; } = new();

        public async Task<ActionResult> OnPostAsync()
        {
            try
            {
                await API.PostForUserAsync("API", CreateModel, options =>
                {
                    options.RelativePath = "GroupRides";
                });
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Unable to create Group Ride");
                PreviousPageAction = "GroupRides/Create/OnPost";
                PreviousPageErrorMessage = "Error when trying to create group ride";
            }
            return RedirectToPage("Index");

        }
    }
}
