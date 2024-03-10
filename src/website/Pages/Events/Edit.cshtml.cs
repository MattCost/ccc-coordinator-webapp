
using System.ComponentModel;
using CCC.ViewModels;
using CCC.website.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Abstractions;

using CCC.Authorization;
using CCC.Entities;
using System.ComponentModel.DataAnnotations;


namespace CCC.website.Pages.Events;

public class EditPageModel : PageModelBase
{
    [BindProperty(SupportsGet =true)]
    public Guid Id {get; set;}

    public RideEventViewModel RideEvent { get; set;} = new();
    public Dictionary<string, object> ExtraData {get;set;} = new();


    [BindProperty]
    public RideEventUpdateModel EventUpdateModel {get;set;} = new();

    [BindProperty, DataType(DataType.Date)]
    public DateTime EventDate { get; set; }
    
    [BindProperty, DataType(DataType.Time)]
    public TimeSpan EventTime { get; set; }        


    public EditPageModel(ILogger<PageModelBase> logger, IDownstreamApi api) : base(logger, api)
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
            EventUpdateModel = new RideEventUpdateModel
            {
                Name = RideEvent.RideEvent.Name,
                Description = RideEvent.RideEvent.Description,
                StartTime = RideEvent.RideEvent.StartTime,
                Location = RideEvent.RideEvent.Location,
            };
            EventDate = RideEvent.RideEvent.StartTime.Date;
            EventTime = RideEvent.RideEvent.StartTime.TimeOfDay;

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

        public async Task<IActionResult> OnPostDeleteEventAsync()
        {
            await API.DeleteForUserAsync("API", string.Empty, options =>
            {
                options.RelativePath = $"RideEvents/{Id}";
            });
            return RedirectToPage("Index");

        }
        public async Task<IActionResult> OnPostUpdateEventDetailsAsync()
        {
            Logger.LogTrace("Entering OnPostUpdateEventDetails");
            EventUpdateModel.StartTime = EventDate.Add(EventTime);

            await API.PatchForUserAsync("API", EventUpdateModel, options =>
            {
                options.RelativePath = $"RideEvents/{Id}";
            });
            Logger.LogDebug("Patch Complete");
            return RedirectToPage();
        }
}