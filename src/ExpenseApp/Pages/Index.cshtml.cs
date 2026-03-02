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
            ViewData["DbError"] = ErrorMessageHelper.BuildErrorMessage(ex);

            // Return sensible dummy stats so the page is still usable
            TotalExpenses = 4;
            PendingCount  = 1;
            ApprovedCount = 2;
            TotalUsers    = 2;
        }
    }
}
