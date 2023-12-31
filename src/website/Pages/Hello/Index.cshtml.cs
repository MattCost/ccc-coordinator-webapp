using CCC.website.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Identity.Abstractions;

namespace CCC.website.Pages.Hello
{
    public class IndexPageModel : PageModelBase
    {
        public string APIStatus { get; private set; } = "unknown";
        public string DisplayName {get;set;} = string.Empty;
        public IndexPageModel(ILogger<IndexPageModel> logger, IDownstreamApi api) : base(logger, api)
        {
        }

        public async Task OnGetAsync()
        {
            DisplayName = HttpContext.User.Claims.Where( claim => claim.Type == "name").First().Value;
            try
            {
                var result = await API.GetForUserAsync<string>("API", options =>
                {
                    options.RelativePath = "HelloWorld";
                });
                Logger.LogDebug("Result from API {Result}", result);
                APIStatus = string.IsNullOrEmpty(result) ? "empty!" : result;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Exception!!!");
            }
        }
    }
}
