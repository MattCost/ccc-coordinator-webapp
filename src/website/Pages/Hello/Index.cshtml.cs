using CCC.website.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Identity.Abstractions;

namespace CCC.website.Pages.Hello
{
    public class IndexPageModel : PageModelBase
    {
        public string APIStatus { get; private set; } = "unknown";
        public string DisplayName { get; set; } = string.Empty;
        private class APIMessage
        {
            public string Message { get; set; } = "unknown";
        }
        public IndexPageModel(ILogger<IndexPageModel> logger, IDownstreamApi api) : base(logger, api)
        {
        }

        public async Task OnGetAsync()
        {
            DisplayName = HttpContext.User.Claims.Where(claim => claim.Type == "name").First().Value;
            try
            {
                var result = await API.GetForUserAsync<APIMessage>("API", options =>
                {
                    options.RelativePath = "HelloWorld";
                });
                if(result != null)
                {
                    Logger.LogDebug("Result from API {Result}", result);
                    if (result.Message == "Hello World")
                    {
                        APIStatus = "Healthy";
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Exception!!!");
            }
        }
    }
}
