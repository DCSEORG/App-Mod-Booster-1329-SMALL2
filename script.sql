-- script.sql
-- Configures the managed identity as a database user and grants required permissions.
-- The placeholder MANAGED-IDENTITY-NAME is replaced by run-sql-dbrole.py before execution.

IF EXISTS (SELECT * FROM sys.database_principals WHERE name = 'MANAGED-IDENTITY-NAME')
BEGIN
    DROP USER [MANAGED-IDENTITY-NAME];
END
GO

CREATE USER [MANAGED-IDENTITY-NAME] FROM EXTERNAL PROVIDER;
GO

ALTER ROLE db_datareader ADD MEMBER [MANAGED-IDENTITY-NAME];
ALTER ROLE db_datawriter ADD MEMBER [MANAGED-IDENTITY-NAME];
GRANT EXECUTE TO [MANAGED-IDENTITY-NAME];
GO
