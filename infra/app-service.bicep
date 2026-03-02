// app-service.bicep
// Deploys an Azure App Service (Plan + Web App) in UK South with a user-assigned
// managed identity so the application can authenticate to Azure SQL without passwords.

@description('Azure region for deployment')
param location string = 'uksouth'

@description('Resource ID of the user-assigned managed identity')
param managedIdentityId string

@description('Client ID of the managed identity (passed to app as AZURE_CLIENT_ID)')
param managedIdentityClientId string

@description('Principal ID of the managed identity')
param managedIdentityPrincipalId string

@description('Connection string for Azure SQL Database')
param sqlConnectionString string = ''

// Unique suffix derived from resource group ID (stable, lower-case)
var uniqueSuffix = toLower(uniqueString(resourceGroup().id))
var appServicePlanName = 'asp-expenseapp-${uniqueSuffix}'
var webAppName        = 'app-expenseapp-${uniqueSuffix}'

// App Service Plan – Standard S1 avoids cold-start issues
resource appServicePlan 'Microsoft.Web/serverfarms@2022-09-01' = {
  name: appServicePlanName
  location: location
  sku: {
    name: 'S1'
    tier: 'Standard'
  }
  properties: {
    reserved: false  // Windows plan (ASP.NET)
  }
  tags: {
    application: 'ExpenseApp'
    environment: 'dev'
  }
}

// Web App with user-assigned managed identity
resource webApp 'Microsoft.Web/sites@2022-09-01' = {
  name: webAppName
  location: location
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${managedIdentityId}': {}
    }
  }
  properties: {
    serverFarmId: appServicePlan.id
    httpsOnly: true
    siteConfig: {
      netFrameworkVersion: 'v8.0'
      appSettings: [
        {
          name: 'AZURE_CLIENT_ID'
          value: managedIdentityClientId
        }
        {
          name: 'ASPNETCORE_ENVIRONMENT'
          value: 'Production'
        }
      ]
      connectionStrings: sqlConnectionString != '' ? [
        {
          name: 'DefaultConnection'
          connectionString: sqlConnectionString
          type: 'SQLAzure'
        }
      ] : []
    }
  }
  tags: {
    application: 'ExpenseApp'
    environment: 'dev'
  }
}

// Outputs consumed by deployment scripts and other modules
@description('Name of the deployed App Service')
output appServiceName string = webApp.name

@description('Default HTTPS URL of the App Service')
output appServiceUrl string = 'https://${webApp.properties.defaultHostName}'

@description('Principal ID of the assigned managed identity')
output managedIdentityPrincipalId string = managedIdentityPrincipalId

@description('Client ID of the assigned managed identity')
output managedIdentityClientId string = managedIdentityClientId
