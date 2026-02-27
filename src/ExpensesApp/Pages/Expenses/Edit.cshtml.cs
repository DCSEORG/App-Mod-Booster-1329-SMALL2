using ExpensesApp.Data;
using ExpensesApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ExpensesApp.Pages.Expenses;

public class EditModel : PageModel
{
    private readonly ExpenseRepository _repo;
    public EditModel(ExpenseRepository repo) => _repo = repo;

    public int ExpenseId { get; set; }
    public List<ExpenseCategory> Categories { get; set; } = new();
    public int CurrentCategoryId { get; set; }
    public decimal CurrentAmountGBP { get; set; }
    public DateTime CurrentDate { get; set; } = DateTime.Today;
    public string? CurrentDescription { get; set; }
    public string? CurrentReceiptFile { get; set; }
    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        ExpenseId = id;
        var (expense, _) = await _repo.GetExpenseByIdAsync(id);
        if (expense == null) return NotFound();

        var (cats, _)  = await _repo.GetAllCategoriesAsync();
        Categories = cats.Where(c => c.IsActive).ToList();

        CurrentCategoryId = expense.CategoryId;
        CurrentAmountGBP  = expense.AmountGBP;
        CurrentDate       = expense.ExpenseDate;
        CurrentDescription = expense.Description;
        CurrentReceiptFile = expense.ReceiptFile;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(
        int id, int categoryId, decimal amountGBP,
        DateTime expenseDate, string? description, string? receiptFile)
    {
        ExpenseId = id;
        var (cats, _) = await _repo.GetAllCategoriesAsync();
        Categories = cats.Where(c => c.IsActive).ToList();

        var req = new UpdateExpenseRequest
        {
            CategoryId  = categoryId,
            AmountMinor = (int)(amountGBP * 100),
            ExpenseDate = expenseDate,
            Description = description,
            ReceiptFile = receiptFile
        };

        var (success, err) = await _repo.UpdateExpenseAsync(id, req);
        if (!success)
        {
            ErrorMessage = err ?? "Failed to update expense.";
            return Page();
        }

        return RedirectToPage("/Expenses/Details", new { id });
    }
}
