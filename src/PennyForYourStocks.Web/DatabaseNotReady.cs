using Microsoft.Data.Sqlite;

namespace PennyForYourStocks.Web;

internal static class DatabaseNotReady
{
    internal const string UserMessage =
        "The sign-in database is still being updated. Restart the AppHost and wait for the triallog-migrations resource to finish, then try again.";

    internal static bool IsMatch(Exception exception)
    {
        for (var current = exception; current is not null; current = current.InnerException)
        {
            if (current is SqliteException sqlite && sqlite.SqliteErrorCode == 1)
            {
                return sqlite.Message.Contains("no such table", StringComparison.OrdinalIgnoreCase)
                    || sqlite.Message.Contains("no such column", StringComparison.OrdinalIgnoreCase);
            }
        }

        return false;
    }
}
