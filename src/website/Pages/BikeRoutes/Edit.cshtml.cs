
using System.Data;
using System.Text.Json;
using CCC.Entities;
using CCC.Enums;
using CCC.website.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Graph.Models.CallRecords;
using Microsoft.Identity.Abstractions;

namespace CCC.website.Pages.BikeRoutes
{
    public class EditPageModel : PageModelBase
    {
        [BindProperty(SupportsGet = true)]
        public Guid Id { get; set; }

        [BindProperty]
        public string Name {get;set;} = string.Empty;

        [BindProperty]
        public string Description {get;set;} = string.Empty;

        [BindProperty]
        public string RideWithGPSRouteId {get;set;} = string.Empty;

        [BindProperty]
        public string GarminConnectRouteId {get;set;} = string.Empty;

        [BindProperty]
        public double Distance {get;set;}

        [BindProperty]
        public List<CueEntry> Cues {get;set;} = new();

        public string CuesJson {get;set;} = string.Empty;

        public string OperationsJson {get;set;}
        
        public EditPageModel(ILogger<EditPageModel> logger, IDownstreamApi api) : base(logger, api)
        {
            Logger.LogTrace("BikeRoutes Index Ctor Trace");
            Logger.LogDebug("BikeRoutes Index Ctor Debug");
            Logger.LogInformation("BikeRoutes Index Ctor Information");
            OperationsJson = System.Text.Json.JsonSerializer.Serialize(Enum.GetValues(typeof(CueOperation)));
        }

        public async Task OnGetAsync()
        {
            Logger.LogTrace("Entering OnGetAsync Id {Id}", Id);
            try
            {
                Logger.LogTrace("Fetching the route");
                var bikeRoute = await API.GetForUserAsync<BikeRoute>("API", options => { options.RelativePath = $"BikeRoutes/{Id}"; }) ?? throw new Exception("API returned invalid data");
                Name = bikeRoute.Name;
                Description = bikeRoute.Description;
                Distance = bikeRoute.Distance;
                RideWithGPSRouteId = bikeRoute.RideWithGPSRouteId;
                GarminConnectRouteId = bikeRoute.GarminConnectRouteId;
                CuesJson = System.Text.Json.JsonSerializer.Serialize(bikeRoute.Cues);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error getting BikeRoute Id {id}",Id);
                CurrentPageErrorMessage = "Unable to get BikeRoutes";
                CurrentPageAction = "BikeRoutes/Edit/OnGet";
            }
            Logger.LogTrace("Exiting OnGetAsync Id {Id}", Id);
        }

        public async Task<JsonResult> OnPostUpdateAsync()
        {
            Logger.LogTrace("Entering OnPostUpdateAsync. Name: {Name} Desc: {Desc} Distance: {Distance}", Name, Description, Distance);
            if(string.IsNullOrEmpty(Name) || string.IsNullOrEmpty(Description))
            {
                Logger.LogError("Missing required parameter.");
                // return RedirectToPage();
                return new JsonResult( new {result="error. missing required parameter"} );
            }

            try
            {
                var updateModel = new BikeRouteUpdateModel
                {
                    Name = Name,
                    Description = Description,
                    Distance = Distance,
                    RideWithGPSRouteId = RideWithGPSRouteId,
                    GarminConnectRouteId = GarminConnectRouteId
                };
                await API.PatchForUserAsync("API", updateModel, options => { options.RelativePath = $"BikeRoutes/{Id}"; });

                Logger.LogDebug("Null Hack");
                for(int i=0; i<Cues.Count ; i++)
                {
                    if(Cues[i].Description == null) 
                        Cues[i].Description = string.Empty;
                    // if(Cues[i].StreetName == null) 
                    //     Cues[i].StreetName = string.Empty;
                    // if(Cues[i].Notes == null) 
                    //     Cues[i].Notes = string.Empty;
                }
                await API.PutForUserAsync("API", Cues, options => { options.RelativePath = $"BikeRoutes/{Id}/cues"; });

            }
            catch(Exception ex)
            {
                Logger.LogError(ex, "Exception trying to update BikeRoute Id {Id}", Id);
                PreviousPageAction = "BikeRoute/Edit/OnPost";
                PreviousPageErrorMessage = $"Error updating BikeRoute";
                return new JsonResult( new {result="error. missing required parameter"} );

            }
            
            Logger.LogTrace("Exiting OnPostUpdateAsync");

            return new JsonResult( new {result="success"} );
        }

        public IActionResult OnPostDiscardChanges()
        {
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

        public async Task<IActionResult> OnPostBulkCueReplace(string bulkCues)
        {
            Logger.LogTrace("Entering OnPostBulkCueReplace. cues {Cues}", bulkCues);
            var rows = bulkCues.Split('\n');
            var cueEntries = new List<CueEntry>();
            foreach(var row in rows)
            {
                var cues= row.Split(',');
                if(cues.Count() >= 3)
                {
                    Enum.TryParse(cues[0], out CueOperation operation);
                    double.TryParse(cues[2], out double distance);
                    cueEntries.Add(new CueEntry {Operation = operation, MileMarker = distance, Description = cues[1] });
                }
            }
            await API.PutForUserAsync("API", cueEntries, OperationsJson => {
                OperationsJson.RelativePath = $"BikeRoutes/{Id}/cues";
            });        
            return RedirectToPage();
        }

    }
}