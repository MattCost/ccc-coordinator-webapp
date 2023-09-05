using CCC.Entities;
using CCC.Common.Authorization;
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

        public Dictionary<string, object> ExtraData {get;set;} = new();

        public DetailsPageModel(ILogger<PageModelBase> logger, IDownstreamApi api) : base(logger, api)
        {
        }
        public async Task OnGetAsync()
        {
            Logger.LogTrace("Entering OnGetAsync Id {Id}", Id);
            ExtraData["ShowSignupButton"] = User.IsCoordinator();
            
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

        public async Task<EmptyResult> OnPostSignupAsync(Guid rideId, CoordinatorRole role)
        {
            Logger.LogTrace("Entering OnPostSignupAsync. RideId: {RideId}. Role: {Role}", rideId, role);
            var userDisplayName = HttpContext.User.Claims.Where( claim => claim.Type == "name").First().Value;
            Logger.LogTrace("UserName: {UserName}", userDisplayName);

            await API.PatchForUserAsync("API", userDisplayName, options =>
            {
                options.RelativePath = $"GroupRides/{rideId}/coordinators/{role}";
            });
            Logger.LogTrace("API Call Complete");
            return new EmptyResult();
        }

    }
}