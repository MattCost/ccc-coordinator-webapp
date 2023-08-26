using Microsoft.Identity.Abstractions;

namespace CCC.website.Models;

public abstract class PageModelBase
{
    protected ILogger Logger {get; private set;}
    protected IDownstreamApi API {get; private set;}

    public PageModelBase(ILogger<PageModelBase> logger, IDownstreamApi api)
    {
        Logger = logger;
        API = api;        
    }
}