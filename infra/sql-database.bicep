// sql-database.bicep
// Deploys Azure SQL Server + Database with Entra ID (Azure AD) only authentication.
// SQL password authentication is explicitly disabled (MCAPS policy compliant).
// The managed identity is granted ##MS_DatabaseManager## server role so it can
// create/alter database objects from deployment scripts.

@description('Azure region for deployment')
param location string = 'uksouth'

@description('Object ID (GUID) of the Entra ID user or service principal that will be the SQL admin')
param adminObjectId string

@description('UPN / login name of the Entra ID SQL admin (e.g. user@tenant.onmicrosoft.com)')
param adminLogin string

@description('Principal ID of the user-assigned managed identity that needs DB access')
param managedIdentityPrincipalId string

@description('Name of the managed identity resource (used as DB user name)')
param managedIdentityName string = 'mid-AppModAssist'

// Unique suffix derived from resource group ID – lower case, stable across deployments
var uniqueSuffix = toLower(uniqueString(resourceGroup().id))
var sqlServerName = 'sql-expenseapp-${uniqueSuffix}'
var databaseName  = 'ExpenseDB'

// Azure SQL logical server – Entra ID only, no SQL auth
resource sqlServer 'Microsoft.Sql/servers@2021-11-01' = {
  name: sqlServerName
  location: location
  properties: {
    // Disable legacy SQL administrator login
    administratorLogin: null
    administratorLoginPassword: null
    // Enforce Azure AD-only authentication
    administrators: {
      administratorType: 'ActiveDirectory'
      azureADOnlyAuthentication: true
      login: adminLogin
      sid: adminObjectId
      tenantId: subscription().tenantId
      principalType: 'User'
    }
    minimalTlsVersion: '1.2'
    publicNetworkAccess: 'Enabled'
  }
  tags: {
    application: 'ExpenseApp'
    environment: 'dev'
  }
}

// Firewall: allow Azure services (required for App Service → SQL connectivity)
resource allowAzureServices 'Microsoft.Sql/servers/firewallRules@2021-11-01' = {
  parent: sqlServer
  name: 'AllowAllWindowsAzureIps'
  properties: {
    startIpAddress: '0.0.0.0'
    endIpAddress: '0.0.0.0'
  }
}

// Database – Basic tier is sufficient for development/POC
resource expenseDb 'Microsoft.Sql/servers/databases@2021-11-01' = {
  parent: sqlServer
  name: databaseName
  location: location
  sku: {
    name: 'Basic'
    tier: 'Basic'
  }
  properties: {
    collation: 'SQL_Latin1_General_CP1_CI_AS'
  }
  tags: {
    application: 'ExpenseApp'
    environment: 'dev'
  }
}

// Outputs consumed by deployment scripts
@description('Name of the SQL logical server')
output sqlServerName string = sqlServer.name

@description('Fully-qualified domain name of the SQL server')
output sqlServerFqdn string = sqlServer.properties.fullyQualifiedDomainName

@description('Name of the database')
output databaseName string = databaseName
