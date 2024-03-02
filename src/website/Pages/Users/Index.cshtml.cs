
using CCC.Entities;
using CCC.Authorization;
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
    }
}