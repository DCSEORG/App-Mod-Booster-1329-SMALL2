using ExpenseApp.Models;
using ExpenseApp.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ExpenseApp.Pages.Expenses;

public class IndexModel : PageModel
{
    private readonly IDatabaseService _db;
    private readonly ILogger<IndexModel> _logger;

    public List<Expense>        Expenses     { get; private set; } = new();
    public List<ExpenseStatus>  Statuses     { get; private set; } = new();
    public List<User>           Users        { get; private set; } = new();
    [BindProperty(SupportsGet = true)] public string? StatusFilter { get; set; }
    [BindProperty(SupportsGet = true)] public int     UserFilter   { get; set; }

    public IndexModel(IDatabaseService db, ILogger<IndexModel> logger)
    {
        _db     = db;
        _logger = logger;
    }

    public async Task OnGetAsync()
    {
        try
        {
            Statuses = await _db.GetExpenseStatusesAsync();
            Users    = await _db.GetUsersAsync();

            if (UserFilter > 0)
                Expenses = await _db.GetExpensesByUserAsync(UserFilter);
            else if (!string.IsNullOrWhiteSpace(StatusFilter))
                Expenses = await _db.GetExpensesByStatusAsync(StatusFilter);
            else
                Expenses = await _db.GetExpensesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Expenses/Index: failed to load data");
            ViewData["DbError"] = IndexModel_Pages.BuildErrorMessage(ex);
            Expenses = DummyData.GetExpenses();
            Statuses = DummyData.GetStatuses();
            Users    = DummyData.GetUsers();
        }
    }
}

internal static class IndexModel_Pages
{
    public static string BuildErrorMessage(Exception ex)
        => ExpenseApp.Pages.IndexModel.BuildErrorMessage(ex);
}
