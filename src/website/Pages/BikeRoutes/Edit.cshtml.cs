
using System.Data;
using System.Text.Json;
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
        public BikeRoute BikeRoute { get; set; } = new();

        
        public EditPageModel(ILogger<EditPageModel> logger, IDownstreamApi api) : base(logger, api)
        {

        }

        public async Task OnGetAsync()
        {
            Logger.LogTrace("Entering OnGetAsync Id {Id}", Id);
            try
            {
                Logger.LogTrace("Fetching the route");
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

        public IActionResult OnPostDiscardCueChanges()
        {
            Logger.LogTrace("Entering OnPostDiscardCuesAsync Id {Id}", Id);
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostSaveCueChangesAsync()
        {
            Logger.LogTrace("Entering OnPostSaveCuesAsync. BikeRoute Json {Cues}", JsonSerializer.Serialize(BikeRoute));
            Logger.LogDebug("Null Hack");
            for(int i=0; i<BikeRoute.Cues.Count ; i++)
            {
                if(BikeRoute.Cues[i].StreetName == null) 
                    BikeRoute.Cues[i].StreetName = string.Empty;
                if(BikeRoute.Cues[i].Notes == null) 
                    BikeRoute.Cues[i].Notes = string.Empty;

            }
            try
            {
                await API.PutForUserAsync("API", BikeRoute.Cues, options => { options.RelativePath = $"BikeRoutes/{Id}/cues"; });
            }
            catch(Exception ex)
            {
                Logger.LogError(ex, "Exception trying to update Cues for BikeRoute Id {Id}", Id);
                PreviousPageAction = "BikeRoute/Edit/OnPostSaveCuesAsync";
                PreviousPageErrorMessage = $"Error updating BikeRoute Cues";
            }
            return RedirectToPage();            

        }


        public void OnPostRemoveCueRow(int cueIndex)
        {
            Logger.LogDebug("Entering OnPostRemoveCue Index {Index}", cueIndex);
            Logger.LogDebug("BikeRoute Json {Json}", JsonSerializer.Serialize(BikeRoute));

            if(cueIndex < BikeRoute.Cues.Count)
            {
                Logger.LogDebug("Removing cue at Index {Index}", cueIndex);
                BikeRoute.Cues.RemoveAt(cueIndex);
                ModelState.Remove(nameof(BikeRoute));
            }
            Logger.LogDebug("BikeRoute Json {Json}", JsonSerializer.Serialize(BikeRoute));

        }


        
        public void OnPostAddCueRow()
        {
            Logger.LogDebug("Entering OnPostAddCueRow");
            Logger.LogDebug("BikeRoute Json {Json}", JsonSerializer.Serialize(BikeRoute));
            BikeRoute.Cues.Add(new CueEntry { StreetName = "_new_", Notes = "_new_"});
            ModelState.Remove(nameof(BikeRoute));
            return Page();
            // redirect to myself, with the model binding param?
            // return RedirectToPage(new {Id, BikeRoute});
        }

        public void OnPostInsertCueRow(int cueIndex)
        {
            Logger.LogDebug("Entering OnPostInsertCueRow Index {Index}", cueIndex);
            Logger.LogDebug("BikeRoute Json {Json}", JsonSerializer.Serialize(BikeRoute));
            if(cueIndex < BikeRoute.Cues.Count)
            {
                Logger.LogDebug("Inserting cue at Index {Index}", cueIndex);
                BikeRoute.Cues.Insert(cueIndex, new CueEntry { StreetName = "_new_", Notes = "_new_"});
                ModelState.Remove(nameof(BikeRoute));

           

            }
            Logger.LogDebug("BikeRoute Json {Json}", JsonSerializer.Serialize(BikeRoute));
        }
        public async Task<IActionResult> OnPostDeleteAsync()
        {
            Logger.LogTrace("Entering OnPostDeleteAsync");
            await API.DeleteForUserAsync("API", string.Empty, options => {
                options.RelativePath = $"BikeRoutes/{Id}";
            });
            return RedirectToPage("Index");
        }

        // public async Task<IActionResult> OnPostAddCueRow()
        // {
        //     Logger.LogTrace("Entering  OnPostAddCueRow");
        //     await API.PostForUserAsync("API", CueRow, options => { options.RelativePath = $"BikeRoutes/{Id}/cues";});
        //     // return RedirectToPage();
        //     return new EmptyResult();
        // }

        // public async Task<IActionResult> OnPostDeleteCueRow(int index)
        // {
        //     Logger.LogTrace("Entering OnPostDeleteCueRow");
        //     await API.DeleteForUserAsync("API", string.Empty, options => { options.RelativePath = $"BikeRoutes/{Id}/cues/{index}"; });
        //     // return RedirectToPage();
        //     return new EmptyResult();
        // }

        // public async Task<IActionResult> OnPostInsertCueRow(int index = 0)
        // {
        //     Logger.LogTrace("Entering OnPostInsertCueRow");
        //     await API.PatchForUserAsync("API", CueRow, options => { options.RelativePath = $"BikeRoutes/{Id}/cues/{index}";});
        //     // return RedirectToPage();
        //     return new EmptyResult();
        // }

    }
}