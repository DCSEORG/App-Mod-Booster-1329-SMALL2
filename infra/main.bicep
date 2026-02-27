// main.bicep
// Orchestrates the deployment of all Azure infrastructure for the Expenses Management App

@description('Azure region for deployment')
param location string = 'uksouth'

@description('Object ID of the deployer for Entra ID SQL admin')
param adminObjectId string

@description('UPN / login name of the deployer for Entra ID SQL admin')
param adminLogin string

// ── Managed Identity ─────────────────────────────────────────────────────────
module managedIdentity 'managed-identity.bicep' = {
  name: 'managedIdentityDeploy'
  params: {
    location: location
  }
}

// ── Azure SQL Database ────────────────────────────────────────────────────────
module sqlDatabase 'sql-database.bicep' = {
  name: 'sqlDatabaseDeploy'
  params: {
    location: location
    adminObjectId: adminObjectId
    adminLogin: adminLogin
    managedIdentityPrincipalId: managedIdentity.outputs.managedIdentityPrincipalId
  }
}

// ── App Service ───────────────────────────────────────────────────────────────
module appService 'app-service.bicep' = {
  name: 'appServiceDeploy'
  params: {
    location: location
    managedIdentityId: managedIdentity.outputs.managedIdentityId
    managedIdentityPrincipalId: managedIdentity.outputs.managedIdentityPrincipalId
    managedIdentityClientId: managedIdentity.outputs.managedIdentityClientId
    sqlServerFqdn: sqlDatabase.outputs.sqlServerFqdn
    databaseName: sqlDatabase.outputs.databaseName
  }
}

// ── Outputs ───────────────────────────────────────────────────────────────────
@description('Resource ID of the managed identity')
output managedIdentityId string = managedIdentity.outputs.managedIdentityId

@description('Client ID of the managed identity')
output managedIdentityClientId string = managedIdentity.outputs.managedIdentityClientId

@description('Principal ID of the managed identity')
output managedIdentityPrincipalId string = managedIdentity.outputs.managedIdentityPrincipalId

@description('Name of the managed identity')
output managedIdentityName string = managedIdentity.outputs.managedIdentityName

@description('SQL Server name')
output sqlServerName string = sqlDatabase.outputs.sqlServerName

@description('SQL Server FQDN')
output sqlServerFqdn string = sqlDatabase.outputs.sqlServerFqdn

@description('Database name')
output databaseName string = sqlDatabase.outputs.databaseName

@description('App Service name')
output appServiceName string = appService.outputs.appServiceName

@description('App Service URL')
output appServiceUrl string = appService.outputs.appServiceUrl
