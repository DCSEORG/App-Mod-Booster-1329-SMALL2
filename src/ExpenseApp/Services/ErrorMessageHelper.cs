using ExpenseApp.Models;
using Microsoft.Data.SqlClient;

namespace ExpenseApp.Services;

/// <summary>
/// Shared error message helper for all page models.
/// Produces user-actionable messages without leaking code or stack traces.
/// </summary>
public static class ErrorMessageHelper
{
    public static string BuildErrorMessage(Exception ex)
    {
        // Prefer checking SqlException properties over parsing message strings
        if (ex is SqlException sqlEx)
        {
            return sqlEx.Number switch
            {
                // Login failed / token auth failures
                18456 or 18452 =>
                    "Database authentication failed. Ensure the App Service has a user-assigned managed identity, "
                    + "AZURE_CLIENT_ID is set to the identity's Client ID, and the identity has been granted "
                    + "db_datareader/db_datawriter/EXECUTE permissions inside ExpenseDB. "
                    + "For local dev, run 'az login' and use Authentication=Active Directory Default.",
                // Network / firewall
                -2 or 10060 or 10061 =>
                    "Cannot reach the SQL Server. Check that SQL_SERVER_FQDN in App Service Configuration is correct "
                    + "and that the SQL firewall allows the App Service outbound IP.",
                _ => $"SQL error {sqlEx.Number}: {sqlEx.Message.Split('\n')[0]}. "
                   + "Check the DefaultConnection string in App Service Configuration."
            };
        }

        // Non-SQL exceptions – check for well-known patterns without relying on message text
        if (ex is InvalidOperationException && ex.Message.Contains("DefaultConnection"))
        {
            return "The DefaultConnection setting is missing in App Service Configuration. "
                 + "Run deploy-infra.sh to configure connection strings automatically.";
        }

        return $"Database error: {ex.GetType().Name}: {ex.Message.Split('\n')[0]}. "
             + "Check the connection string in App Service Configuration.";
    }
}
