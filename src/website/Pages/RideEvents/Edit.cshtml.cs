
using CCC.Entities;
using CCC.website.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Abstractions;
using Microsoft.Identity.Web;

namespace CCC.website.Pages.RideEvents
{
    public class EditPageModel : PageModelBase
    {
        public EditPageModel(ILogger<PageModelBase> logger, IDownstreamApi api) : base(logger, api)
        {
        }

        [BindProperty(SupportsGet = true)]
        public Guid Id { get; set; }

        [BindProperty]
        public RideEvent RideEvent { get; set; } = new();

        public async Task OnGetAsync()
        {
            Logger.LogTrace("Entering OnGetAsync Id {Id}", Id);
            try
            {
                RideEvent = await API.GetForUserAsync<RideEvent>("API", options => { options.RelativePath = $"RideEvents/{Id}"; }) ?? throw new Exception("API returned invalid data");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error getting RideEvent Id {id}", Id);
                CurrentPageErrorMessage = "Unable to get RideEvent";
                CurrentPageAction = "RideEvent/Edit/OnGet";
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            Logger.LogTrace("Entering OnPostAsync");
            if (RideEvent != null)
            {
                Logger.LogTrace("UpdateModel is not null");
                try
                {
                    RideEvent.Id = Id;
                    await API.PatchForUserAsync("API", RideEvent, options => { options.RelativePath = $"RideEvents/{Id}"; });
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "Exception trying to update RideEvent Id {Id}", Id);
                    PreviousPageAction = "RideEvent/Edit/OnPost";
                    PreviousPageErrorMessage = $"Error updating RideEvent";
                }
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync()
        {
            Logger.LogTrace("Entering OnPostDeleteAsync");
            try
            {
                //todo get event, delete all group rides, then delete event
                await API.DeleteForUserAsync("API", string.Empty, options => { options.RelativePath = $"RideEvents/{Id}";});
            }
            catch(Exception ex)
            {
                Logger.LogError(ex, "Exception trying to delete RideEvent Id {Id}", Id);
                PreviousPageAction = "RideEvent/Edit/OnPostDelete";
                PreviousPageErrorMessage = $"Error deleting RideEvent. {ex.Message}";
            }
            return RedirectToPage("Index");
        }
    }
}