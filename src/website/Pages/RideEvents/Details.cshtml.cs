using CCC.Entities;
using CCC.Authorization;
using CCC.website.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Abstractions;

namespace CCC.website.Pages.RideEvents
{
    public class DetailsPageModel : PageModelBase
    {
        [BindProperty(SupportsGet = true)]
        public Guid Id { get; set; }


        public DetailsPageModel(ILogger<PageModelBase> logger, IDownstreamApi api) : base(logger, api)
        {

        }

        public IActionResult OnGet()
        {
            return RedirectToPage("/EventDetails", new {id = Id});
        }
    }
}