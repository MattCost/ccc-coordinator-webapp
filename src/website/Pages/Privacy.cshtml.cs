using CCC.website.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Identity.Abstractions;

namespace website.Pages;

public class PrivacyModel : PageModelBase
{
    public PrivacyModel(ILogger<PrivacyModel> logger, IDownstreamApi api) : base(logger, api)
    {
    }

    public void OnGet()
    {
    }
}

