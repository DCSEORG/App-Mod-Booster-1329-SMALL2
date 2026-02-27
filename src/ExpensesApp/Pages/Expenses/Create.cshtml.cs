using ExpensesApp.Data;
using ExpensesApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ExpensesApp.Pages.Expenses;

public class CreateModel : PageModel
{
    private readonly ExpenseRepository _repo;
    public CreateModel(ExpenseRepository repo) => _repo = repo;

    public List<User> Users { get; set; } = new();
    public List<ExpenseCategory> Categories { get; set; } = new();
    public string? ErrorMessage { get; set; }

    public async Task OnGetAsync()
    {
        var (users, _)      = await _repo.GetAllUsersAsync();
        var (categories, _) = await _repo.GetAllCategoriesAsync();
        Users      = users.Where(u => u.IsActive).ToList();
        Categories = categories.Where(c => c.IsActive).ToList();
    }

    public async Task<IActionResult> OnPostAsync(
        int userId, int categoryId, decimal amountGBP,
        DateTime expenseDate, string? description, string? receiptFile,
        string action)
    {
        var (users, _)      = await _repo.GetAllUsersAsync();
        var (categories, _) = await _repo.GetAllCategoriesAsync();
        Users      = users.Where(u => u.IsActive).ToList();
        Categories = categories.Where(c => c.IsActive).ToList();

        var req = new CreateExpenseRequest
        {
            UserId      = userId,
            CategoryId  = categoryId,
            AmountMinor = (int)(amountGBP * 100),
            ExpenseDate = expenseDate,
            Description = description,
            ReceiptFile = receiptFile
        };

        var (newId, err) = await _repo.CreateExpenseAsync(req);
        if (err != null || newId == 0)
        {
            ErrorMessage = err ?? "Failed to create expense.";
            return Page();
        }

        if (action == "submit")
        {
            await _repo.SubmitExpenseAsync(newId);
        }

        return RedirectToPage("/Expenses/Index");
    }
}
