using CCC.Entities;
using CCC.website.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Abstractions;

namespace CCC.website.Pages.RideEvents
{
    public class DetailsPageModel : PageModelBase
    {
        [BindProperty(SupportsGet = true)]
        public Guid Id { get; set; }

        public RideEvent RideEvent { get; set; } = new();

        public List<GroupRide> Rides {get;set;} = new();
        public DetailsPageModel(ILogger<PageModelBase> logger, IDownstreamApi api) : base(logger, api)
        {
        }
        public async Task OnGetAsync()
        {
            Logger.LogTrace("Entering OnGetAsync Id {Id}", Id);
            try
            {
                RideEvent = await API.GetForUserAsync<RideEvent>("API", options =>
                {
                    options.RelativePath = $"RideEvents/{Id}";
                }) ?? new();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error getting entity Id {id}", Id);
                CurrentPageErrorMessage = $"Unable to get RideEvent";
                CurrentPageAction = "RideEvent/Details/OnGet";
            }
            Logger.LogTrace("Fetching child rides");
            foreach(var rideId in RideEvent.Rides)
            {
                var ride = await API.GetForUserAsync<GroupRide>("API", options => { options.RelativePath = $"GroupRides/{rideId}";});
                if(ride != null)
                {
                    Rides.Add(ride);
                }
            }
        }
    }
}