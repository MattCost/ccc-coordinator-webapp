using Microsoft.Identity.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CCC.website.Models;

public abstract class PageModelBase : PageModel
{
    protected ILogger Logger { get; private set; }
    protected IDownstreamApi API { get; private set; }

    public PageModelBase(ILogger<PageModelBase> logger, IDownstreamApi api)
    {
        Logger = logger;
        API = api;
    }

    /// <summary>
    /// An error message from a previous page that redirects. 
    /// For example, in an OnPostHandler one can set this string, then redirect to a Get page, and display the error then
    /// </summary>
    [TempData]
    public string? PreviousPageErrorMessage { get; set; }

    /// <summary>
    /// The Action that caused the previous error message
    /// </summary>
    [TempData]
    public string? PreviousPageAction { get; set; }
    
    /// <summary>
    /// An error message from the current page. Not saved in temp data, so it can only be displayed on the current page
    /// </summary>
    public string? CurrentPageErrorMessage { get; set; }
    /// <summary>
    /// The Action that caused the Current Error Message
    /// </summary>
    public string? CurrentPageAction { get; set; }

    public bool DisplayCurrentError => !string.IsNullOrEmpty(CurrentPageErrorMessage);
    
    public bool DisplayPreviousError => !string.IsNullOrEmpty(PreviousPageErrorMessage);

}