
using System.ComponentModel;
using CCC.ViewModels;
using CCC.website.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Abstractions;

using CCC.Authorization;
using CCC.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;
using CCC.Enums;

namespace CCC.website.Pages.Events;

public class DetailsPageModel : PageModelBase
{

    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    public RideEventViewModel RideEventVM { get; set; } = new();
    // public Dictionary<string, object> ExtraData { get; set; } = new();
    public bool IsSignedUp {get;set;}

    public List<User> AllCoordinators { get; set; } = new();


    [BindProperty]
    public List<SignupEntry> Signups { get; set; } = new();

    public SelectList CoordinatorSelectList { get; set; } = new SelectList(new List<SelectListItem>(), "Value", "Text");

    public DetailsPageModel(ILogger<PageModelBase> logger, IDownstreamApi api) : base(logger, api)
    {
    }

    public async Task OnGetAsync(string activeTab)
    {
        // ExtraData["UserIsCoordinator"] = User.IsCoordinator();
        // ExtraData["activeTab"] = string.IsNullOrEmpty(activeTab) ? "list-details" : activeTab;
        ViewData["activeTab"] = string.IsNullOrEmpty(activeTab) ? "list-details" : activeTab;

        try
        {
            var result = await API.GetForUserAsync<RideEventViewModel>("API", options =>
            {
                options.RelativePath = $"ViewModels/RideEvents/{Id}";
            });
            Logger.LogDebug("Result from API {Result}", result);
            RideEventVM = result ?? new RideEventViewModel();

            if (User.IsCoordinator())
            {
                var coordinatorSignedUp = RideEventVM.GroupRides.Select(ride => ride.Coordinators.Values.ToList()).SelectMany(x => x).Where(entry => entry.CoordinatorIds.Contains(User.NameIdentifier())).Any();
                var supportSignedUp = RideEventVM.RideEvent.SupportPersonnel.Select( entry => entry.Value).ToList().Where(entry => entry.CoordinatorIds.Contains(User.NameIdentifier())).Any();
                Logger.LogDebug("CoordinatorSignedUp: {coordinatorSignedUp}", coordinatorSignedUp);
                IsSignedUp = coordinatorSignedUp | supportSignedUp;
                ViewData["signedUp"] = IsSignedUp;
                ViewData["userDisplayNameLookup"] = RideEventVM.CoordinatorDisplayNames;

                AllCoordinators = await API.GetForUserAsync<List<User>>("API", options =>
                {
                    options.RelativePath = "Users/coordinators";
                }) ?? new();

                Signups = await API.GetForUserAsync<List<SignupEntry>>("API", options =>
                {
                    options.RelativePath = $"RideEvents/{Id}/signups";
                }) ?? new List<SignupEntry>();

                CoordinatorSelectList = new SelectList(AllCoordinators.Select(coordinator => new SelectListItem { Value = coordinator.UserId, Text = coordinator.DisplayName }).Append(new SelectListItem { Value = string.Empty, Text = "None" }), "Value", "Text");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Exception trying to get RideEventViewModel!");
            CurrentPageAction = "OnGetAsync";
            CurrentPageErrorMessage = ex.Message;
        }
    }


    public async Task<EmptyResult> OnPostSignupAsync(Guid entityId, CoordinatorRole role, EntityTypes entityType)
    {
        Logger.LogTrace("Entering OnPostSignupAsync. RideId: {RideId}. Role: {Role}. EntityType {EntityType}", entityId, role, entityType);
        var userIdentifier = User.NameIdentifier();
        Logger.LogTrace("User: {UserId}", userIdentifier);

        if(entityType == EntityTypes.GroupRide)
        {
            await API.PatchForUserAsync("API", userIdentifier, options =>
            {
                options.RelativePath = $"GroupRides/{entityId}/coordinators/{role}";
            });
            Logger.LogTrace("API to GroupRides controller Complete");
        }

        if(entityType == EntityTypes.RideEvent)
        {
            await API.PatchForUserAsync("API", userIdentifier, options =>
            {
                options.RelativePath = $"RideEvents/{entityId}/support/{role}";
            });
            Logger.LogTrace("API to RideEvents controller Complete");
        }        
        
        return new EmptyResult();
    }

    public async Task<EmptyResult> OnPostDropoutAsync(Guid entityId, CoordinatorRole role, EntityTypes entityType)
    {
        Logger.LogTrace("Entering OnPostDropoutAsync. RideId: {RideId}. Role: {Role}. EntityType {EntityType}", entityId, role, entityType);

        var userIdentifier = User.NameIdentifier();
        Logger.LogTrace("User: {UserId}", userIdentifier);

        if(entityType == EntityTypes.GroupRide)
        {
            await API.DeleteForUserAsync("API", userIdentifier, options =>
            {
                options.RelativePath = $"GroupRides/{entityId}/coordinators/{role}";
            });
            Logger.LogTrace("API Call Complete");
        }

        if(entityType == EntityTypes.RideEvent)
        {
            await API.DeleteForUserAsync("API", userIdentifier, options =>
            {
                options.RelativePath = $"RideEvents/{entityId}/support/{role}";
            });
            Logger.LogTrace("API Call Complete");
        }        
        return new EmptyResult();
    }

    public async Task<IActionResult> OnPostAdvancedSignupAsync()
    {

        Logger.LogTrace("Entering OnPostAdvancedSignup. Getting current state");

        var x = Signups.Where(signup => !string.IsNullOrEmpty(signup.UserId)).Select(signup => signup.UserId);
        if(x.Count() != x.Distinct().Count() )
        {
            Logger.LogError("Bad input, same coordinator can't do 2 roles");
            return RedirectToPage();
        }

        var result = await API.GetForUserAsync<RideEventViewModel>("API", options =>
        {
            options.RelativePath = $"ViewModels/RideEvents/{Id}";
        });
        Logger.LogDebug("Result from API {Result}", result);
        RideEventVM = result ?? new RideEventViewModel();

        Logger.LogTrace("Processing Support signups");
        foreach(var supportRole in RideEventVM.RideEvent.SupportPersonnel.Keys)
        {
            var currentCoordinators = RideEventVM.RideEvent.SupportPersonnel[supportRole].CoordinatorIds;
            var requestCoordinators =
                Signups.Where(signup => !string.IsNullOrEmpty(signup.UserId))
                    .Where(signup =>  signup.EntityType == Enums.EntityTypes.RideEvent && signup.EntityId == RideEventVM.RideEvent.Id && signup.CoordinatorRole == supportRole)
                    .Select(signup => signup.UserId);
            var toRemove = currentCoordinators.Where(coordinatorId => !requestCoordinators.Contains(coordinatorId));
            var toAdd = requestCoordinators.Where(coordinatorId => !currentCoordinators.Contains(coordinatorId));
        
            if (toRemove.Any())
            {
                Logger.LogTrace("We have {Count} signups to remove for {Role} role in Event {Id}", toRemove.Count(), supportRole, RideEventVM.RideEvent.Id);
            }

            if (toAdd.Any())
            {
                Logger.LogTrace("We have {Count} signups to add for {Role} role in Event {Id}", toAdd.Count(), supportRole, RideEventVM.RideEvent.Id);
            }
                
        
            foreach (var userId in toRemove)
            {
                Logger.LogTrace("Removing userId {UserId} from Role {Role} in Event {Id}", userId, supportRole, RideEventVM.RideEvent.Id);
                await API.DeleteForUserAsync("API", userId, options =>
                {
                    options.RelativePath = $"RideEvents/{RideEventVM.RideEvent.Id}/support/{supportRole}";
                });
            }

            foreach (var userId in toAdd)
            {
                Logger.LogTrace("Adding userId {UserId} from Role {Role} in Event {Id}", userId, supportRole, RideEventVM.RideEvent.Id);
                await API.PatchForUserAsync("API", userId, options =>
                {
                    options.RelativePath = $"RideEvents/{RideEventVM.RideEvent.Id}/support/{supportRole}";
                });
            }
        }

        Logger.LogTrace("Processing Coordinator signups");

        // for each ride
        foreach (var ride in RideEventVM.GroupRides)
        {
            // for each role
            foreach (var role in ride.Coordinators.Keys)
            {
                var currentCoordinators = ride.Coordinators[role].CoordinatorIds;
                var requestCoordinators =
                    Signups.Where(signup => !string.IsNullOrEmpty(signup.UserId))
                        .Where(signup =>  signup.EntityType == Enums.EntityTypes.GroupRide && signup.EntityId == ride.Id && signup.CoordinatorRole == role)
                        .Select(signup => signup.UserId);

                // if a user is in current state, but not requested state remove
                var toRemove = currentCoordinators.Where(coordinatorId => !requestCoordinators.Contains(coordinatorId));
                
                // if a user is in request state, but not in current state add
                var toAdd = requestCoordinators.Where(coordinatorId => !currentCoordinators.Contains(coordinatorId));

                if (toRemove.Any())
                {
                    Logger.LogTrace("We have {Count} signups to remove for {Role} role in Ride {RideId}", toRemove.Count(), role, ride.Id);
                }

                if (toAdd.Any())
                {
                    Logger.LogTrace("We have {Count} signups to add for {Role} role in Ride {RideId}", toAdd.Count(), role, ride.Id);
                }
                
                // todo - make api return bad request if a user tries to double book for a ride?
                // todo - do all removes across all signups first, then do all adds.

                foreach (var userId in toRemove)
                {
                    Logger.LogTrace("Removing userId {UserId} from Role {Role} in Ride {Ride}", userId, role, ride.Id);
                    await API.DeleteForUserAsync("API", userId, options =>
                    {
                        options.RelativePath = $"GroupRides/{ride.Id}/coordinators/{role}";
                    });
                }
                foreach (var userId in toAdd)
                {
                    Logger.LogTrace("Adding userId {UserId} from Role {Role} in Ride {Ride}", userId, role, ride.Id);

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
