using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using System.Net;

/// <summary>
/// The web helper.
/// </summary>
public static class WebHelper
{
    /// <summary>
    /// Performs a response redirect using string interpolation/format.
    /// Note: In modern .NET, redirects are usually handled by returning a RedirectResult 
    /// from a controller, but this extension works for middleware or direct context access.
    /// </summary>
    public static void Redirect(this HttpResponse response, string urlFormat, params object[] values)
    {
        var url = string.Format(urlFormat, values);
        // Defaulting to a 302 Found redirect
        response.Redirect(url);
    }

    /// <summary>
    /// Reloads the current page by redirecting to the current Display URL.
    /// </summary>
    public static void Reload(this HttpResponse response)
    {
        // GetDisplayUrl() is the modern replacement for Request.Url.ToString()
        var currentUrl = response.HttpContext.Request.GetDisplayUrl();
        response.Redirect(currentUrl);
    }

    /// <summary>
    /// Sets a 404 Not Found status.
    /// </summary>
    public static void SetFileNotFound(this HttpResponse response)
    {
        response.SetStatus((int)HttpStatusCode.NotFound);
    }

    /// <summary>
    /// Sets a 500 Internal Server Error status.
    /// </summary>
    public static void SetInternalServerError(this HttpResponse response)
    {
        response.SetStatus((int)HttpStatusCode.InternalServerError);
    }

    /// <summary>
    /// Modern Status setter. In .NET Core/10, 'StatusDescription' is not a property 
    /// of HttpResponse. Reason phrases are handled by the web server (Kestrel/IIS).
    /// </summary>
    public static void SetStatus(this HttpResponse response, int code)
    {
        response.StatusCode = code;

        // Note: response.End() is deprecated. In modern .NET, you simply 
        // stop processing or return from your middleware/action.
    }
}