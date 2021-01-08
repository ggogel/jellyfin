using System;
using System.Net.Mime;
using System.Threading.Tasks;
using MediaBrowser.Controller;
using MediaBrowser.Model.Globalization;
using Microsoft.AspNetCore.Http;

namespace Jellyfin.Server.Middleware
{
    /// <summary>
    /// Shows a custom message during server startup.
    /// </summary>
    public class ServerStartupMessageMiddleware
    {
        private readonly RequestDelegate _next;

        /// <summary>
        /// Initializes a new instance of the <see cref="ServerStartupMessageMiddleware"/> class.
        /// </summary>
        /// <param name="next">The next delegate in the pipeline.</param>
        public ServerStartupMessageMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        /// <summary>
        /// Executes the middleware action.
        /// </summary>
        /// <param name="httpContext">The current HTTP context.</param>
        /// <param name="serverApplicationHost">The server application host.</param>
        /// <param name="localizationManager">The localization manager.</param>
        /// <returns>The async task.</returns>
        public async Task Invoke(
            HttpContext httpContext,
            IServerApplicationHost serverApplicationHost,
            ILocalizationManager localizationManager)
        {
            if (serverApplicationHost.CoreStartupHasCompleted
                || httpContext.Request.Path.Equals("/system/ping", StringComparison.OrdinalIgnoreCase))
            {
                if (httpContext.Request.Path.Equals("/system/headers", StringComparison.OrdinalIgnoreCase))
                {
                    string headers = string.Empty;
                    foreach (var key in httpContext.Request.Headers.Keys)
                    {
                        headers += key + "=" + httpContext.Request.Headers[key] + Environment.NewLine;
                    }

                    httpContext.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
                    httpContext.Response.ContentType = MediaTypeNames.Text.Html;
                    await httpContext.Response.WriteAsync(headers, httpContext.RequestAborted).ConfigureAwait(false);
                    return;
                }

                await _next(httpContext).ConfigureAwait(false);
                return;
            }

            var message = localizationManager.GetLocalizedString("StartupEmbyServerIsLoading");
            httpContext.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
            httpContext.Response.ContentType = MediaTypeNames.Text.Html;
            await httpContext.Response.WriteAsync(message, httpContext.RequestAborted).ConfigureAwait(false);
        }
    }
}
