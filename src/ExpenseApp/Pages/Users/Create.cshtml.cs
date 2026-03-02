using ExpenseApp.Models;
using ExpenseApp.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ExpenseApp.Pages.Users;

public class CreateModel : PageModel
{
    private readonly IDatabaseService   _db;
    private readonly ILogger<CreateModel> _logger;

    public List<User> Managers      { get; private set; } = new();
    public string?    SuccessMessage { get; private set; }

    [BindProperty] public CreateUserRequest Input { get; set; } = new();

    public CreateModel(IDatabaseService db, ILogger<CreateModel> logger)
    {
        _db     = db;
        _logger = logger;
    }

    public async Task OnGetAsync()
    {
        await LoadManagersAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await LoadManagersAsync();
        if (!ModelState.IsValid) return Page();

        try
        {
            int newId = await _db.CreateUserAsync(Input);
            SuccessMessage = $"User #{newId} created successfully.";
            Input = new CreateUserRequest();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CreateUser failed");
            ViewData["DbError"] = ErrorMessageHelper.BuildErrorMessage(ex);
        }
        return Page();
    }

    private async Task LoadManagersAsync()
    {
        try
        {
            var all = await _db.GetUsersAsync();
            Managers = all.Where(u => u.RoleName == "Manager").ToList();
        }
        catch
        {
            Managers = DummyData.GetUsers().Where(u => u.RoleName == "Manager").ToList();
        }
    }
}
