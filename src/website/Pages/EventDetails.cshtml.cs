
using System.ComponentModel;
using CCC.ViewModels;
using CCC.website.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Abstractions;

using CCC.Authorization;
using CCC.Entities;

namespace CCC.website.Pages;

public class EventDetailsPageModel : PageModelBase
{
    [BindProperty(SupportsGet =true)]
    public Guid Id {get; set;}

    public RideEventViewModel RideEvent { get; set;} = new();
    public Dictionary<string, object> ExtraData {get;set;} = new();


    public EventDetailsPageModel(ILogger<PageModelBase> logger, IDownstreamApi api) : base(logger, api)
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
}