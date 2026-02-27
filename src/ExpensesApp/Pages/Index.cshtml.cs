using ExpensesApp.Data;
using ExpensesApp.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ExpensesApp.Pages;

public class IndexModel : PageModel
{
    private readonly ExpenseRepository _repo;

    public IndexModel(ExpenseRepository repo) => _repo = repo;

    public List<Expense> RecentExpenses { get; set; } = new();
    public List<ExpenseSummary> CategorySummary { get; set; } = new();
    public int TotalExpenses { get; set; }
    public int PendingCount { get; set; }
    public decimal TotalApprovedGBP { get; set; }
    public int UserCount { get; set; }
    public string? DbError { get; set; }

    public async Task OnGetAsync()
    {
        var (expenses, expError) = await _repo.GetAllExpensesAsync();
        DbError = expError;

        RecentExpenses   = expenses.Take(5).ToList();
        TotalExpenses    = expenses.Count;
        PendingCount     = expenses.Count(e => e.StatusName == "Submitted");
        TotalApprovedGBP = expenses.Where(e => e.StatusName == "Approved").Sum(e => e.AmountGBP);

        var (summary, _) = await _repo.GetSummaryByCategoryAsync();
        CategorySummary = summary;

        var (users, _) = await _repo.GetAllUsersAsync();
        UserCount = users.Count(u => u.IsActive);
    }
}
