using ExpenseApp.Models;
using ExpenseApp.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ExpenseApp.Pages.Expenses;

public class CreateModel : PageModel
{
    private readonly IDatabaseService   _db;
    private readonly ILogger<CreateModel> _logger;

    public List<User>           Users      { get; private set; } = new();
    public List<ExpenseCategory> Categories { get; private set; } = new();
    public string? SuccessMessage { get; private set; }

    [BindProperty] public CreateExpenseRequest Input { get; set; } = new();

    public CreateModel(IDatabaseService db, ILogger<CreateModel> logger)
    {
        _db     = db;
        _logger = logger;
    }

    public async Task OnGetAsync()
    {
        await LoadLookupsAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await LoadLookupsAsync();

        if (!ModelState.IsValid)
            return Page();

        try
        {
            int newId = await _db.CreateExpenseAsync(Input);
            SuccessMessage = $"Expense #{newId} created successfully.";
            Input = new CreateExpenseRequest();   // reset form
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CreateExpense failed");
            ViewData["DbError"] = IndexModel_Pages.BuildErrorMessage(ex);
        }

        return Page();
    }

    private async Task LoadLookupsAsync()
    {
        try
        {
            Users      = await _db.GetUsersAsync();
            Categories = await _db.GetExpenseCategoriesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to load lookups; using dummy data");
            Users      = DummyData.GetUsers();
            Categories = DummyData.GetCategories();
        }
    }
}
