using ExpensesApp.Data;
using ExpensesApp.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ExpensesApp.Pages.Expenses;

public class PendingModel : PageModel
{
    private readonly ExpenseRepository _repo;
    public PendingModel(ExpenseRepository repo) => _repo = repo;

    public List<Expense> Expenses { get; set; } = new();
    public string? DbError { get; set; }

    public async Task OnGetAsync()
    {
        var (expenses, err) = await _repo.GetPendingExpensesAsync();
        Expenses = expenses;
        DbError  = err;
    }
}
