// managed-identity.bicep
// Creates a user-assigned managed identity for the Expense Management application.
// The managed identity is used by App Service to authenticate to Azure SQL Database
// without storing credentials.

@description('Azure region for deployment')
param location string = resourceGroup().location

// Unique suffix derived from resource group ID (stable across deployments)
var uniqueSuffix = uniqueString(resourceGroup().id)
var identityName = 'mid-AppModAssist-${uniqueSuffix}'

resource managedIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: identityName
  location: location
  tags: {
    application: 'ExpenseApp'
    environment: 'dev'
  }
}

// Outputs for use in other Bicep modules and deployment scripts
@description('Resource ID of the managed identity')
output managedIdentityId string = managedIdentity.id

@description('Client ID of the managed identity (used as AZURE_CLIENT_ID)')
output managedIdentityClientId string = managedIdentity.properties.clientId

@description('Principal ID of the managed identity (used for role assignments)')
output managedIdentityPrincipalId string = managedIdentity.properties.principalId
