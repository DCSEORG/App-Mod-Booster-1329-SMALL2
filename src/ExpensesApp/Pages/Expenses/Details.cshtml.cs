using ExpensesApp.Data;
using ExpensesApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ExpensesApp.Pages.Expenses;

public class DetailsModel : PageModel
{
    private readonly ExpenseRepository _repo;
    public DetailsModel(ExpenseRepository repo) => _repo = repo;

    public Expense? Expense { get; set; }
    public List<User> Managers { get; set; } = new();
    public string? DbError { get; set; }
    public string? ActionMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var (expense, err) = await _repo.GetExpenseByIdAsync(id);
        Expense = expense;
        DbError = err;
        await LoadManagersAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostSubmitAsync(int id)
    {
        var (success, err) = await _repo.SubmitExpenseAsync(id);
        if (!success) DbError = err ?? "Could not submit.";
        else ActionMessage = "Expense submitted for approval.";
        var (expense, _) = await _repo.GetExpenseByIdAsync(id);
        Expense = expense;
        await LoadManagersAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostApproveAsync(int id, int reviewedBy)
    {
        var (success, err) = await _repo.ApproveExpenseAsync(id, reviewedBy);
        if (!success) DbError = err ?? "Could not approve.";
        else ActionMessage = "Expense approved.";
        var (expense, _) = await _repo.GetExpenseByIdAsync(id);
        Expense = expense;
        await LoadManagersAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostRejectAsync(int id, int reviewedBy)
    {
        var (success, err) = await _repo.RejectExpenseAsync(id, reviewedBy);
        if (!success) DbError = err ?? "Could not reject.";
        else ActionMessage = "Expense rejected.";
        var (expense, _) = await _repo.GetExpenseByIdAsync(id);
        Expense = expense;
        await LoadManagersAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        await _repo.DeleteExpenseAsync(id);
        return RedirectToPage("/Expenses/Index");
    }

    private async Task LoadManagersAsync()
    {
        var (users, _) = await _repo.GetAllUsersAsync();
        Managers = users.Where(u => u.RoleName == "Manager" && u.IsActive).ToList();
    }
}
