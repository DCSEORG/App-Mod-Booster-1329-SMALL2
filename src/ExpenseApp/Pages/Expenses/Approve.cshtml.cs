using ExpenseApp.Models;
using ExpenseApp.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ExpenseApp.Pages.Expenses;

public class ApproveModel : PageModel
{
    private readonly IDatabaseService    _db;
    private readonly ILogger<ApproveModel> _logger;

    public List<Expense> PendingExpenses { get; private set; } = new();
    public List<User>    Managers        { get; private set; } = new();
    public string?       Message         { get; private set; }

    [BindProperty(SupportsGet = true)] public int ReviewerId { get; set; }

    public ApproveModel(IDatabaseService db, ILogger<ApproveModel> logger)
    {
        _db     = db;
        _logger = logger;
    }

    public async Task OnGetAsync()
    {
        await LoadDataAsync();
    }

    public async Task<IActionResult> OnPostApproveAsync(int expenseId, int reviewerId)
    {
        try
        {
            await _db.ApproveExpenseAsync(expenseId, reviewerId);
            Message = $"Expense #{expenseId} approved.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Approve failed for {Id}", expenseId);
            ViewData["DbError"] = IndexModel_Pages.BuildErrorMessage(ex);
        }
        ReviewerId = reviewerId;
        await LoadDataAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostRejectAsync(int expenseId, int reviewerId)
    {
        try
        {
            await _db.RejectExpenseAsync(expenseId, reviewerId);
            Message = $"Expense #{expenseId} rejected.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Reject failed for {Id}", expenseId);
            ViewData["DbError"] = IndexModel_Pages.BuildErrorMessage(ex);
        }
        ReviewerId = reviewerId;
        await LoadDataAsync();
        return Page();
    }

    private async Task LoadDataAsync()
    {
        try
        {
            var allUsers = await _db.GetUsersAsync();
            Managers        = allUsers.Where(u => u.RoleName == "Manager").ToList();
            PendingExpenses = await _db.GetExpensesByStatusAsync("Submitted");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ApproveModel: failed to load data");
            ViewData["DbError"] = IndexModel_Pages.BuildErrorMessage(ex);
            PendingExpenses = DummyData.GetExpenses().Where(e => e.StatusName == "Submitted").ToList();
            Managers        = DummyData.GetUsers().Where(u => u.RoleName == "Manager").ToList();
        }
    }
}
