using ExpenseApp.Models;
using ExpenseApp.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ExpenseApp.Pages.Expenses;

public class EditInputModel
{
    public int      ExpenseId   { get; set; }
    public int      CategoryId  { get; set; }
    public int      AmountMinor { get; set; }
    public string   Currency    { get; set; } = "GBP";
    public DateTime ExpenseDate { get; set; }
    public string?  Description { get; set; }
    public string?  ReceiptFile { get; set; }
}

public class EditModel : PageModel
{
    private readonly IDatabaseService  _db;
    private readonly ILogger<EditModel> _logger;

    public List<ExpenseCategory> Categories    { get; private set; } = new();
    public string?               SuccessMessage { get; private set; }

    [BindProperty] public EditInputModel? Input { get; set; }

    public EditModel(IDatabaseService db, ILogger<EditModel> logger)
    {
        _db     = db;
        _logger = logger;
    }

    public async Task OnGetAsync([FromQuery] int id)
    {
        await LoadCategoriesAsync();
        try
        {
            var expense = await _db.GetExpenseByIdAsync(id);
            if (expense == null || expense.StatusName != "Draft")
            {
                Input = null;
                return;
            }
            Input = new EditInputModel
            {
                ExpenseId   = expense.ExpenseId,
                CategoryId  = expense.CategoryId,
                AmountMinor = expense.AmountMinor,
                Currency    = expense.Currency,
                ExpenseDate = expense.ExpenseDate,
                Description = expense.Description,
                ReceiptFile = expense.ReceiptFile
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Edit GET failed for expense {Id}", id);
            ViewData["DbError"] = ErrorMessageHelper.BuildErrorMessage(ex);
        }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await LoadCategoriesAsync();
        if (!ModelState.IsValid || Input == null) return Page();

        try
        {
            var req = new UpdateExpenseRequest
            {
                CategoryId  = Input.CategoryId,
                AmountMinor = Input.AmountMinor,
                Currency    = Input.Currency,
                ExpenseDate = Input.ExpenseDate,
                Description = Input.Description,
                ReceiptFile = Input.ReceiptFile
            };
            await _db.UpdateExpenseAsync(Input.ExpenseId, req);
            SuccessMessage = "Expense updated successfully.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Edit POST failed");
            ViewData["DbError"] = ErrorMessageHelper.BuildErrorMessage(ex);
        }
        return Page();
    }

    private async Task LoadCategoriesAsync()
    {
        try   { Categories = await _db.GetExpenseCategoriesAsync(); }
        catch { Categories = DummyData.GetCategories(); }
    }
}
