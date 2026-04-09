using Microsoft.AspNetCore.Http;
using System.Text.Encodings.Web;

/// <summary>
/// Provides high-visibility extensions for <see cref="HttpResponse"/> to handle 
/// complex redirection scenarios, including client-side window management.
/// </summary>
public static class ResponseHelper
{
    /// <summary>
    /// Modern Redirect. 
    /// If a target or window features are provided, it writes a JS block to the response.
    /// Otherwise, it performs a standard 302 Redirect.
    /// </summary>
    public static async Task RedirectAsync(this HttpResponse response, string url, string? target = "_self", string? windowFeatures = null)
    {
        bool isStandardRedirect = (string.IsNullOrEmpty(target) ||
                                   target.Equals("_self", StringComparison.OrdinalIgnoreCase)) &&
                                   string.IsNullOrEmpty(windowFeatures);

        if (isStandardRedirect)
        {
            response.Redirect(url);
            return;
        }

        // High-Visibility: We are manually injecting the script because ScriptManager is gone.
        // We use JavaScriptEncoder to prevent XSS attacks via the URL or target name.
        var encodedUrl = JavaScriptEncoder.Default.Encode(url);
        var encodedTarget = JavaScriptEncoder.Default.Encode(target ?? "_blank");

        string script;
        if (!string.IsNullOrEmpty(windowFeatures))
        {
            var encodedFeatures = JavaScriptEncoder.Default.Encode(windowFeatures);
            script = $"<script>window.open(\"{encodedUrl}\", \"{encodedTarget}\", \"{encodedFeatures}\");</script>";
        }
        else
        {
            script = $"<script>window.open(\"{encodedUrl}\", \"{encodedTarget}\");</script>";
        }

        response.ContentType = "text/html";
        await response.WriteAsync(script);
    }
}