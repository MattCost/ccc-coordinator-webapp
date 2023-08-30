
using System.Data;
using CCC.Entities;
using CCC.website.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Abstractions;

namespace CCC.website.Pages.BikeRoutes
{
    public class EditPageModel : PageModelBase
    {
        [BindProperty(SupportsGet = true)]
        public Guid Id { get; set; }

        [BindProperty]
        public CueEntry CueRow { get; set; } = new();

        [BindProperty]
        public BikeRoute BikeRoute { get; set; } = new();

        public EditPageModel(ILogger<EditPageModel> logger, IDownstreamApi api) : base(logger, api)
        {

        }

        public async Task OnGetAsync()
        {
            Logger.LogTrace("Entering OnGetAsync Id {Id}", Id);
            try
            {
                BikeRoute = await API.GetForUserAsync<BikeRoute>("API", options => { options.RelativePath = $"BikeRoutes/{Id}"; }) ?? throw new Exception("API returned invalid data");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error getting BikeRoute Id {id}",Id);
                CurrentPageErrorMessage = "Unable to get BikeRoutes";
                CurrentPageAction = "BikeRoutes/Edit/OnGet";
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            Logger.LogTrace("Entering OnPostAsync");
            if(BikeRoute != null)
            {
                Logger.LogTrace("UpdateModel is not null");
                try
                {
                    BikeRoute.Id = Id;
                    // var updateModel = System.Text.Json.JsonSerializer.Deserialize<BikeRouteUpdateModel>( System.Text.Json.JsonSerializer.Serialize(BikeRoute));
                    await API.PatchForUserAsync("API", BikeRoute, options => { options.RelativePath = $"BikeRoutes/{Id}"; });
                }
                catch(Exception ex)
                {
                    Logger.LogError(ex, "Exception trying to update BikeRoute Id {Id}", Id);
                    PreviousPageAction = "BikeRoute/Edit/OnPost";
                    PreviousPageErrorMessage = $"Error updating BikeRoute";
                }
            }

            return RedirectToPage();
        }             

        public async Task<IActionResult> OnPostDeleteAsync()
        {
            Logger.LogTrace("Entering OnPostDeleteAsync");
            await API.DeleteForUserAsync("API", string.Empty, options => {
                options.RelativePath = $"BikeRoutes/{Id}";
            });
            return RedirectToPage("Index");
        }

        public async Task<IActionResult> OnPostAddCueRow()
        {
            Logger.LogTrace("Entering  OnPostAddCueRow");
            await API.PostForUserAsync("API", CueRow, options => { options.RelativePath = $"BikeRoutes/{Id}/cues";});
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteCueRow(int index)
        {
            Logger.LogTrace("Entering OnPostDeleteCueRow");
            await API.DeleteForUserAsync("API", string.Empty, options => { options.RelativePath = $"BikeRoutes/{Id}/cues/{index}"; });
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostInsertCueRow(int index = 0)
        {
            Logger.LogTrace("Entering OnPostInsertCueRow");
            await API.PatchForUserAsync("API", CueRow, options => { options.RelativePath = $"BikeRoutes/{Id}/cues/{index}";});
            return RedirectToPage();
        }

    }
}