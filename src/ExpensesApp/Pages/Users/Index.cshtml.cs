using ExpensesApp.Data;
using ExpensesApp.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ExpensesApp.Pages.Users;

public class IndexModel : PageModel
{
    private readonly ExpenseRepository _repo;
    public IndexModel(ExpenseRepository repo) => _repo = repo;

    public List<User> Users { get; set; } = new();
    public string? DbError { get; set; }

    public async Task OnGetAsync()
    {
        var (users, err) = await _repo.GetAllUsersAsync();
        Users   = users;
        DbError = err;
    }
}
