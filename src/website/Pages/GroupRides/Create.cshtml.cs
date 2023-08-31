
using CCC.Entities;
using CCC.website.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Identity.Abstractions;
using Microsoft.Identity.Web;

namespace CCC.website.Pages.GroupRides
{
    public class CreatePageModel : PageModelBase
    {
        [BindProperty]
        public GroupRideCreateModel CreateModel { get; set; } = new();

        public Dictionary<string, object> ExtraProperties = new();

        // public SelectList ParentEvents { get; set; }
        // public SelectList BikeRoutes { get; set; }
        public CreatePageModel(ILogger<CreatePageModel> logger, IDownstreamApi api) : base(logger, api)
        {
            // var events = API.GetForUserAsync<List<RideEvent>>("API", options => options.RelativePath = "RideEvents").Result;
            // var listItems = events?.Select(e => new SelectListItem { Value = e.Id.ToString(), Text = $"{e.Name} - {e.Description}" });
            // ParentEvents = new SelectList(listItems, "Value", "Text");

            // var routes = API.GetForUserAsync<List<BikeRoute>>("API", options => options.RelativePath = "BikeRoutes").Result;
            // BikeRoutes = new SelectList( routes?.Select( r => new SelectListItem{ Value = r.Id.ToString(), Text = $"{r.Name} - {r.Distance} miles" }), "Value", "Text");
        }

        public async Task OnGetAsync()
        {
            var eventsTask = API.GetForUserAsync<List<RideEvent>>("API", options => options.RelativePath = "RideEvents");
            var routesTask = API.GetForUserAsync<List<BikeRoute>>("API", options => options.RelativePath = "BikeRoutes");
            
            await Task.WhenAll(eventsTask, routesTask);

            ExtraProperties["ParentEvents"] = new SelectList(eventsTask.Result?.Select(e => new SelectListItem { Value = e.Id.ToString(), Text = $"{e.Name} - {e.Description}" }), "Value", "Text");
            ExtraProperties["BikeRoutes"] = new SelectList( routesTask.Result?.Select( r => new SelectListItem{ Value = r.Id.ToString(), Text = $"{r.Name} - {r.Distance} miles" }), "Value", "Text");            
        }

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
