using ExpenseApp.Models;
using ExpenseApp.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ExpenseApp.Pages.Users;

public class IndexModel : PageModel
{
    private readonly IDatabaseService  _db;
    private readonly ILogger<IndexModel> _logger;

    public List<User> Users { get; private set; } = new();

    public IndexModel(IDatabaseService db, ILogger<IndexModel> logger)
    {
        _db     = db;
        _logger = logger;
    }

    public async Task OnGetAsync()
    {
        try
        {
            Users = await _db.GetUsersAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Users/Index failed");
            ViewData["DbError"] = ErrorMessageHelper.BuildErrorMessage(ex);
            Users = DummyData.GetUsers();
        }
    }
}
