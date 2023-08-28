using CCC.website.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Identity.Abstractions;

namespace website.Pages;

public class IndexModel : PageModelBase
{
    public IndexModel(ILogger<IndexModel> logger, IDownstreamApi api) : base(logger, api)
    {
    }

    public void OnGet()
    {

    }
}
