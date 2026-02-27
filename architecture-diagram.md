# Architecture Diagram – Expenses Management System

```
┌─────────────────────────────────────────────────────────────────────────┐
│                         Azure (UK South)                                 │
│                                                                          │
│  ┌──────────────────────────────────────────────────────────────────┐   │
│  │                       Resource Group                             │   │
│  │                  rg-appmodassist                                 │   │
│  │                                                                  │   │
│  │  ┌─────────────────────┐     ┌─────────────────────────────┐    │   │
│  │  │   User-Assigned     │     │       App Service           │    │   │
│  │  │  Managed Identity   │────▶│  app-appmodassist-[hash]    │    │   │
│  │  │                     │     │  .NET 8 · Standard S1       │    │   │
│  │  │ mid-appmodassist-   │     │  Razor Pages + REST API     │    │   │
│  │  │     [hash]          │     │  Swagger at /swagger        │    │   │
│  │  └─────────────────────┘     └──────────────┬──────────────┘    │   │
│  │           │                                 │                   │   │
│  │           │ Assigned to                     │ Managed Identity  │   │
│  │           │                                 │ Authentication    │   │
│  │           ▼                                 ▼                   │   │
│  │  ┌─────────────────────────────────────────────────────────┐    │   │
│  │  │                  Azure SQL Database                      │    │   │
│  │  │  sql-appmodassist-[hash].database.windows.net           │    │   │
│  │  │  Database: Northwind · Basic Tier                        │    │   │
│  │  │  Entra ID-Only Auth (no SQL passwords)                  │    │   │
│  │  │                                                          │    │   │
│  │  │  Tables:                  Stored Procedures:             │    │   │
│  │  │  • dbo.Roles              • GetAllRoles                  │    │   │
│  │  │  • dbo.Users              • GetAllUsers                  │    │   │
│  │  │  • dbo.ExpenseCategories  • GetAllExpenses               │    │   │
│  │  │  • dbo.ExpenseStatus      • CreateExpense                │    │   │
│  │  │  • dbo.Expenses           • SubmitExpense                │    │   │
│  │  │                           • ApproveExpense               │    │   │
│  │  │                           • RejectExpense                │    │   │
│  │  │                           • (+ 15 more SPs)             │    │   │
│  │  └─────────────────────────────────────────────────────────┘    │   │
│  │                                                                  │   │
│  └──────────────────────────────────────────────────────────────────┘   │
│                                                                          │
└─────────────────────────────────────────────────────────────────────────┘
                                   ▲
                                   │  HTTPS
                                   │
                            ┌──────┴──────┐
                            │    User     │
                            │  (Browser)  │
                            └─────────────┘
```

## Component Details

| Component | Technology | Purpose |
|---|---|---|
| **User-Assigned Managed Identity** | `mid-appmodassist-[hash]` | Provides passwordless identity for App Service to authenticate to Azure SQL |
| **App Service Plan** | Standard S1, UK South | Hosts the web application (no cold start) |
| **App Service** | ASP.NET Core .NET 8 | Razor Pages UI + REST API with Swagger |
| **Azure SQL Server** | `sql-appmodassist-[hash]` | Hosts the Northwind database with Entra ID-only auth |
| **Azure SQL Database** | Northwind, Basic Tier | Stores Roles, Users, ExpenseCategories, ExpenseStatus, Expenses |

## Authentication Flow

```
User (Browser)
    │
    │  HTTPS
    ▼
App Service (.NET 8 Razor Pages)
    │
    │  Authentication=Active Directory Managed Identity
    │  User Id=<managed-identity-client-id>
    ▼
Azure SQL Database (Northwind)
    │
    │  Entra ID validates token from Managed Identity
    ▼
Stored Procedures (CRUD operations only)
```

## Application URLs

| URL | Description |
|---|---|
| `/Index` | Dashboard with stats and recent expenses |
| `/Expenses` | All expenses with status filter |
| `/Expenses/Create` | Submit a new expense claim |
| `/Expenses/Pending` | Manager approval queue |
| `/Expenses/Details/{id}` | Expense details + approve/reject |
| `/Users` | User management |
| `/swagger` | Swagger / OpenAPI documentation |
| `/api/expenses` | REST API for expenses |
| `/api/users` | REST API for users |
| `/api/roles` | REST API for roles |
| `/api/categories` | REST API for expense categories |
| `/api/statuses` | REST API for expense statuses |

## Security Principles

- **No SQL passwords** – Azure SQL configured with `azureADOnlyAuthentication: true`
- **Least privilege** – Managed identity has only `db_datareader`, `db_datawriter`, and `EXECUTE` permissions
- **Stored procedures only** – Application code never executes raw SQL or accesses tables directly
- **HTTPS enforced** – App Service configured with `httpsOnly: true` and TLS 1.2+
- **Managed Identity** – User-assigned identity scoped to the resource group

## Deployment Order

```
deploy-infra.sh                    deploy-app.sh
    │                                  │
    ├─ 1. Resource Group               ├─ 1. Install Python deps
    ├─ 2. Bicep: managed identity      ├─ 2. Import schema (run-sql.py)
    ├─ 2. Bicep: Azure SQL             ├─ 3. DB roles (run-sql-dbrole.py)
    ├─ 2. Bicep: App Service           ├─ 4. Stored procs (run-sql-stored-procs.py)
    ├─ 3. App Service settings         ├─ 5. dotnet publish
    └─ 4. SQL firewall rules           └─ 6. az webapp deploy
```
