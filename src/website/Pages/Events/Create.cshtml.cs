
using CCC.Entities;
using CCC.website.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Abstractions;
using Microsoft.Identity.Web;
using System.ComponentModel.DataAnnotations;

namespace CCC.website.Pages.Events
{
    public class CreatePageModel : PageModelBase
    {
        public CreatePageModel(ILogger<CreatePageModel> logger, IDownstreamApi api) : base(logger, api)
        {
        }

        [BindProperty]
        public RideEventCreateModel CreateModel { get; set; } = new();
        
        [BindProperty, DataType(DataType.Date)]
        public DateTime Date { get; set; } = DateTime.Now.Date;
        [BindProperty, DataType(DataType.Time)]
        public DateTime Time { get; set; }        

        public async Task<ActionResult> OnPostAsync()
        {
            Logger.LogDebug("Entering OnPostAsync. CreateModel: {CreateModel}", System.Text.Json.JsonSerializer.Serialize(CreateModel));
            CreateModel.StartTime = Date.Add(Time.TimeOfDay);
            
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
                    return RedirectToPage("/Index");
                }
                return RedirectToPage( "Edit", new { id = newModel.Id});
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Unable to create Ride Event");
                PreviousPageAction = "RideEvents/Create/OnPost";
                PreviousPageErrorMessage = "Error when trying to create ride event";
            }
            return RedirectToPage("/Index");

        }
    }
}
