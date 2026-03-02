/*
  stored-procedures.sql
  CRUD stored procedures for the ExpenseDB Expense Management System.
  Uses "CREATE OR ALTER PROCEDURE" so the script is idempotent and safe to re-run.
  Application code must use ONLY these stored procedures – no direct table access.
*/

-- =============================================================================
-- EXPENSE CATEGORIES
-- =============================================================================

CREATE OR ALTER PROCEDURE dbo.GetExpenseCategories
AS
BEGIN
    SET NOCOUNT ON;
    SELECT CategoryId, CategoryName, IsActive
    FROM   dbo.ExpenseCategories
    WHERE  IsActive = 1
    ORDER  BY CategoryName;
END
GO

-- =============================================================================
-- EXPENSE STATUSES
-- =============================================================================

CREATE OR ALTER PROCEDURE dbo.GetExpenseStatuses
AS
BEGIN
    SET NOCOUNT ON;
    SELECT StatusId, StatusName
    FROM   dbo.ExpenseStatus
    ORDER  BY StatusId;
END
GO

-- =============================================================================
-- USERS
-- =============================================================================

CREATE OR ALTER PROCEDURE dbo.GetUsers
AS
BEGIN
    SET NOCOUNT ON;
    SELECT u.UserId,
           u.UserName,
           u.Email,
           u.RoleId,
           r.RoleName,
           u.ManagerId,
           m.UserName  AS ManagerName,
           u.IsActive,
           u.CreatedAt
    FROM   dbo.Users u
    JOIN   dbo.Roles r ON r.RoleId = u.RoleId
    LEFT JOIN dbo.Users m ON m.UserId = u.ManagerId
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
           m.UserName  AS ManagerName,
           u.IsActive,
           u.CreatedAt
    FROM   dbo.Users u
    JOIN   dbo.Roles r ON r.RoleId = u.RoleId
    LEFT JOIN dbo.Users m ON m.UserId = u.ManagerId
    WHERE  u.UserId = @UserId;
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

    SELECT SCOPE_IDENTITY() AS NewUserId;
END
GO

CREATE OR ALTER PROCEDURE dbo.UpdateUser
    @UserId    INT,
    @UserName  NVARCHAR(100),
    @Email     NVARCHAR(255),
    @RoleId    INT,
    @ManagerId INT = NULL,
    @IsActive  BIT = 1
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

-- =============================================================================
-- EXPENSES
-- =============================================================================

CREATE OR ALTER PROCEDURE dbo.GetExpenses
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
    JOIN   dbo.Users u             ON u.UserId     = e.UserId
    JOIN   dbo.ExpenseCategories c ON c.CategoryId = e.CategoryId
    JOIN   dbo.ExpenseStatus s     ON s.StatusId   = e.StatusId
    LEFT JOIN dbo.Users rev        ON rev.UserId   = e.ReviewedBy
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
    JOIN   dbo.Users u             ON u.UserId     = e.UserId
    JOIN   dbo.ExpenseCategories c ON c.CategoryId = e.CategoryId
    JOIN   dbo.ExpenseStatus s     ON s.StatusId   = e.StatusId
    LEFT JOIN dbo.Users rev        ON rev.UserId   = e.ReviewedBy
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
    JOIN   dbo.Users u             ON u.UserId     = e.UserId
    JOIN   dbo.ExpenseCategories c ON c.CategoryId = e.CategoryId
    JOIN   dbo.ExpenseStatus s     ON s.StatusId   = e.StatusId
    LEFT JOIN dbo.Users rev        ON rev.UserId   = e.ReviewedBy
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
    JOIN   dbo.Users u             ON u.UserId     = e.UserId
    JOIN   dbo.ExpenseCategories c ON c.CategoryId = e.CategoryId
    JOIN   dbo.ExpenseStatus s     ON s.StatusId   = e.StatusId
    LEFT JOIN dbo.Users rev        ON rev.UserId   = e.ReviewedBy
    WHERE  s.StatusName = @StatusName
    ORDER  BY e.SubmittedAt ASC;
END
GO

CREATE OR ALTER PROCEDURE dbo.CreateExpense
    @UserId      INT,
    @CategoryId  INT,
    @AmountMinor INT,
    @Currency    NVARCHAR(3) = 'GBP',
    @ExpenseDate DATE,
    @Description NVARCHAR(1000) = NULL,
    @ReceiptFile NVARCHAR(500)  = NULL,
    @Submit      BIT = 0   -- 1 = immediately submit (Status: Submitted), 0 = Draft
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @StatusId INT;

    IF @Submit = 1
        SELECT @StatusId = StatusId FROM dbo.ExpenseStatus WHERE StatusName = 'Submitted';
    ELSE
        SELECT @StatusId = StatusId FROM dbo.ExpenseStatus WHERE StatusName = 'Draft';

    INSERT INTO dbo.Expenses
        (UserId, CategoryId, StatusId, AmountMinor, Currency, ExpenseDate,
         Description, ReceiptFile, SubmittedAt)
    VALUES
        (@UserId, @CategoryId, @StatusId, @AmountMinor, @Currency, @ExpenseDate,
         @Description, @ReceiptFile,
         CASE WHEN @Submit = 1 THEN SYSUTCDATETIME() ELSE NULL END);

    SELECT SCOPE_IDENTITY() AS NewExpenseId;
END
GO

CREATE OR ALTER PROCEDURE dbo.UpdateExpense
    @ExpenseId   INT,
    @CategoryId  INT,
    @AmountMinor INT,
    @Currency    NVARCHAR(3) = 'GBP',
    @ExpenseDate DATE,
    @Description NVARCHAR(1000) = NULL,
    @ReceiptFile NVARCHAR(500)  = NULL
AS
BEGIN
    SET NOCOUNT ON;
    -- Only Draft expenses may be updated
    UPDATE dbo.Expenses
    SET    CategoryId  = @CategoryId,
           AmountMinor = @AmountMinor,
           Currency    = @Currency,
           ExpenseDate = @ExpenseDate,
           Description = @Description,
           ReceiptFile = @ReceiptFile
    WHERE  ExpenseId = @ExpenseId
      AND  StatusId  = (SELECT StatusId FROM dbo.ExpenseStatus WHERE StatusName = 'Draft');

    SELECT @@ROWCOUNT AS RowsAffected;
END
GO

CREATE OR ALTER PROCEDURE dbo.DeleteExpense
    @ExpenseId INT
AS
BEGIN
    SET NOCOUNT ON;
    -- Only Draft expenses may be deleted
    DELETE FROM dbo.Expenses
    WHERE  ExpenseId = @ExpenseId
      AND  StatusId  = (SELECT StatusId FROM dbo.ExpenseStatus WHERE StatusName = 'Draft');

    SELECT @@ROWCOUNT AS RowsAffected;
END
GO

-- =============================================================================
-- APPROVE / REJECT
-- =============================================================================

CREATE OR ALTER PROCEDURE dbo.ApproveExpense
    @ExpenseId   INT,
    @ReviewedBy  INT   -- UserId of the approving manager
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.Expenses
    SET    StatusId   = (SELECT StatusId FROM dbo.ExpenseStatus WHERE StatusName = 'Approved'),
           ReviewedBy = @ReviewedBy,
           ReviewedAt = SYSUTCDATETIME()
    WHERE  ExpenseId = @ExpenseId
      AND  StatusId  = (SELECT StatusId FROM dbo.ExpenseStatus WHERE StatusName = 'Submitted');

    SELECT @@ROWCOUNT AS RowsAffected;
END
GO

CREATE OR ALTER PROCEDURE dbo.RejectExpense
    @ExpenseId   INT,
    @ReviewedBy  INT   -- UserId of the rejecting manager
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.Expenses
    SET    StatusId   = (SELECT StatusId FROM dbo.ExpenseStatus WHERE StatusName = 'Rejected'),
           ReviewedBy = @ReviewedBy,
           ReviewedAt = SYSUTCDATETIME()
    WHERE  ExpenseId = @ExpenseId
      AND  StatusId  = (SELECT StatusId FROM dbo.ExpenseStatus WHERE StatusName = 'Submitted');

    SELECT @@ROWCOUNT AS RowsAffected;
END
GO
