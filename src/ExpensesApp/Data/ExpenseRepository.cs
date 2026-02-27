using System.Data;
using Microsoft.Data.SqlClient;
using ExpensesApp.Models;

namespace ExpensesApp.Data;

/// <summary>
/// Repository for all database operations. Uses stored procedures exclusively.
/// Returns dummy/seed data when the database connection is unavailable.
/// </summary>
public class ExpenseRepository
{
    private readonly string _connectionString;
    private readonly ILogger<ExpenseRepository> _logger;

    public ExpenseRepository(string connectionString, ILogger<ExpenseRepository> logger)
    {
        _connectionString = connectionString;
        _logger = logger;
    }

    private SqlConnection CreateConnection() => new(_connectionString);

    // ── Dummy / fallback data ─────────────────────────────────────────────────

    private static List<Expense> DummyExpenses() => new()
    {
        new Expense { ExpenseId=1, UserId=1, UserName="Alice Example", CategoryId=1, CategoryName="Travel",
            StatusId=2, StatusName="Submitted", AmountMinor=2540, Currency="GBP",
            ExpenseDate=new DateTime(2025,10,20), Description="Taxi from airport to client site",
            SubmittedAt=DateTime.UtcNow.AddDays(-3), CreatedAt=DateTime.UtcNow.AddDays(-3) },
        new Expense { ExpenseId=2, UserId=1, UserName="Alice Example", CategoryId=2, CategoryName="Meals",
            StatusId=3, StatusName="Approved", AmountMinor=1425, Currency="GBP",
            ExpenseDate=new DateTime(2025,9,15), Description="Client lunch meeting",
            SubmittedAt=DateTime.UtcNow.AddDays(-20), ReviewedBy=2, ReviewedByName="Bob Manager",
            ReviewedAt=DateTime.UtcNow.AddDays(-19), CreatedAt=DateTime.UtcNow.AddDays(-21) },
        new Expense { ExpenseId=3, UserId=1, UserName="Alice Example", CategoryId=3, CategoryName="Supplies",
            StatusId=1, StatusName="Draft", AmountMinor=799, Currency="GBP",
            ExpenseDate=new DateTime(2025,11,1), Description="Office stationery",
            CreatedAt=DateTime.UtcNow.AddDays(-1) },
        new Expense { ExpenseId=4, UserId=1, UserName="Alice Example", CategoryId=4, CategoryName="Accommodation",
            StatusId=3, StatusName="Approved", AmountMinor=12300, Currency="GBP",
            ExpenseDate=new DateTime(2025,8,10), Description="Hotel during client visit",
            SubmittedAt=DateTime.UtcNow.AddDays(-45), ReviewedBy=2, ReviewedByName="Bob Manager",
            ReviewedAt=DateTime.UtcNow.AddDays(-44), CreatedAt=DateTime.UtcNow.AddDays(-46) },
    };

    private static List<User> DummyUsers() => new()
    {
        new User { UserId=1, UserName="Alice Example", Email="alice@example.co.uk",
            RoleId=1, RoleName="Employee", ManagerId=2, ManagerName="Bob Manager",
            IsActive=true, CreatedAt=DateTime.UtcNow.AddDays(-100) },
        new User { UserId=2, UserName="Bob Manager", Email="bob.manager@example.co.uk",
            RoleId=2, RoleName="Manager", IsActive=true, CreatedAt=DateTime.UtcNow.AddDays(-200) },
    };

    private static List<Role> DummyRoles() => new()
    {
        new Role { RoleId=1, RoleName="Employee", Description="Regular employee who can submit expenses" },
        new Role { RoleId=2, RoleName="Manager",  Description="Can view and approve/reject submitted expenses" },
    };

    private static List<ExpenseCategory> DummyCategories() => new()
    {
        new ExpenseCategory { CategoryId=1, CategoryName="Travel",        IsActive=true },
        new ExpenseCategory { CategoryId=2, CategoryName="Meals",         IsActive=true },
        new ExpenseCategory { CategoryId=3, CategoryName="Supplies",      IsActive=true },
        new ExpenseCategory { CategoryId=4, CategoryName="Accommodation", IsActive=true },
        new ExpenseCategory { CategoryId=5, CategoryName="Other",         IsActive=true },
    };

    private static List<ExpenseStatus> DummyStatuses() => new()
    {
        new ExpenseStatus { StatusId=1, StatusName="Draft" },
        new ExpenseStatus { StatusId=2, StatusName="Submitted" },
        new ExpenseStatus { StatusId=3, StatusName="Approved" },
        new ExpenseStatus { StatusId=4, StatusName="Rejected" },
    };

    // ── Expenses ──────────────────────────────────────────────────────────────

    public async Task<(List<Expense> expenses, string? error)> GetAllExpensesAsync()
    {
        try
        {
            await using var conn = CreateConnection();
            await conn.OpenAsync();
            await using var cmd = new SqlCommand("dbo.GetAllExpenses", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            var expenses = await ReadExpensesAsync(cmd);
            return (expenses, null);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "DB unavailable – returning dummy expenses");
            return (DummyExpenses(), FormatError(ex));
        }
    }

    public async Task<(Expense? expense, string? error)> GetExpenseByIdAsync(int id)
    {
        try
        {
            await using var conn = CreateConnection();
            await conn.OpenAsync();
            await using var cmd = new SqlCommand("dbo.GetExpenseById", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.AddWithValue("@ExpenseId", id);
            var list = await ReadExpensesAsync(cmd);
            return (list.FirstOrDefault(), null);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "DB unavailable – returning dummy expense");
            return (DummyExpenses().FirstOrDefault(e => e.ExpenseId == id), FormatError(ex));
        }
    }

    public async Task<(List<Expense> expenses, string? error)> GetExpensesByUserAsync(int userId)
    {
        try
        {
            await using var conn = CreateConnection();
            await conn.OpenAsync();
            await using var cmd = new SqlCommand("dbo.GetExpensesByUser", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.AddWithValue("@UserId", userId);
            var expenses = await ReadExpensesAsync(cmd);
            return (expenses, null);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "DB unavailable");
            return (DummyExpenses().Where(e => e.UserId == userId).ToList(), FormatError(ex));
        }
    }

    public async Task<(List<Expense> expenses, string? error)> GetExpensesByStatusAsync(string statusName)
    {
        try
        {
            await using var conn = CreateConnection();
            await conn.OpenAsync();
            await using var cmd = new SqlCommand("dbo.GetExpensesByStatus", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.AddWithValue("@StatusName", statusName);
            var expenses = await ReadExpensesAsync(cmd);
            return (expenses, null);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "DB unavailable");
            return (DummyExpenses().Where(e => e.StatusName == statusName).ToList(), FormatError(ex));
        }
    }

    public async Task<(List<Expense> expenses, string? error)> GetPendingExpensesAsync()
    {
        try
        {
            await using var conn = CreateConnection();
            await conn.OpenAsync();
            await using var cmd = new SqlCommand("dbo.GetPendingExpensesForManager", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            var expenses = await ReadExpensesAsync(cmd);
            return (expenses, null);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "DB unavailable");
            return (DummyExpenses().Where(e => e.StatusName == "Submitted").ToList(), FormatError(ex));
        }
    }

    public async Task<(int newId, string? error)> CreateExpenseAsync(CreateExpenseRequest req)
    {
        try
        {
            await using var conn = CreateConnection();
            await conn.OpenAsync();
            await using var cmd = new SqlCommand("dbo.CreateExpense", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.AddWithValue("@UserId",      req.UserId);
            cmd.Parameters.AddWithValue("@CategoryId",  req.CategoryId);
            cmd.Parameters.AddWithValue("@AmountMinor", req.AmountMinor);
            cmd.Parameters.AddWithValue("@ExpenseDate", req.ExpenseDate.Date);
            cmd.Parameters.AddWithValue("@Description", (object?)req.Description ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ReceiptFile", (object?)req.ReceiptFile  ?? DBNull.Value);
            var result = await cmd.ExecuteScalarAsync();
            return (Convert.ToInt32(result), null);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "DB unavailable – cannot create expense");
            return (0, FormatError(ex));
        }
    }

    public async Task<(bool success, string? error)> UpdateExpenseAsync(int id, UpdateExpenseRequest req)
    {
        try
        {
            await using var conn = CreateConnection();
            await conn.OpenAsync();
            await using var cmd = new SqlCommand("dbo.UpdateExpense", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.AddWithValue("@ExpenseId",   id);
            cmd.Parameters.AddWithValue("@CategoryId",  req.CategoryId);
            cmd.Parameters.AddWithValue("@AmountMinor", req.AmountMinor);
            cmd.Parameters.AddWithValue("@ExpenseDate", req.ExpenseDate.Date);
            cmd.Parameters.AddWithValue("@Description", (object?)req.Description ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ReceiptFile", (object?)req.ReceiptFile  ?? DBNull.Value);
            var rows = (int)(await cmd.ExecuteScalarAsync() ?? 0);
            return (rows > 0, null);
        }
        catch (Exception ex)
        {
            return (false, FormatError(ex));
        }
    }

    public async Task<(bool success, string? error)> SubmitExpenseAsync(int id)
    {
        try
        {
            await using var conn = CreateConnection();
            await conn.OpenAsync();
            await using var cmd = new SqlCommand("dbo.SubmitExpense", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.AddWithValue("@ExpenseId", id);
            var rows = (int)(await cmd.ExecuteScalarAsync() ?? 0);
            return (rows > 0, null);
        }
        catch (Exception ex)
        {
            return (false, FormatError(ex));
        }
    }

    public async Task<(bool success, string? error)> ApproveExpenseAsync(int id, int reviewedBy)
    {
        try
        {
            await using var conn = CreateConnection();
            await conn.OpenAsync();
            await using var cmd = new SqlCommand("dbo.ApproveExpense", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.AddWithValue("@ExpenseId",  id);
            cmd.Parameters.AddWithValue("@ReviewedBy", reviewedBy);
            var rows = (int)(await cmd.ExecuteScalarAsync() ?? 0);
            return (rows > 0, null);
        }
        catch (Exception ex)
        {
            return (false, FormatError(ex));
        }
    }

    public async Task<(bool success, string? error)> RejectExpenseAsync(int id, int reviewedBy)
    {
        try
        {
            await using var conn = CreateConnection();
            await conn.OpenAsync();
            await using var cmd = new SqlCommand("dbo.RejectExpense", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.AddWithValue("@ExpenseId",  id);
            cmd.Parameters.AddWithValue("@ReviewedBy", reviewedBy);
            var rows = (int)(await cmd.ExecuteScalarAsync() ?? 0);
            return (rows > 0, null);
        }
        catch (Exception ex)
        {
            return (false, FormatError(ex));
        }
    }

    public async Task<(bool success, string? error)> DeleteExpenseAsync(int id)
    {
        try
        {
            await using var conn = CreateConnection();
            await conn.OpenAsync();
            await using var cmd = new SqlCommand("dbo.DeleteExpense", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.AddWithValue("@ExpenseId", id);
            var rows = (int)(await cmd.ExecuteScalarAsync() ?? 0);
            return (rows > 0, null);
        }
        catch (Exception ex)
        {
            return (false, FormatError(ex));
        }
    }

    // ── Summary ───────────────────────────────────────────────────────────────

    public async Task<(List<ExpenseSummary> summaries, string? error)> GetSummaryByCategoryAsync()
    {
        try
        {
            await using var conn = CreateConnection();
            await conn.OpenAsync();
            await using var cmd = new SqlCommand("dbo.GetExpenseSummaryByCategory", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            var summaries = new List<ExpenseSummary>();
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                summaries.Add(new ExpenseSummary
                {
                    GroupName        = reader.GetString(0),
                    ExpenseCount     = reader.GetInt32(1),
                    TotalAmountMinor = reader.GetInt32(2)
                });
            }
            return (summaries, null);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "DB unavailable – returning dummy summary");
            var dummy = DummyExpenses()
                .GroupBy(e => e.CategoryName)
                .Select(g => new ExpenseSummary
                {
                    GroupName        = g.Key,
                    ExpenseCount     = g.Count(),
                    TotalAmountMinor = g.Sum(e => e.AmountMinor)
                }).ToList();
            return (dummy, FormatError(ex));
        }
    }

    public async Task<(List<ExpenseSummary> summaries, string? error)> GetSummaryByStatusAsync()
    {
        try
        {
            await using var conn = CreateConnection();
            await conn.OpenAsync();
            await using var cmd = new SqlCommand("dbo.GetExpenseSummaryByStatus", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            var summaries = new List<ExpenseSummary>();
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                summaries.Add(new ExpenseSummary
                {
                    GroupName        = reader.GetString(0),
                    ExpenseCount     = reader.GetInt32(1),
                    TotalAmountMinor = reader.GetInt32(2)
                });
            }
            return (summaries, null);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "DB unavailable – returning dummy summary");
            var dummy = DummyExpenses()
                .GroupBy(e => e.StatusName)
                .Select(g => new ExpenseSummary
                {
                    GroupName        = g.Key,
                    ExpenseCount     = g.Count(),
                    TotalAmountMinor = g.Sum(e => e.AmountMinor)
                }).ToList();
            return (dummy, FormatError(ex));
        }
    }

    // ── Users ─────────────────────────────────────────────────────────────────

    public async Task<(List<User> users, string? error)> GetAllUsersAsync()
    {
        try
        {
            await using var conn = CreateConnection();
            await conn.OpenAsync();
            await using var cmd = new SqlCommand("dbo.GetAllUsers", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            var users = await ReadUsersAsync(cmd);
            return (users, null);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "DB unavailable – returning dummy users");
            return (DummyUsers(), FormatError(ex));
        }
    }

    public async Task<(User? user, string? error)> GetUserByIdAsync(int id)
    {
        try
        {
            await using var conn = CreateConnection();
            await conn.OpenAsync();
            await using var cmd = new SqlCommand("dbo.GetUserById", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.AddWithValue("@UserId", id);
            var list = await ReadUsersAsync(cmd);
            return (list.FirstOrDefault(), null);
        }
        catch (Exception ex)
        {
            return (DummyUsers().FirstOrDefault(u => u.UserId == id), FormatError(ex));
        }
    }

    public async Task<(int newId, string? error)> CreateUserAsync(CreateUserRequest req)
    {
        try
        {
            await using var conn = CreateConnection();
            await conn.OpenAsync();
            await using var cmd = new SqlCommand("dbo.CreateUser", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.AddWithValue("@UserName",  req.UserName);
            cmd.Parameters.AddWithValue("@Email",     req.Email);
            cmd.Parameters.AddWithValue("@RoleId",    req.RoleId);
            cmd.Parameters.AddWithValue("@ManagerId", (object?)req.ManagerId ?? DBNull.Value);
            var result = await cmd.ExecuteScalarAsync();
            return (Convert.ToInt32(result), null);
        }
        catch (Exception ex)
        {
            return (0, FormatError(ex));
        }
    }

    public async Task<(bool success, string? error)> UpdateUserAsync(int id, UpdateUserRequest req)
    {
        try
        {
            await using var conn = CreateConnection();
            await conn.OpenAsync();
            await using var cmd = new SqlCommand("dbo.UpdateUser", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.AddWithValue("@UserId",    id);
            cmd.Parameters.AddWithValue("@UserName",  req.UserName);
            cmd.Parameters.AddWithValue("@Email",     req.Email);
            cmd.Parameters.AddWithValue("@RoleId",    req.RoleId);
            cmd.Parameters.AddWithValue("@ManagerId", (object?)req.ManagerId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@IsActive",  req.IsActive);
            var rows = (int)(await cmd.ExecuteScalarAsync() ?? 0);
            return (rows > 0, null);
        }
        catch (Exception ex)
        {
            return (false, FormatError(ex));
        }
    }

    public async Task<(bool success, string? error)> DeleteUserAsync(int id)
    {
        try
        {
            await using var conn = CreateConnection();
            await conn.OpenAsync();
            await using var cmd = new SqlCommand("dbo.DeleteUser", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.AddWithValue("@UserId", id);
            var rows = (int)(await cmd.ExecuteScalarAsync() ?? 0);
            return (rows > 0, null);
        }
        catch (Exception ex)
        {
            return (false, FormatError(ex));
        }
    }

    // ── Roles ─────────────────────────────────────────────────────────────────

    public async Task<(List<Role> roles, string? error)> GetAllRolesAsync()
    {
        try
        {
            await using var conn = CreateConnection();
            await conn.OpenAsync();
            await using var cmd = new SqlCommand("dbo.GetAllRoles", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            var roles = new List<Role>();
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                roles.Add(new Role
                {
                    RoleId      = reader.GetInt32(0),
                    RoleName    = reader.GetString(1),
                    Description = reader.IsDBNull(2) ? null : reader.GetString(2)
                });
            }
            return (roles, null);
        }
        catch (Exception ex)
        {
            return (DummyRoles(), FormatError(ex));
        }
    }

    // ── Categories ────────────────────────────────────────────────────────────

    public async Task<(List<ExpenseCategory> categories, string? error)> GetAllCategoriesAsync()
    {
        try
        {
            await using var conn = CreateConnection();
            await conn.OpenAsync();
            await using var cmd = new SqlCommand("dbo.GetAllExpenseCategories", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            var cats = new List<ExpenseCategory>();
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                cats.Add(new ExpenseCategory
                {
                    CategoryId   = reader.GetInt32(0),
                    CategoryName = reader.GetString(1),
                    IsActive     = reader.GetBoolean(2)
                });
            }
            return (cats, null);
        }
        catch (Exception ex)
        {
            return (DummyCategories(), FormatError(ex));
        }
    }

    public async Task<(int newId, string? error)> CreateCategoryAsync(string name)
    {
        try
        {
            await using var conn = CreateConnection();
            await conn.OpenAsync();
            await using var cmd = new SqlCommand("dbo.CreateExpenseCategory", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.AddWithValue("@CategoryName", name);
            var result = await cmd.ExecuteScalarAsync();
            return (Convert.ToInt32(result), null);
        }
        catch (Exception ex)
        {
            return (0, FormatError(ex));
        }
    }

    // ── Statuses ──────────────────────────────────────────────────────────────

    public async Task<(List<ExpenseStatus> statuses, string? error)> GetAllStatusesAsync()
    {
        try
        {
            await using var conn = CreateConnection();
            await conn.OpenAsync();
            await using var cmd = new SqlCommand("dbo.GetAllExpenseStatuses", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            var statuses = new List<ExpenseStatus>();
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                statuses.Add(new ExpenseStatus
                {
                    StatusId   = reader.GetInt32(0),
                    StatusName = reader.GetString(1)
                });
            }
            return (statuses, null);
        }
        catch (Exception ex)
        {
            return (DummyStatuses(), FormatError(ex));
        }
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static async Task<List<Expense>> ReadExpensesAsync(SqlCommand cmd)
    {
        var expenses = new List<Expense>();
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            expenses.Add(new Expense
            {
                ExpenseId      = reader.GetInt32(reader.GetOrdinal("ExpenseId")),
                UserId         = reader.GetInt32(reader.GetOrdinal("UserId")),
                UserName       = reader.GetString(reader.GetOrdinal("UserName")),
                CategoryId     = reader.GetInt32(reader.GetOrdinal("CategoryId")),
                CategoryName   = reader.GetString(reader.GetOrdinal("CategoryName")),
                StatusId       = reader.GetInt32(reader.GetOrdinal("StatusId")),
                StatusName     = reader.GetString(reader.GetOrdinal("StatusName")),
                AmountMinor    = reader.GetInt32(reader.GetOrdinal("AmountMinor")),
                Currency       = reader.GetString(reader.GetOrdinal("Currency")),
                ExpenseDate    = reader.GetDateTime(reader.GetOrdinal("ExpenseDate")),
                Description    = reader.IsDBNull(reader.GetOrdinal("Description"))    ? null : reader.GetString(reader.GetOrdinal("Description")),
                ReceiptFile    = reader.IsDBNull(reader.GetOrdinal("ReceiptFile"))     ? null : reader.GetString(reader.GetOrdinal("ReceiptFile")),
                SubmittedAt    = reader.IsDBNull(reader.GetOrdinal("SubmittedAt"))    ? null : reader.GetDateTime(reader.GetOrdinal("SubmittedAt")),
                ReviewedBy     = reader.IsDBNull(reader.GetOrdinal("ReviewedBy"))     ? null : reader.GetInt32(reader.GetOrdinal("ReviewedBy")),
                ReviewedByName = HasColumn(reader, "ReviewedByName") && !reader.IsDBNull(reader.GetOrdinal("ReviewedByName")) ? reader.GetString(reader.GetOrdinal("ReviewedByName")) : null,
                ReviewedAt     = reader.IsDBNull(reader.GetOrdinal("ReviewedAt"))     ? null : reader.GetDateTime(reader.GetOrdinal("ReviewedAt")),
                CreatedAt      = reader.GetDateTime(reader.GetOrdinal("CreatedAt"))
            });
        }
        return expenses;
    }

    private static async Task<List<User>> ReadUsersAsync(SqlCommand cmd)
    {
        var users = new List<User>();
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            users.Add(new User
            {
                UserId      = reader.GetInt32(reader.GetOrdinal("UserId")),
                UserName    = reader.GetString(reader.GetOrdinal("UserName")),
                Email       = reader.GetString(reader.GetOrdinal("Email")),
                RoleId      = reader.GetInt32(reader.GetOrdinal("RoleId")),
                RoleName    = reader.GetString(reader.GetOrdinal("RoleName")),
                ManagerId   = reader.IsDBNull(reader.GetOrdinal("ManagerId"))   ? null : reader.GetInt32(reader.GetOrdinal("ManagerId")),
                ManagerName = reader.IsDBNull(reader.GetOrdinal("ManagerName")) ? null : reader.GetString(reader.GetOrdinal("ManagerName")),
                IsActive    = reader.GetBoolean(reader.GetOrdinal("IsActive")),
                CreatedAt   = reader.GetDateTime(reader.GetOrdinal("CreatedAt"))
            });
        }
        return users;
    }

    private static bool HasColumn(SqlDataReader reader, string name)
    {
        for (int i = 0; i < reader.FieldCount; i++)
            if (reader.GetName(i).Equals(name, StringComparison.OrdinalIgnoreCase)) return true;
        return false;
    }

    private static string FormatError(Exception ex)
    {
        var msg = ex.InnerException?.Message ?? ex.Message;
        if (msg.Contains("Login failed") || msg.Contains("Managed Identity"))
            return $"Database connection failed. Ensure the managed identity is configured: " +
                   $"Set AZURE_CLIENT_ID in App Service → Configuration → Application Settings. " +
                   $"Error: {msg}";
        return $"Database unavailable (showing demo data). Error: {msg}";
    }
}
