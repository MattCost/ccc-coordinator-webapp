
using CCC.Entities;
using CCC.website.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Abstractions;
using Microsoft.Identity.Web;

namespace CCC.website.Pages.BikeRoutes
{
    // [Microsoft.Identity.Web.Resource.RequiredScope(new string[] { "API.Access" })]
    
    [AuthorizeForScopes(Scopes = new string[] {"https://cccwebapp.onmicrosoft.com/ccc-webapp-api/API.Access"})]
    public class IndexPageModel : PageModelBase
    {
        private ITokenAcquisition _tokenAcquisition;
        public IndexPageModel(ILogger<IndexPageModel> logger, IDownstreamApi api, ITokenAcquisition tokenAcquisition) : base(logger, api)
        {
            _tokenAcquisition = tokenAcquisition;
        }

        public List<BikeRoute> Routes { get; set; } = new ();

        public async Task OnGetAsync()
        {
            try
            {
                var result = await API.GetForUserAsync<List<BikeRoute>>("API", options =>
                {
                    options.RelativePath = "BikeRoutes/all";
                });
                Logger.LogDebug("Result from API {Result}", result);
                Routes = result ?? new();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Exception trying to get bike routes!");
                CurrentPageAction = "OnGetAsync";
                CurrentPageErrorMessage = ex.Message;
            }
        }
    }
}
