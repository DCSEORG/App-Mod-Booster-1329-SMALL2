/*
  stored-procedures.sql
  CRUD stored procedures for the Expenses Management System.
  Tables: Roles, Users, ExpenseCategories, ExpenseStatus, Expenses
  Currency: GBP – amounts stored in pence (AmountMinor INT)
*/

SET NOCOUNT ON;
GO

-- ============================================================
-- ROLES
-- ============================================================

CREATE OR ALTER PROCEDURE dbo.GetAllRoles
AS
BEGIN
    SET NOCOUNT ON;
    SELECT RoleId, RoleName, Description
    FROM   dbo.Roles
    ORDER  BY RoleName;
END
GO

CREATE OR ALTER PROCEDURE dbo.GetRoleById
    @RoleId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT RoleId, RoleName, Description
    FROM   dbo.Roles
    WHERE  RoleId = @RoleId;
END
GO

CREATE OR ALTER PROCEDURE dbo.CreateRole
    @RoleName    NVARCHAR(50),
    @Description NVARCHAR(250) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.Roles (RoleName, Description)
    VALUES (@RoleName, @Description);
    SELECT SCOPE_IDENTITY() AS RoleId;
END
GO

CREATE OR ALTER PROCEDURE dbo.UpdateRole
    @RoleId      INT,
    @RoleName    NVARCHAR(50),
    @Description NVARCHAR(250) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.Roles
    SET    RoleName    = @RoleName,
           Description = @Description
    WHERE  RoleId = @RoleId;
    SELECT @@ROWCOUNT AS RowsAffected;
END
GO

CREATE OR ALTER PROCEDURE dbo.DeleteRole
    @RoleId INT
AS
BEGIN
    SET NOCOUNT ON;
    DELETE FROM dbo.Roles WHERE RoleId = @RoleId;
    SELECT @@ROWCOUNT AS RowsAffected;
END
GO

-- ============================================================
-- USERS
-- ============================================================

CREATE OR ALTER PROCEDURE dbo.GetAllUsers
AS
BEGIN
    SET NOCOUNT ON;
    SELECT u.UserId,
           u.UserName,
           u.Email,
           u.RoleId,
           r.RoleName,
           u.ManagerId,
           m.UserName AS ManagerName,
           u.IsActive,
           u.CreatedAt
    FROM   dbo.Users u
    JOIN   dbo.Roles r ON u.RoleId = r.RoleId
    LEFT JOIN dbo.Users m ON u.ManagerId = m.UserId
    ORDER  BY u.UserName;
END
GO

CREATE OR ALTER PROCEDURE dbo.GetUserById
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT u.UserId,
           u.UserName,
           u.Email,
           u.RoleId,
           r.RoleName,
           u.ManagerId,
           m.UserName AS ManagerName,
           u.IsActive,
           u.CreatedAt
    FROM   dbo.Users u
    JOIN   dbo.Roles r ON u.RoleId = r.RoleId
    LEFT JOIN dbo.Users m ON u.ManagerId = m.UserId
    WHERE  u.UserId = @UserId;
END
GO

CREATE OR ALTER PROCEDURE dbo.GetUsersByRole
    @RoleName NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT u.UserId,
           u.UserName,
           u.Email,
           u.RoleId,
           r.RoleName,
           u.ManagerId,
           m.UserName AS ManagerName,
           u.IsActive,
           u.CreatedAt
    FROM   dbo.Users u
    JOIN   dbo.Roles r ON u.RoleId = r.RoleId
    LEFT JOIN dbo.Users m ON u.ManagerId = m.UserId
    WHERE  r.RoleName = @RoleName
    AND    u.IsActive  = 1
    ORDER  BY u.UserName;
END
GO

CREATE OR ALTER PROCEDURE dbo.CreateUser
    @UserName  NVARCHAR(100),
    @Email     NVARCHAR(255),
    @RoleId    INT,
    @ManagerId INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.Users (UserName, Email, RoleId, ManagerId, IsActive)
    VALUES (@UserName, @Email, @RoleId, @ManagerId, 1);
    SELECT SCOPE_IDENTITY() AS UserId;
END
GO

CREATE OR ALTER PROCEDURE dbo.UpdateUser
    @UserId    INT,
    @UserName  NVARCHAR(100),
    @Email     NVARCHAR(255),
    @RoleId    INT,
    @ManagerId INT  = NULL,
    @IsActive  BIT  = 1
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.Users
    SET    UserName  = @UserName,
           Email     = @Email,
           RoleId    = @RoleId,
           ManagerId = @ManagerId,
           IsActive  = @IsActive
    WHERE  UserId = @UserId;
    SELECT @@ROWCOUNT AS RowsAffected;
END
GO

CREATE OR ALTER PROCEDURE dbo.DeleteUser
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;
    -- Soft delete
    UPDATE dbo.Users SET IsActive = 0 WHERE UserId = @UserId;
    SELECT @@ROWCOUNT AS RowsAffected;
END
GO

-- ============================================================
-- EXPENSE CATEGORIES
-- ============================================================

CREATE OR ALTER PROCEDURE dbo.GetAllExpenseCategories
AS
BEGIN
    SET NOCOUNT ON;
    SELECT CategoryId, CategoryName, IsActive
    FROM   dbo.ExpenseCategories
    ORDER  BY CategoryName;
END
GO

CREATE OR ALTER PROCEDURE dbo.GetExpenseCategoryById
    @CategoryId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT CategoryId, CategoryName, IsActive
    FROM   dbo.ExpenseCategories
    WHERE  CategoryId = @CategoryId;
END
GO

CREATE OR ALTER PROCEDURE dbo.CreateExpenseCategory
    @CategoryName NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.ExpenseCategories (CategoryName, IsActive)
    VALUES (@CategoryName, 1);
    SELECT SCOPE_IDENTITY() AS CategoryId;
END
GO

CREATE OR ALTER PROCEDURE dbo.UpdateExpenseCategory
    @CategoryId   INT,
    @CategoryName NVARCHAR(100),
    @IsActive     BIT = 1
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.ExpenseCategories
    SET    CategoryName = @CategoryName,
           IsActive     = @IsActive
    WHERE  CategoryId = @CategoryId;
    SELECT @@ROWCOUNT AS RowsAffected;
END
GO

CREATE OR ALTER PROCEDURE dbo.DeleteExpenseCategory
    @CategoryId INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.ExpenseCategories SET IsActive = 0 WHERE CategoryId = @CategoryId;
    SELECT @@ROWCOUNT AS RowsAffected;
END
GO

-- ============================================================
-- EXPENSE STATUS
-- ============================================================

CREATE OR ALTER PROCEDURE dbo.GetAllExpenseStatuses
AS
BEGIN
    SET NOCOUNT ON;
    SELECT StatusId, StatusName
    FROM   dbo.ExpenseStatus
    ORDER  BY StatusId;
END
GO

CREATE OR ALTER PROCEDURE dbo.GetExpenseStatusById
    @StatusId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT StatusId, StatusName
    FROM   dbo.ExpenseStatus
    WHERE  StatusId = @StatusId;
END
GO

CREATE OR ALTER PROCEDURE dbo.CreateExpenseStatus
    @StatusName NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.ExpenseStatus (StatusName)
    VALUES (@StatusName);
    SELECT SCOPE_IDENTITY() AS StatusId;
END
GO

CREATE OR ALTER PROCEDURE dbo.UpdateExpenseStatus
    @StatusId   INT,
    @StatusName NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.ExpenseStatus
    SET    StatusName = @StatusName
    WHERE  StatusId = @StatusId;
    SELECT @@ROWCOUNT AS RowsAffected;
END
GO

CREATE OR ALTER PROCEDURE dbo.DeleteExpenseStatus
    @StatusId INT
AS
BEGIN
    SET NOCOUNT ON;
    DELETE FROM dbo.ExpenseStatus WHERE StatusId = @StatusId;
    SELECT @@ROWCOUNT AS RowsAffected;
END
GO

-- ============================================================
-- EXPENSES
-- ============================================================

CREATE OR ALTER PROCEDURE dbo.GetAllExpenses
AS
BEGIN
    SET NOCOUNT ON;
    SELECT e.ExpenseId,
           e.UserId,
           u.UserName,
           e.CategoryId,
           c.CategoryName,
           e.StatusId,
           s.StatusName,
           e.AmountMinor,
           CAST(e.AmountMinor / 100.0 AS DECIMAL(10,2)) AS AmountGBP,
           e.Currency,
           e.ExpenseDate,
           e.Description,
           e.ReceiptFile,
           e.SubmittedAt,
           e.ReviewedBy,
           rev.UserName AS ReviewedByName,
           e.ReviewedAt,
           e.CreatedAt
    FROM   dbo.Expenses e
    JOIN   dbo.Users u            ON e.UserId     = u.UserId
    JOIN   dbo.ExpenseCategories c ON e.CategoryId = c.CategoryId
    JOIN   dbo.ExpenseStatus s    ON e.StatusId   = s.StatusId
    LEFT JOIN dbo.Users rev       ON e.ReviewedBy = rev.UserId
    ORDER  BY e.CreatedAt DESC;
END
GO

CREATE OR ALTER PROCEDURE dbo.GetExpenseById
    @ExpenseId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT e.ExpenseId,
           e.UserId,
           u.UserName,
           e.CategoryId,
           c.CategoryName,
           e.StatusId,
           s.StatusName,
           e.AmountMinor,
           CAST(e.AmountMinor / 100.0 AS DECIMAL(10,2)) AS AmountGBP,
           e.Currency,
           e.ExpenseDate,
           e.Description,
           e.ReceiptFile,
           e.SubmittedAt,
           e.ReviewedBy,
           rev.UserName AS ReviewedByName,
           e.ReviewedAt,
           e.CreatedAt
    FROM   dbo.Expenses e
    JOIN   dbo.Users u            ON e.UserId     = u.UserId
    JOIN   dbo.ExpenseCategories c ON e.CategoryId = c.CategoryId
    JOIN   dbo.ExpenseStatus s    ON e.StatusId   = s.StatusId
    LEFT JOIN dbo.Users rev       ON e.ReviewedBy = rev.UserId
    WHERE  e.ExpenseId = @ExpenseId;
END
GO

CREATE OR ALTER PROCEDURE dbo.GetExpensesByUser
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT e.ExpenseId,
           e.UserId,
           u.UserName,
           e.CategoryId,
           c.CategoryName,
           e.StatusId,
           s.StatusName,
           e.AmountMinor,
           CAST(e.AmountMinor / 100.0 AS DECIMAL(10,2)) AS AmountGBP,
           e.Currency,
           e.ExpenseDate,
           e.Description,
           e.ReceiptFile,
           e.SubmittedAt,
           e.ReviewedBy,
           rev.UserName AS ReviewedByName,
           e.ReviewedAt,
           e.CreatedAt
    FROM   dbo.Expenses e
    JOIN   dbo.Users u            ON e.UserId     = u.UserId
    JOIN   dbo.ExpenseCategories c ON e.CategoryId = c.CategoryId
    JOIN   dbo.ExpenseStatus s    ON e.StatusId   = s.StatusId
    LEFT JOIN dbo.Users rev       ON e.ReviewedBy = rev.UserId
    WHERE  e.UserId = @UserId
    ORDER  BY e.CreatedAt DESC;
END
GO

CREATE OR ALTER PROCEDURE dbo.GetExpensesByStatus
    @StatusName NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT e.ExpenseId,
           e.UserId,
           u.UserName,
           e.CategoryId,
           c.CategoryName,
           e.StatusId,
           s.StatusName,
           e.AmountMinor,
           CAST(e.AmountMinor / 100.0 AS DECIMAL(10,2)) AS AmountGBP,
           e.Currency,
           e.ExpenseDate,
           e.Description,
           e.ReceiptFile,
           e.SubmittedAt,
           e.ReviewedBy,
           rev.UserName AS ReviewedByName,
           e.ReviewedAt,
           e.CreatedAt
    FROM   dbo.Expenses e
    JOIN   dbo.Users u            ON e.UserId     = u.UserId
    JOIN   dbo.ExpenseCategories c ON e.CategoryId = c.CategoryId
    JOIN   dbo.ExpenseStatus s    ON e.StatusId   = s.StatusId
    LEFT JOIN dbo.Users rev       ON e.ReviewedBy = rev.UserId
    WHERE  s.StatusName = @StatusName
    ORDER  BY e.SubmittedAt ASC;
END
GO

CREATE OR ALTER PROCEDURE dbo.GetPendingExpensesForManager
AS
BEGIN
    SET NOCOUNT ON;
    SELECT e.ExpenseId,
           e.UserId,
           u.UserName,
           e.CategoryId,
           c.CategoryName,
           e.StatusId,
           s.StatusName,
           e.AmountMinor,
           CAST(e.AmountMinor / 100.0 AS DECIMAL(10,2)) AS AmountGBP,
           e.Currency,
           e.ExpenseDate,
           e.Description,
           e.ReceiptFile,
           e.SubmittedAt,
           e.CreatedAt
    FROM   dbo.Expenses e
    JOIN   dbo.Users u            ON e.UserId     = u.UserId
    JOIN   dbo.ExpenseCategories c ON e.CategoryId = c.CategoryId
    JOIN   dbo.ExpenseStatus s    ON e.StatusId   = s.StatusId
    WHERE  s.StatusName = 'Submitted'
    ORDER  BY e.SubmittedAt ASC;
END
GO

CREATE OR ALTER PROCEDURE dbo.CreateExpense
    @UserId      INT,
    @CategoryId  INT,
    @AmountMinor INT,
    @ExpenseDate DATE,
    @Description NVARCHAR(1000) = NULL,
    @ReceiptFile NVARCHAR(500)  = NULL
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @DraftStatusId INT;
    SELECT @DraftStatusId = StatusId FROM dbo.ExpenseStatus WHERE StatusName = 'Draft';

    INSERT INTO dbo.Expenses
        (UserId, CategoryId, StatusId, AmountMinor, Currency, ExpenseDate, Description, ReceiptFile)
    VALUES
        (@UserId, @CategoryId, @DraftStatusId, @AmountMinor, 'GBP', @ExpenseDate, @Description, @ReceiptFile);

    SELECT SCOPE_IDENTITY() AS ExpenseId;
END
GO

CREATE OR ALTER PROCEDURE dbo.UpdateExpense
    @ExpenseId   INT,
    @CategoryId  INT,
    @AmountMinor INT,
    @ExpenseDate DATE,
    @Description NVARCHAR(1000) = NULL,
    @ReceiptFile NVARCHAR(500)  = NULL
AS
BEGIN
    SET NOCOUNT ON;
    -- Only allow editing Draft expenses
    UPDATE dbo.Expenses
    SET    CategoryId  = @CategoryId,
           AmountMinor = @AmountMinor,
           ExpenseDate = @ExpenseDate,
           Description = @Description,
           ReceiptFile = @ReceiptFile
    WHERE  ExpenseId = @ExpenseId
    AND    StatusId  = (SELECT StatusId FROM dbo.ExpenseStatus WHERE StatusName = 'Draft');
    SELECT @@ROWCOUNT AS RowsAffected;
END
GO

CREATE OR ALTER PROCEDURE dbo.SubmitExpense
    @ExpenseId INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.Expenses
    SET    StatusId    = (SELECT StatusId FROM dbo.ExpenseStatus WHERE StatusName = 'Submitted'),
           SubmittedAt = SYSUTCDATETIME()
    WHERE  ExpenseId = @ExpenseId
    AND    StatusId  = (SELECT StatusId FROM dbo.ExpenseStatus WHERE StatusName = 'Draft');
    SELECT @@ROWCOUNT AS RowsAffected;
END
GO

CREATE OR ALTER PROCEDURE dbo.ApproveExpense
    @ExpenseId  INT,
    @ReviewedBy INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.Expenses
    SET    StatusId   = (SELECT StatusId FROM dbo.ExpenseStatus WHERE StatusName = 'Approved'),
           ReviewedBy = @ReviewedBy,
           ReviewedAt = SYSUTCDATETIME()
    WHERE  ExpenseId = @ExpenseId
    AND    StatusId  = (SELECT StatusId FROM dbo.ExpenseStatus WHERE StatusName = 'Submitted');
    SELECT @@ROWCOUNT AS RowsAffected;
END
GO

CREATE OR ALTER PROCEDURE dbo.RejectExpense
    @ExpenseId  INT,
    @ReviewedBy INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.Expenses
    SET    StatusId   = (SELECT StatusId FROM dbo.ExpenseStatus WHERE StatusName = 'Rejected'),
           ReviewedBy = @ReviewedBy,
           ReviewedAt = SYSUTCDATETIME()
    WHERE  ExpenseId = @ExpenseId
    AND    StatusId  = (SELECT StatusId FROM dbo.ExpenseStatus WHERE StatusName = 'Submitted');
    SELECT @@ROWCOUNT AS RowsAffected;
END
GO

CREATE OR ALTER PROCEDURE dbo.DeleteExpense
    @ExpenseId INT
AS
BEGIN
    SET NOCOUNT ON;
    -- Only allow deleting Draft expenses
    DELETE FROM dbo.Expenses
    WHERE  ExpenseId = @ExpenseId
    AND    StatusId  = (SELECT StatusId FROM dbo.ExpenseStatus WHERE StatusName = 'Draft');
    SELECT @@ROWCOUNT AS RowsAffected;
END
GO

CREATE OR ALTER PROCEDURE dbo.GetExpenseSummaryByCategory
AS
BEGIN
    SET NOCOUNT ON;
    SELECT c.CategoryName,
           COUNT(*)                                          AS ExpenseCount,
           SUM(e.AmountMinor)                               AS TotalAmountMinor,
           CAST(SUM(e.AmountMinor) / 100.0 AS DECIMAL(10,2)) AS TotalAmountGBP
    FROM   dbo.Expenses e
    JOIN   dbo.ExpenseCategories c ON e.CategoryId = c.CategoryId
    GROUP  BY c.CategoryName
    ORDER  BY TotalAmountMinor DESC;
END
GO

CREATE OR ALTER PROCEDURE dbo.GetExpenseSummaryByStatus
AS
BEGIN
    SET NOCOUNT ON;
    SELECT s.StatusName,
           COUNT(*)                                          AS ExpenseCount,
           SUM(e.AmountMinor)                               AS TotalAmountMinor,
           CAST(SUM(e.AmountMinor) / 100.0 AS DECIMAL(10,2)) AS TotalAmountGBP
    FROM   dbo.Expenses e
    JOIN   dbo.ExpenseStatus s ON e.StatusId = s.StatusId
    GROUP  BY s.StatusName
    ORDER  BY s.StatusName;
END
GO
