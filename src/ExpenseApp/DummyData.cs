using ExpenseApp.Models;

namespace ExpenseApp;

/// <summary>
/// Sample data returned when the database is unavailable so that the UI
/// remains functional and gives a realistic preview of the application.
/// </summary>
public static class DummyData
{
    public static List<User> GetUsers() =>
    [
        new() { UserId = 1, UserName = "Alice Example",  Email = "alice@example.co.uk",     RoleId = 1, RoleName = "Employee", ManagerId = 2, ManagerName = "Bob Manager",  IsActive = true, CreatedAt = DateTime.UtcNow.AddDays(-30) },
        new() { UserId = 2, UserName = "Bob Manager",    Email = "bob.manager@example.co.uk",RoleId = 2, RoleName = "Manager",  ManagerId = null, ManagerName = null, IsActive = true, CreatedAt = DateTime.UtcNow.AddDays(-60) }
    ];

    public static List<ExpenseCategory> GetCategories() =>
    [
        new() { CategoryId = 1, CategoryName = "Travel",        IsActive = true },
        new() { CategoryId = 2, CategoryName = "Meals",         IsActive = true },
        new() { CategoryId = 3, CategoryName = "Supplies",      IsActive = true },
        new() { CategoryId = 4, CategoryName = "Accommodation", IsActive = true },
        new() { CategoryId = 5, CategoryName = "Other",         IsActive = true }
    ];

    public static List<ExpenseStatus> GetStatuses() =>
    [
        new() { StatusId = 1, StatusName = "Draft"     },
        new() { StatusId = 2, StatusName = "Submitted" },
        new() { StatusId = 3, StatusName = "Approved"  },
        new() { StatusId = 4, StatusName = "Rejected"  }
    ];

    public static List<Expense> GetExpenses() =>
    [
        new() { ExpenseId = 1, UserId = 1, UserName = "Alice Example", CategoryId = 1, CategoryName = "Travel",
                StatusId = 2, StatusName = "Submitted", AmountMinor = 2540, AmountGBP = 25.40m, Currency = "GBP",
                ExpenseDate = DateTime.UtcNow.AddDays(-14), Description = "Taxi from airport to client site",
                SubmittedAt = DateTime.UtcNow.AddDays(-13), CreatedAt = DateTime.UtcNow.AddDays(-14) },
        new() { ExpenseId = 2, UserId = 1, UserName = "Alice Example", CategoryId = 2, CategoryName = "Meals",
                StatusId = 3, StatusName = "Approved",   AmountMinor = 1425, AmountGBP = 14.25m, Currency = "GBP",
                ExpenseDate = DateTime.UtcNow.AddDays(-45), Description = "Client lunch meeting",
                SubmittedAt = DateTime.UtcNow.AddDays(-44), ReviewedBy = 2, ReviewedByName = "Bob Manager",
                ReviewedAt  = DateTime.UtcNow.AddDays(-43), CreatedAt = DateTime.UtcNow.AddDays(-45) },
        new() { ExpenseId = 3, UserId = 1, UserName = "Alice Example", CategoryId = 3, CategoryName = "Supplies",
                StatusId = 1, StatusName = "Draft",      AmountMinor = 799,  AmountGBP = 7.99m,  Currency = "GBP",
                ExpenseDate = DateTime.UtcNow.AddDays(-3), Description = "Office stationery",
                CreatedAt = DateTime.UtcNow.AddDays(-3) },
        new() { ExpenseId = 4, UserId = 1, UserName = "Alice Example", CategoryId = 4, CategoryName = "Accommodation",
                StatusId = 3, StatusName = "Approved",   AmountMinor = 12300, AmountGBP = 123.00m, Currency = "GBP",
                ExpenseDate = DateTime.UtcNow.AddDays(-90), Description = "Hotel during client visit",
                SubmittedAt = DateTime.UtcNow.AddDays(-89), ReviewedBy = 2, ReviewedByName = "Bob Manager",
                ReviewedAt  = DateTime.UtcNow.AddDays(-88), CreatedAt = DateTime.UtcNow.AddDays(-90) }
    ];
}
