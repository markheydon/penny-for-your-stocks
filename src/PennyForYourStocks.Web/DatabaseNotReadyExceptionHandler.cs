using Microsoft.AspNetCore.Diagnostics;

namespace PennyForYourStocks.Web;

internal sealed class DatabaseNotReadyExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (!DatabaseNotReady.IsMatch(exception))
        {
            return false;
        }

        httpContext.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
        httpContext.Response.ContentType = "text/html; charset=utf-8";

        await httpContext.Response.WriteAsync(
            """
            <!DOCTYPE html>
            <html lang="en">
            <head>
              <meta charset="utf-8" />
              <meta name="viewport" content="width=device-width, initial-scale=1.0" />
              <title>Database update required</title>
              <style>
                body { font-family: system-ui, sans-serif; max-width: 32rem; margin: 3rem auto; padding: 0 1rem; line-height: 1.5; color: #1e293b; }
                h1 { font-size: 1.25rem; margin-bottom: 0.5rem; }
                p { margin: 0.75rem 0; }
              </style>
            </head>
            <body>
              <h1>Database update in progress</h1>
              <p>The trial log database schema is not ready yet.</p>
              <p>Restart the AppHost and wait until the <strong>triallog-migrations</strong> resource has completed, then reload this page.</p>
              <p>If you run locally without Aspire, apply pending EF migrations to the triallog database and try again.</p>
            </body>
            </html>
            """,
            cancellationToken);

        return true;
    }
}
