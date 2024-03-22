
using System.ComponentModel;
using CCC.ViewModels;
using CCC.website.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Abstractions;

using CCC.Authorization;
using CCC.Entities;

namespace CCC.website.Pages.Events;

public class DetailsPageModel : PageModelBase
{

    public class AdvancedSignup
    {
        public string UserId {get;set;} = string.Empty;
        public Guid RideId {get;set;}
        public CoordinatorRole CoordinatorRole {get;set;}
    }

    [BindProperty(SupportsGet =true)]
    public Guid Id {get; set;}

    public RideEventViewModel RideEvent { get; set;} = new();
    public Dictionary<string, object> ExtraData {get;set;} = new();

    //add list for all coordinators, allow any coordinator to add any other coordinator for any role (to play musical chairs)
    public List<User> AllCoordinators {get;set;} = new();

    // Need a select list for allCoordinators, userId and displayName

    public DetailsPageModel(ILogger<PageModelBase> logger, IDownstreamApi api) : base(logger, api)
    {
    }

    public async Task OnGetAsync(string activeTab)
    {
        // ExtraData["UserIsCoordinator"] = User.IsCoordinator();
        ExtraData["activeTab"] = string.IsNullOrEmpty(activeTab) ? "list-details" : activeTab;

        try
        {
            var result = await API.GetForUserAsync<RideEventViewModel>("API", options =>
            {
                options.RelativePath = $"ViewModels/RideEvents/{Id}";
            });
            Logger.LogDebug("Result from API {Result}", result);
            RideEvent = result ?? new RideEventViewModel();

            if(User.IsCoordinator())
            {
                var coordinatorSignedUp  = RideEvent.GroupRides.Select( ride => ride.Coordinators.Values.ToList()).SelectMany( x=>x).Where( entry => entry.CoordinatorIds.Contains(User.NameIdentifier())).Any();
                Logger.LogDebug("CoordinatorSignedUp: {coordinatorSignedUp}", coordinatorSignedUp);
                ExtraData["signedUp"] = coordinatorSignedUp;
                ExtraData["userDisplayNameLookup"] = RideEvent.CoordinatorDisplayNames;

                AllCoordinators = await API.GetForUserAsync<List<User>>("API", options =>
                {
                    options.RelativePath = "Users/coordinators";
                }) ?? new();
            }

            
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Exception trying to get RideEventViewModel!");
            CurrentPageAction = "OnGetAsync";
            CurrentPageErrorMessage = ex.Message;
        }
    }


        public async Task<EmptyResult> OnPostSignupAsync(Guid rideId, CoordinatorRole role)
        {
            Logger.LogTrace("Entering OnPostSignupAsync. RideId: {RideId}. Role: {Role}", rideId, role);
            var userIdentifier = User.NameIdentifier();
            Logger.LogTrace("User: {UserId}", userIdentifier);

            await API.PatchForUserAsync("API", userIdentifier, options =>
            {
                options.RelativePath = $"GroupRides/{rideId}/coordinators/{role}";
            });
            Logger.LogTrace("API Call Complete");
            return new EmptyResult();
        }

        public async Task<EmptyResult> OnPostDropoutAsync(Guid rideId, CoordinatorRole role)
        {
            Logger.LogTrace("Entering OnPostSignupAsync. RideId: {RideId}. Role: {Role}", rideId, role);
            var userIdentifier = User.NameIdentifier();
            Logger.LogTrace("User: {UserId}", userIdentifier);

            await API.DeleteForUserAsync("API", userIdentifier, options =>
            {
                options.RelativePath = $"GroupRides/{rideId}/coordinators/{role}";
            });
            Logger.LogTrace("API Call Complete");
            return new EmptyResult();
        }        

        public async Task<IActionResult> OnPostAdvancedSignupAsync(List<AdvancedSignup> advancedSignups)
        {

            Logger.LogTrace("Entering OnPostAdvancedSignup. Getting current state");

            var result = await API.GetForUserAsync<RideEventViewModel>("API", options =>
            {
                options.RelativePath = $"ViewModels/RideEvents/{Id}";
            });
            Logger.LogDebug("Result from API {Result}", result);
            RideEvent = result ?? new RideEventViewModel();

            Logger.LogTrace("Comparing Current signups to requested signups");
            // for each ride
            foreach(var ride in RideEvent.GroupRides)
            {
                // for each role
                foreach(var role in ride.Coordinators.Keys)
                {
                    var currentCoordinators = ride.Coordinators[role].CoordinatorIds;
                    var requestCoordinators = advancedSignups.Where( signup => signup.RideId == ride.Id && signup.CoordinatorRole == role).Select( signup => signup.UserId);

                    // if a user is in current state, but not requested state remove
                    var toRemove = currentCoordinators.Where( coordinatorId => !requestCoordinators.Contains(coordinatorId) );
                    // if a user is in request state, but not in current state add

                    var toAdd = requestCoordinators.Where( coordinatorId => !currentCoordinators.Contains(coordinatorId));

                    if(toRemove.Any())
                    {
                        Logger.LogTrace("We have {Count} signups to remove for {Role} role in Ride {RideId}", toRemove.Count(), role, ride.Id);
                    }

                    if(toAdd.Any())
                    {
                        Logger.LogTrace("We have {Count} signups to add for {Role} role in Ride {RideId}", toAdd.Count(), role, ride.Id);
                    }

                    foreach(var userId in toRemove)
                    {
                        await API.DeleteForUserAsync("API", userId, options => 
                        {
                            options.RelativePath = $"GroupRides/{ride.Id}/coordinators/{role}";
                        });
                    }
                    foreach(var userId in toAdd)
                    {
                        await API.PatchForUserAsync("API", userId, options => 
                        {
                            options.RelativePath = $"GroupRides/{ride.Id}/coordinators/{role}";
                        });
                    }                    
                }
            }

            Logger.LogTrace("AdvancedSignup Complete");
            return RedirectToPage();
        }

}
