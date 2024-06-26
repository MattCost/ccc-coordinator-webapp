
using CCC.Authorization;
using CCC.Entities;
using CCC.Services.UserProvider;
using CCC.website.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Abstractions;
using Microsoft.Identity.Web;

namespace CCC.website.Pages.Users
{

    [Authorize(Policy = CCC.Authorization.Enums.CoordinatorAdminPolicy)]
    public class IndexPageModel : PageModelBase
    {
        public bool ShowAdminSection => User.IsAdmin();
        public bool ShowCoordinatorAdminSection => User.IsCoordinatorAdmin();
        public IndexPageModel(ILogger<IndexPageModel> logger, IDownstreamApi api) : base(logger, api)
        {
        }

        public async Task<JsonResult> OnGetFetchAllUsersAsync()
        {
            Logger.LogTrace("Entering OnGetFetchAllUsersAsync");
            try
            {
                var allUsers = await API.GetForUserAsync<List<User>>("API", options =>
                {
                    options.RelativePath = "Users";
                }) ?? new List<User>();
                Logger.LogTrace("Get All Users complete. Returning {Count} users.", allUsers.Count);
                return new JsonResult(allUsers);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Exception trying to Fetch all users!");
                return new JsonResult(new { });
            }
        }

        public async Task<JsonResult> OnPostAssignCoordinatorAsync(string userId)
        {
            Logger.LogTrace("Entering OnPostAssignCoordinatorAsync User Id {UserId}", userId);
            try
            {
                await API.PatchForUserAsync("API", string.Empty, options =>
                {
                    options.RelativePath = $"Users/{userId}/coordinator";
                });
                Logger.LogTrace("Returning ok");
                return new JsonResult(new { message = "ok" });
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Exception trying to Assign Coordinator to user Id {UserId}", userId);
                return new JsonResult(new { message = "problem" });

            }
        }
        public async Task<JsonResult> OnPostRemoveCoordinatorAsync(string userId)
        {
            Logger.LogTrace("Entering OnPostRemoveCoordinator User Id {UserId}", userId);
            try
            {
                await API.DeleteForUserAsync("API", string.Empty, options =>
                {
                    options.RelativePath = $"Users/{userId}/coordinator";
                });
                Logger.LogTrace("Returning ok");
                return new JsonResult(new { message = "ok" });
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Exception trying to Remove Coordinator from user Id {UserId}", userId);
                return new JsonResult(new { message = "problem" });

            }
        }

        public async Task<JsonResult> OnPostAssignCoordinatorAdminAsync(string userId)
        {
            Logger.LogTrace("Entering OnPostAssignCoordinatorAdminAsync User Id {UserId}", userId);
            try
            {
                await API.PatchForUserAsync("API", string.Empty, options =>
                {
                    options.RelativePath = $"Users/{userId}/coordinatorAdmin";
                });
                Logger.LogTrace("Returning ok");
                return new JsonResult(new { message = "ok" });
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Exception trying to Assign Coordinator Admin to user Id {UserId}", userId);
                return new JsonResult(new { message = "problem" });

            }
        }

        public async Task<JsonResult> OnPostRemoveCoordinatorAdminAsync(string userId)
        {
            Logger.LogTrace("Entering OnPostRemoveCoordinatorAdminAsync User Id {UserId}", userId);
            try
            {
                await API.DeleteForUserAsync("API", string.Empty, options =>
                {
                    options.RelativePath = $"Users/{userId}/coordinatorAdmin";
                });
                Logger.LogTrace("Returning ok");
                return new JsonResult(new { message = "ok" });
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Exception trying to Remove Coordinator Admin from user Id {UserId}", userId);
                return new JsonResult(new { message = "problem" });
            }
        }

        public async Task<JsonResult> OnPostAssignContributorAsync(string userId)
        {
            Logger.LogTrace("Entering OnPostAssignContributorAsync User Id {UserId}", userId);
            try
            {
                await API.PatchForUserAsync("API", string.Empty, options =>
                {
                    options.RelativePath = $"Users/{userId}/contributor";
                });
                Logger.LogTrace("Returning ok");
                return new JsonResult(new { message = "ok" });
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Exception trying to Assign Contributor to user Id {UserId}", userId);
                return new JsonResult(new { message = "problem" });

            }
        }

        public async Task<JsonResult> OnPostRemoveContributorAsync(string userId)
        {
            Logger.LogTrace("Entering OnPostRemoveContributorAsync User Id {UserId}", userId);
            try
            {
                await API.DeleteForUserAsync("API", string.Empty, options =>
                {
                    options.RelativePath = $"Users/{userId}/contributor";
                });
                Logger.LogTrace("Returning ok");
                return new JsonResult(new { message = "ok" });
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Exception trying to Remove Contributor from user Id {UserId}", userId);
                return new JsonResult(new { message = "problem" });

            }
        }
    }
}