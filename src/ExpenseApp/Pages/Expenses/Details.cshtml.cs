using ExpenseApp.Models;
using ExpenseApp.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ExpenseApp.Pages.Expenses;

public class DetailsModel : PageModel
{
    private readonly IDatabaseService     _db;
    private readonly ILogger<DetailsModel> _logger;

    public Expense? Expense { get; private set; }

    public DetailsModel(IDatabaseService db, ILogger<DetailsModel> logger)
    {
        _db     = db;
        _logger = logger;
    }

    public async Task OnGetAsync([FromQuery] int id)
    {
        try
        {
            Expense = await _db.GetExpenseByIdAsync(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Details: failed to load expense {Id}", id);
            ViewData["DbError"] = ErrorMessageHelper.BuildErrorMessage(ex);
            Expense = DummyData.GetExpenses().FirstOrDefault(e => e.ExpenseId == id)
                      ?? DummyData.GetExpenses().First();
        }
    }
}
