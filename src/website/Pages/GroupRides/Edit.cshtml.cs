
using CCC.Entities;
using CCC.website.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Abstractions;
using Microsoft.Identity.Web;

namespace CCC.website.Pages.GroupRides
{
    public class EditPageModel : PageModelBase
    {
        public EditPageModel(ILogger<PageModelBase> logger, IDownstreamApi api) : base(logger, api)
        {
        }

        [BindProperty(SupportsGet = true)]
        public Guid Id { get; set; }

        [BindProperty]
        public GroupRide GroupRide { get; set; } = new();

        public async Task OnGetAsync()
        {
            Logger.LogTrace("Entering OnGetAsync Id {Id}", Id);
            try
            {
                GroupRide = await API.GetForUserAsync<GroupRide>("API", options => { options.RelativePath = $"GroupRides/{Id}"; }) ?? throw new Exception("API returned invalid data");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error getting GroupRide Id {id}", Id);
                CurrentPageErrorMessage = "Unable to get GroupRide";
                CurrentPageAction = "GroupRide/Edit/OnGet";
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            Logger.LogTrace("Entering OnPostAsync");
            if (GroupRide != null)
            {
                Logger.LogTrace("UpdateModel is not null");
                try
                {
                    GroupRide.Id = Id;
                    // var updateModel = System.Text.Json.JsonSerializer.Deserialize<BikeRouteUpdateModel>( System.Text.Json.JsonSerializer.Serialize(BikeRoute));
                    await API.PatchForUserAsync("API", GroupRide, options => { options.RelativePath = $"GroupRides/{Id}"; });
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "Exception trying to update GroupRide Id {Id}", Id);
                    PreviousPageAction = "GroupRide/Edit/OnPost";
                    PreviousPageErrorMessage = $"Error updating GroupRide";
                }
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync()
        {
            try
            {
                Logger.LogTrace("Entering OnPostDeleteAsync");
                // var response = await API.CallApiForUserAsync("API", options =>
                // {
                //     options.HttpMethod = HttpMethod.Delete;
                //     options.RelativePath = $"GroupRides/{Id}";
                // });

                // response.EnsureSuccessStatusCode();

                await API.DeleteForUserAsync("API", string.Empty, options =>
                {
                    options.RelativePath = $"GroupRides/{Id}";
                });
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error deleting group ride");
                PreviousPageErrorMessage = ex.Message;
                PreviousPageAction = "GroupRide/Delete/OnPost";
            }

            return RedirectToPage("Index");
        }
    }
}
