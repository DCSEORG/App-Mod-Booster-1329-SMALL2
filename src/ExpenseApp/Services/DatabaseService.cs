using ExpenseApp.Models;
using Microsoft.Data.SqlClient;

namespace ExpenseApp.Services;

/// <summary>
/// Implements all database operations via stored procedures.
/// Uses Microsoft.Data.SqlClient which supports Azure AD authentication
/// natively through the connection string.
/// </summary>
public class DatabaseService : IDatabaseService
{
    private readonly string  _connectionString;
    private readonly ILogger _logger;

    public DatabaseService(string connectionString, ILogger<DatabaseService> logger)
    {
        _connectionString = connectionString;
        _logger           = logger;
    }

    // -----------------------------------------------------------------------
    // Helpers
    // -----------------------------------------------------------------------

    private SqlConnection CreateConnection() => new SqlConnection(_connectionString);

    private static T? ReadNullable<T>(SqlDataReader r, string col) where T : struct
    {
        int ordinal = r.GetOrdinal(col);
        return r.IsDBNull(ordinal) ? null : (T)Convert.ChangeType(r.GetValue(ordinal), typeof(T));
    }

    private static string? ReadNullableString(SqlDataReader r, string col)
    {
        int ordinal = r.GetOrdinal(col);
        return r.IsDBNull(ordinal) ? null : r.GetString(ordinal);
    }

    // -----------------------------------------------------------------------
    // Lookups
    // -----------------------------------------------------------------------

    public async Task<List<ExpenseCategory>> GetExpenseCategoriesAsync()
    {
        var list = new List<ExpenseCategory>();
        await using var conn = CreateConnection();
        await conn.OpenAsync();
        await using var cmd = new SqlCommand("dbo.GetExpenseCategories", conn)
        {
            CommandType = System.Data.CommandType.StoredProcedure
        };
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            list.Add(new ExpenseCategory
            {
                CategoryId   = reader.GetInt32(reader.GetOrdinal("CategoryId")),
                CategoryName = reader.GetString(reader.GetOrdinal("CategoryName")),
                IsActive     = reader.GetBoolean(reader.GetOrdinal("IsActive"))
            });
        }
        return list;
    }

    public async Task<List<ExpenseStatus>> GetExpenseStatusesAsync()
    {
        var list = new List<ExpenseStatus>();
        await using var conn = CreateConnection();
        await conn.OpenAsync();
        await using var cmd = new SqlCommand("dbo.GetExpenseStatuses", conn)
        {
            CommandType = System.Data.CommandType.StoredProcedure
        };
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            list.Add(new ExpenseStatus
            {
                StatusId   = reader.GetInt32(reader.GetOrdinal("StatusId")),
                StatusName = reader.GetString(reader.GetOrdinal("StatusName"))
            });
        }
        return list;
    }

    // -----------------------------------------------------------------------
    // Users
    // -----------------------------------------------------------------------

    public async Task<List<User>> GetUsersAsync()
    {
        var list = new List<User>();
        await using var conn = CreateConnection();
        await conn.OpenAsync();
        await using var cmd = new SqlCommand("dbo.GetUsers", conn)
        {
            CommandType = System.Data.CommandType.StoredProcedure
        };
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
            list.Add(MapUser(reader));
        return list;
    }

    public async Task<User?> GetUserByIdAsync(int userId)
    {
        await using var conn = CreateConnection();
        await conn.OpenAsync();
        await using var cmd = new SqlCommand("dbo.GetUserById", conn)
        {
            CommandType = System.Data.CommandType.StoredProcedure
        };
        cmd.Parameters.AddWithValue("@UserId", userId);
        await using var reader = await cmd.ExecuteReaderAsync();
        return await reader.ReadAsync() ? MapUser(reader) : null;
    }

    public async Task<int> CreateUserAsync(CreateUserRequest req)
    {
        await using var conn = CreateConnection();
        await conn.OpenAsync();
        await using var cmd = new SqlCommand("dbo.CreateUser", conn)
        {
            CommandType = System.Data.CommandType.StoredProcedure
        };
        cmd.Parameters.AddWithValue("@UserName",  req.UserName);
        cmd.Parameters.AddWithValue("@Email",     req.Email);
        cmd.Parameters.AddWithValue("@RoleId",    req.RoleId);
        cmd.Parameters.AddWithValue("@ManagerId", (object?)req.ManagerId ?? DBNull.Value);
        var result = await cmd.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }

    public async Task<int> UpdateUserAsync(int userId, UpdateUserRequest req)
    {
        await using var conn = CreateConnection();
        await conn.OpenAsync();
        await using var cmd = new SqlCommand("dbo.UpdateUser", conn)
        {
            CommandType = System.Data.CommandType.StoredProcedure
        };
        cmd.Parameters.AddWithValue("@UserId",    userId);
        cmd.Parameters.AddWithValue("@UserName",  req.UserName);
        cmd.Parameters.AddWithValue("@Email",     req.Email);
        cmd.Parameters.AddWithValue("@RoleId",    req.RoleId);
        cmd.Parameters.AddWithValue("@ManagerId", (object?)req.ManagerId ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@IsActive",  req.IsActive);
        await using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
            return reader.GetInt32(reader.GetOrdinal("RowsAffected"));
        return 0;
    }

    // -----------------------------------------------------------------------
    // Expenses
    // -----------------------------------------------------------------------

    public async Task<List<Expense>> GetExpensesAsync()
    {
        var list = new List<Expense>();
        await using var conn = CreateConnection();
        await conn.OpenAsync();
        await using var cmd = new SqlCommand("dbo.GetExpenses", conn)
        {
            CommandType = System.Data.CommandType.StoredProcedure
        };
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
            list.Add(MapExpense(reader));
        return list;
    }

    public async Task<Expense?> GetExpenseByIdAsync(int expenseId)
    {
        await using var conn = CreateConnection();
        await conn.OpenAsync();
        await using var cmd = new SqlCommand("dbo.GetExpenseById", conn)
        {
            CommandType = System.Data.CommandType.StoredProcedure
        };
        cmd.Parameters.AddWithValue("@ExpenseId", expenseId);
        await using var reader = await cmd.ExecuteReaderAsync();
        return await reader.ReadAsync() ? MapExpense(reader) : null;
    }

    public async Task<List<Expense>> GetExpensesByUserAsync(int userId)
    {
        var list = new List<Expense>();
        await using var conn = CreateConnection();
        await conn.OpenAsync();
        await using var cmd = new SqlCommand("dbo.GetExpensesByUser", conn)
        {
            CommandType = System.Data.CommandType.StoredProcedure
        };
        cmd.Parameters.AddWithValue("@UserId", userId);
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
            list.Add(MapExpense(reader));
        return list;
    }

    public async Task<List<Expense>> GetExpensesByStatusAsync(string statusName)
    {
        var list = new List<Expense>();
        await using var conn = CreateConnection();
        await conn.OpenAsync();
        await using var cmd = new SqlCommand("dbo.GetExpensesByStatus", conn)
        {
            CommandType = System.Data.CommandType.StoredProcedure
        };
        cmd.Parameters.AddWithValue("@StatusName", statusName);
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
            list.Add(MapExpense(reader));
        return list;
    }

    public async Task<int> CreateExpenseAsync(CreateExpenseRequest req)
    {
        await using var conn = CreateConnection();
        await conn.OpenAsync();
        await using var cmd = new SqlCommand("dbo.CreateExpense", conn)
        {
            CommandType = System.Data.CommandType.StoredProcedure
        };
        cmd.Parameters.AddWithValue("@UserId",      req.UserId);
        cmd.Parameters.AddWithValue("@CategoryId",  req.CategoryId);
        cmd.Parameters.AddWithValue("@AmountMinor", req.AmountMinor);
        cmd.Parameters.AddWithValue("@Currency",    req.Currency);
        cmd.Parameters.AddWithValue("@ExpenseDate", req.ExpenseDate.Date);
        cmd.Parameters.AddWithValue("@Description", (object?)req.Description ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@ReceiptFile", (object?)req.ReceiptFile  ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@Submit",      req.Submit ? 1 : 0);
        var result = await cmd.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }

    public async Task<int> UpdateExpenseAsync(int expenseId, UpdateExpenseRequest req)
    {
        await using var conn = CreateConnection();
        await conn.OpenAsync();
        await using var cmd = new SqlCommand("dbo.UpdateExpense", conn)
        {
            CommandType = System.Data.CommandType.StoredProcedure
        };
        cmd.Parameters.AddWithValue("@ExpenseId",  expenseId);
        cmd.Parameters.AddWithValue("@CategoryId", req.CategoryId);
        cmd.Parameters.AddWithValue("@AmountMinor",req.AmountMinor);
        cmd.Parameters.AddWithValue("@Currency",   req.Currency);
        cmd.Parameters.AddWithValue("@ExpenseDate",req.ExpenseDate.Date);
        cmd.Parameters.AddWithValue("@Description",(object?)req.Description ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@ReceiptFile",(object?)req.ReceiptFile  ?? DBNull.Value);
        await using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
            return reader.GetInt32(reader.GetOrdinal("RowsAffected"));
        return 0;
    }

    public async Task<int> DeleteExpenseAsync(int expenseId)
    {
        await using var conn = CreateConnection();
        await conn.OpenAsync();
        await using var cmd = new SqlCommand("dbo.DeleteExpense", conn)
        {
            CommandType = System.Data.CommandType.StoredProcedure
        };
        cmd.Parameters.AddWithValue("@ExpenseId", expenseId);
        await using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
            return reader.GetInt32(reader.GetOrdinal("RowsAffected"));
        return 0;
    }

    public async Task<int> ApproveExpenseAsync(int expenseId, int reviewedBy)
    {
        await using var conn = CreateConnection();
        await conn.OpenAsync();
        await using var cmd = new SqlCommand("dbo.ApproveExpense", conn)
        {
            CommandType = System.Data.CommandType.StoredProcedure
        };
        cmd.Parameters.AddWithValue("@ExpenseId",  expenseId);
        cmd.Parameters.AddWithValue("@ReviewedBy", reviewedBy);
        await using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
            return reader.GetInt32(reader.GetOrdinal("RowsAffected"));
        return 0;
    }

    public async Task<int> RejectExpenseAsync(int expenseId, int reviewedBy)
    {
        await using var conn = CreateConnection();
        await conn.OpenAsync();
        await using var cmd = new SqlCommand("dbo.RejectExpense", conn)
        {
            CommandType = System.Data.CommandType.StoredProcedure
        };
        cmd.Parameters.AddWithValue("@ExpenseId",  expenseId);
        cmd.Parameters.AddWithValue("@ReviewedBy", reviewedBy);
        await using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
            return reader.GetInt32(reader.GetOrdinal("RowsAffected"));
        return 0;
    }

    // -----------------------------------------------------------------------
    // Private mappers
    // -----------------------------------------------------------------------

    private static User MapUser(SqlDataReader r) => new()
    {
        UserId      = r.GetInt32(r.GetOrdinal("UserId")),
        UserName    = r.GetString(r.GetOrdinal("UserName")),
        Email       = r.GetString(r.GetOrdinal("Email")),
        RoleId      = r.GetInt32(r.GetOrdinal("RoleId")),
        RoleName    = r.GetString(r.GetOrdinal("RoleName")),
        ManagerId   = ReadNullable<int>(r, "ManagerId"),
        ManagerName = ReadNullableString(r, "ManagerName"),
        IsActive    = r.GetBoolean(r.GetOrdinal("IsActive")),
        CreatedAt   = r.GetDateTime(r.GetOrdinal("CreatedAt"))
    };

    private static Expense MapExpense(SqlDataReader r) => new()
    {
        ExpenseId      = r.GetInt32(r.GetOrdinal("ExpenseId")),
        UserId         = r.GetInt32(r.GetOrdinal("UserId")),
        UserName       = r.GetString(r.GetOrdinal("UserName")),
        CategoryId     = r.GetInt32(r.GetOrdinal("CategoryId")),
        CategoryName   = r.GetString(r.GetOrdinal("CategoryName")),
        StatusId       = r.GetInt32(r.GetOrdinal("StatusId")),
        StatusName     = r.GetString(r.GetOrdinal("StatusName")),
        AmountMinor    = r.GetInt32(r.GetOrdinal("AmountMinor")),
        AmountGBP      = r.GetDecimal(r.GetOrdinal("AmountGBP")),
        Currency       = r.GetString(r.GetOrdinal("Currency")),
        ExpenseDate    = r.GetDateTime(r.GetOrdinal("ExpenseDate")),
        Description    = ReadNullableString(r, "Description"),
        ReceiptFile    = ReadNullableString(r, "ReceiptFile"),
        SubmittedAt    = ReadNullable<DateTime>(r, "SubmittedAt"),
        ReviewedBy     = ReadNullable<int>(r, "ReviewedBy"),
        ReviewedByName = ReadNullableString(r, "ReviewedByName"),
        ReviewedAt     = ReadNullable<DateTime>(r, "ReviewedAt"),
        CreatedAt      = r.GetDateTime(r.GetOrdinal("CreatedAt"))
    };
}
