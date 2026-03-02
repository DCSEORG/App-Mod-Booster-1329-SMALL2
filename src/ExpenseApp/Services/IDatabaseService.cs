using ExpenseApp.Models;
using Microsoft.Data.SqlClient;

namespace ExpenseApp.Services;

/// <summary>
/// Contract for all database operations.  All methods call stored procedures –
/// no direct table access or inline T-SQL anywhere in the application.
/// </summary>
public interface IDatabaseService
{
    // Lookups
    Task<List<ExpenseCategory>> GetExpenseCategoriesAsync();
    Task<List<ExpenseStatus>>   GetExpenseStatusesAsync();

    // Users
    Task<List<User>> GetUsersAsync();
    Task<User?>      GetUserByIdAsync(int userId);
    Task<int>        CreateUserAsync(CreateUserRequest req);
    Task<int>        UpdateUserAsync(int userId, UpdateUserRequest req);

    // Expenses
    Task<List<Expense>> GetExpensesAsync();
    Task<Expense?>      GetExpenseByIdAsync(int expenseId);
    Task<List<Expense>> GetExpensesByUserAsync(int userId);
    Task<List<Expense>> GetExpensesByStatusAsync(string statusName);
    Task<int>           CreateExpenseAsync(CreateExpenseRequest req);
    Task<int>           UpdateExpenseAsync(int expenseId, UpdateExpenseRequest req);
    Task<int>           DeleteExpenseAsync(int expenseId);
    Task<int>           ApproveExpenseAsync(int expenseId, int reviewedBy);
    Task<int>           RejectExpenseAsync(int expenseId, int reviewedBy);
}
