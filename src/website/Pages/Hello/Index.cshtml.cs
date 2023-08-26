using CCC.website.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Identity.Abstractions;

namespace CCC.website.Pages.Hello
{
    public class IndexModel : PageModelBase
    {
        public string APIStatus { get; private set; } = "unknown";
        public IndexModel(ILogger<PageModelBase> logger, IDownstreamApi api) : base(logger, api)
        {
        }

        public async Task OnGet()
        {
            Logger.LogDebug("Entering OnGet");
            try
            {
                var result = await API.GetForUserAsync<string>("API", options =>
                {
                    options.RelativePath = "/api/HelloWorld";
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
