using ExpenseApp.Models;
using ExpenseApp.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ExpenseApp.Pages;

public class IndexModel : PageModel
{
    private readonly IDatabaseService _db;
    private readonly ILogger<IndexModel> _logger;

    public int TotalExpenses { get; private set; }
    public int PendingCount  { get; private set; }
    public int ApprovedCount { get; private set; }
    public int TotalUsers    { get; private set; }

    public IndexModel(IDatabaseService db, ILogger<IndexModel> logger)
    {
        _db     = db;
        _logger = logger;
    }

    public async Task OnGetAsync()
    {
        try
        {
            var expenses = await _db.GetExpensesAsync();
            var users    = await _db.GetUsersAsync();

            TotalExpenses = expenses.Count;
            PendingCount  = expenses.Count(e => e.StatusName == "Submitted");
            ApprovedCount = expenses.Count(e => e.StatusName == "Approved");
            TotalUsers    = users.Count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Dashboard: failed to load data from database");
            ViewData["DbError"] = BuildErrorMessage(ex);

            // Return sensible dummy stats so the page is still usable
            TotalExpenses = 4;
            PendingCount  = 1;
            ApprovedCount = 2;
            TotalUsers    = 2;
        }
    }

    internal static string BuildErrorMessage(Exception ex)
    {
        // Provide an actionable message without leaking code or stack traces
        if (ex.Message.Contains("Managed Identity") || ex.Message.Contains("AZURE_CLIENT_ID")
            || ex.Message.Contains("Active Directory"))
        {
            return "Managed Identity authentication failed. "
                 + "Ensure the App Service has a user-assigned managed identity and "
                 + "AZURE_CLIENT_ID is set to the identity's Client ID. "
                 + "For local dev, run 'az login' and use Authentication=Active Directory Default.";
        }
        if (ex.Message.Contains("Cannot open server") || ex.Message.Contains("network-related"))
        {
            return "Cannot reach the SQL Server. Check that the SQL_SERVER_FQDN app setting is correct "
                 + "and that the SQL firewall rule allows the App Service outbound IP.";
        }
        return $"Database error: {ex.Message.Split('\n')[0]}. Check the connection string in App Service Configuration.";
    }
}
