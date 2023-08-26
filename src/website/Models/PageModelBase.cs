using Microsoft.Identity.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CCC.website.Models;

public abstract class PageModelBase : PageModel
{
    protected ILogger Logger {get; private set;}
    protected IDownstreamApi API {get; private set;}

    public PageModelBase(ILogger<PageModelBase> logger, IDownstreamApi api)
    {
        Logger = logger;
        API = api;        
    }
}