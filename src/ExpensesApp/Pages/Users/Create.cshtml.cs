using ExpensesApp.Data;
using ExpensesApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ExpensesApp.Pages.Users;

public class CreateModel : PageModel
{
    private readonly ExpenseRepository _repo;
    public CreateModel(ExpenseRepository repo) => _repo = repo;

    public List<Role> Roles { get; set; } = new();
    public List<User> Managers { get; set; } = new();
    public string? ErrorMessage { get; set; }

    public async Task OnGetAsync()
    {
        var (roles, _) = await _repo.GetAllRolesAsync();
        var (users, _) = await _repo.GetAllUsersAsync();
        Roles    = roles;
        Managers = users.Where(u => u.RoleName == "Manager" && u.IsActive).ToList();
    }

    public async Task<IActionResult> OnPostAsync(
        string userName, string email, int roleId, int? managerId)
    {
        var (roles, _) = await _repo.GetAllRolesAsync();
        var (users, _) = await _repo.GetAllUsersAsync();
        Roles    = roles;
        Managers = users.Where(u => u.RoleName == "Manager" && u.IsActive).ToList();

        var req = new CreateUserRequest
        {
            UserName  = userName,
            Email     = email,
            RoleId    = roleId,
            ManagerId = managerId
        };

        var (newId, err) = await _repo.CreateUserAsync(req);
        if (err != null || newId == 0)
        {
            ErrorMessage = err ?? "Failed to create user.";
            return Page();
        }

        return RedirectToPage("/Users/Index");
    }
}
