using CCC.Entities;
using CCC.website.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Abstractions;

namespace CCC.website.Pages.GroupRides
{
    public class DetailsPageModel : PageModelBase
    {
        [BindProperty(SupportsGet = true)]
        public Guid Id { get; set; }

        public GroupRide GroupRide { get; set; } = new();

        public DetailsPageModel(ILogger<PageModelBase> logger, IDownstreamApi api) : base(logger, api)
        {
        }

        public async Task OnGetAsync()
        {
            Logger.LogTrace("Entering OnGetAsync Id {Id}", Id);
            try
            {
                GroupRide = await API.GetForUserAsync<GroupRide>("API", options =>
                {
                    options.RelativePath = $"GroupRides/{Id}";
                }) ?? new();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error getting entity Id {id}", Id);
                CurrentPageErrorMessage = $"Unable to get GroupRide";
                CurrentPageAction = "GroupRide/Details/OnGet";
            }
        }

        public async Task<ActionResult> OnPostSignupAsync(CoordinatorRole role, string coordinatorId)
        {
            Logger.LogTrace("Entering OnPostSignupAsync");
            await API.PatchForUserAsync("API", coordinatorId, options => { options.RelativePath = $"GroupRides/{Id}/coordinators/{role}";});
            return RedirectToPage(); //do I need to pass id?
        }
    }
}