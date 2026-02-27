using ExpensesApp.Data;
using ExpensesApp.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ExpensesApp.Pages.Expenses;

public class IndexModel : PageModel
{
    private readonly ExpenseRepository _repo;
    public IndexModel(ExpenseRepository repo) => _repo = repo;

    public List<Expense> Expenses { get; set; } = new();
    public string? DbError { get; set; }
    public string? CurrentStatus { get; set; }

    public async Task OnGetAsync(string? status)
    {
        CurrentStatus = status;
        if (!string.IsNullOrEmpty(status))
        {
            var (expenses, err) = await _repo.GetExpensesByStatusAsync(status);
            Expenses = expenses;
            DbError  = err;
        }
        else
        {
            var (expenses, err) = await _repo.GetAllExpensesAsync();
            Expenses = expenses;
            DbError  = err;
        }
    }
}
