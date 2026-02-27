// app-service.bicep
// Creates an App Service Plan and App Service with user-assigned managed identity

@description('Azure region for deployment')
param location string = 'uksouth'

@description('Resource ID of the user-assigned managed identity')
param managedIdentityId string

@description('Principal ID of the user-assigned managed identity')
param managedIdentityPrincipalId string

@description('Client ID of the user-assigned managed identity')
param managedIdentityClientId string

@description('SQL Server FQDN for connection string')
param sqlServerFqdn string = ''

@description('Database name')
param databaseName string = 'Northwind'

@description('Unique suffix based on resource group ID')
var uniqueSuffix = uniqueString(resourceGroup().id)

@description('App Service Plan name')
var appServicePlanName = 'asp-appmodassist-${uniqueSuffix}'

@description('App Service name')
var appServiceName = 'app-appmodassist-${uniqueSuffix}'

resource appServicePlan 'Microsoft.Web/serverfarms@2022-09-01' = {
  name: appServicePlanName
  location: location
  sku: {
    name: 'S1'
    tier: 'Standard'
    capacity: 1
  }
  kind: 'app'
  properties: {
    reserved: false
  }
}

resource appService 'Microsoft.Web/sites@2022-09-01' = {
  name: appServiceName
  location: location
  kind: 'app'
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
      minTlsVersion: '1.2'
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
      connectionStrings: !empty(sqlServerFqdn) ? [
        {
          name: 'DefaultConnection'
          connectionString: 'Server=tcp:${sqlServerFqdn},1433;Database=${databaseName};Authentication=Active Directory Managed Identity;User Id=${managedIdentityClientId};Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;'
          type: 'SQLAzure'
        }
      ] : []
    }
  }
}

@description('Name of the App Service')
output appServiceName string = appService.name

@description('Default hostname of the App Service')
output appServiceUrl string = 'https://${appService.properties.defaultHostName}'

@description('Principal ID of the managed identity assigned to the App Service')
output managedIdentityPrincipalId string = managedIdentityPrincipalId

@description('Client ID of the managed identity assigned to the App Service')
output managedIdentityClientId string = managedIdentityClientId
