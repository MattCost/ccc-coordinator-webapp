
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
            Logger.LogTrace("BikeRoutes Index Ctor Trace");
            Logger.LogDebug("BikeRoutes Index Ctor Debug");
            Logger.LogInformation("BikeRoutes Index Ctor Information");
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
            Logger.LogTrace("Exiting OnGetAsync Id {Id}", Id);
        }

        public async Task<IActionResult> OnPostUpdateAsync()
        {
            Logger.LogTrace("Entering OnPostUpdateAsync");
            if(BikeRoute != null)
            {
                Logger.LogTrace("UpdateModel is not null");

                try
                {
                    await API.PatchForUserAsync("API", BikeRoute, options => { options.RelativePath = $"BikeRoutes/{Id}"; });
                    
                    Logger.LogDebug("Null Hack");
                    for(int i=0; i<BikeRoute.Cues.Count ; i++)
                    {
                        if(BikeRoute.Cues[i].StreetName == null) 
                            BikeRoute.Cues[i].StreetName = string.Empty;
                        if(BikeRoute.Cues[i].Notes == null) 
                            BikeRoute.Cues[i].Notes = string.Empty;
                    }
                    await API.PutForUserAsync("API", BikeRoute.Cues, options => { options.RelativePath = $"BikeRoutes/{Id}/cues"; });

                }
                catch(Exception ex)
                {
                    Logger.LogError(ex, "Exception trying to update BikeRoute Id {Id}", Id);
                    PreviousPageAction = "BikeRoute/Edit/OnPost";
                    PreviousPageErrorMessage = $"Error updating BikeRoute";
                }
            }
            Logger.LogTrace("Exiting OnPostUpdateAsync");
            return RedirectToPage();
        }

        public IActionResult OnPostDiscardChanges()
        {
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
            Logger.LogTrace("Exiting OnPostSaveCueChangesAsync");
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
                ModelState.Clear();
            }
            Logger.LogDebug("BikeRoute Json {Json}", JsonSerializer.Serialize(BikeRoute));

        }

        public void OnPostAddCueRow()
        {
            Logger.LogTrace("Entering OnPostAddCueRow");
            Logger.LogDebug("BikeRoute Json {Json}", JsonSerializer.Serialize(BikeRoute));
            BikeRoute.Cues.Add(new CueEntry());
            ModelState.Clear();
            Logger.LogTrace("Exiting OnPostAddCueRow");

        }

        public void OnPostInsertCueRow(int cueIndex)
        {
            Logger.LogTrace("Entering OnPostInsertCueRow Index {Index}", cueIndex);
            Logger.LogDebug("BikeRoute Json {Json}", JsonSerializer.Serialize(BikeRoute));
            if(cueIndex < BikeRoute.Cues.Count)
            {
                Logger.LogDebug("Inserting cue at Index {Index}", cueIndex);
                BikeRoute.Cues.Insert(cueIndex, new CueEntry());
                ModelState.Clear();
            }
            Logger.LogTrace("BikeRoute Json {Json}", JsonSerializer.Serialize(BikeRoute));
            Logger.LogTrace("Exiting OnPostInsertCueRow");
        }

        public async Task<IActionResult> OnPostDeleteAsync()
        {
            Logger.LogTrace("Entering OnPostDeleteAsync");
            await API.DeleteForUserAsync("API", string.Empty, options => {
                options.RelativePath = $"BikeRoutes/{Id}";
            });
            return RedirectToPage("Index");
        }

    }
}