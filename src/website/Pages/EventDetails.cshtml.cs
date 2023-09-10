
using System.ComponentModel;
using CCC.Common.ViewModels;
using CCC.website.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Abstractions;

using CCC.Common.Authorization;
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

    public async Task OnGetAsync()
    {
        ExtraData["ShowSignupButton"] = User.IsCoordinator();

        try
        {
            var result = await API.GetForUserAsync<RideEventViewModel>("API", options =>
            {
                options.RelativePath = $"ViewModels/RideEvents/{Id}";
            });
            Logger.LogDebug("Result from API {Result}", result);
            RideEvent = result ?? new RideEventViewModel();
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