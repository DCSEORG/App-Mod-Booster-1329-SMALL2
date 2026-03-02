using ExpenseApp.Models;
using ExpenseApp.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ExpenseApp.Pages.Users;

public class UserEditInput
{
    public int     UserId    { get; set; }
    public string  UserName  { get; set; } = string.Empty;
    public string  Email     { get; set; } = string.Empty;
    public int     RoleId    { get; set; }
    public int?    ManagerId { get; set; }
    public bool    IsActive  { get; set; } = true;
}

public class EditModel : PageModel
{
    private readonly IDatabaseService  _db;
    private readonly ILogger<EditModel> _logger;

    public List<User> Managers       { get; private set; } = new();
    public string?    SuccessMessage { get; private set; }

    [BindProperty] public UserEditInput? Input { get; set; }

    public EditModel(IDatabaseService db, ILogger<EditModel> logger)
    {
        _db     = db;
        _logger = logger;
    }

    public async Task OnGetAsync([FromQuery] int id)
    {
        await LoadManagersAsync();
        try
        {
            var user = await _db.GetUserByIdAsync(id);
            if (user == null) { Input = null; return; }
            Input = new UserEditInput
            {
                UserId    = user.UserId,
                UserName  = user.UserName,
                Email     = user.Email,
                RoleId    = user.RoleId,
                ManagerId = user.ManagerId,
                IsActive  = user.IsActive
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "EditUser GET failed for {Id}", id);
            ViewData["DbError"] = ExpenseApp.Pages.IndexModel.BuildErrorMessage(ex);
        }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await LoadManagersAsync();
        if (!ModelState.IsValid || Input == null) return Page();
        try
        {
            var req = new UpdateUserRequest
            {
                UserName  = Input.UserName,
                Email     = Input.Email,
                RoleId    = Input.RoleId,
                ManagerId = Input.ManagerId,
                IsActive  = Input.IsActive
            };
            await _db.UpdateUserAsync(Input.UserId, req);
            SuccessMessage = "User updated.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "EditUser POST failed");
            ViewData["DbError"] = ExpenseApp.Pages.IndexModel.BuildErrorMessage(ex);
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
        catch { Managers = DummyData.GetUsers().Where(u => u.RoleName == "Manager").ToList(); }
    }
}
