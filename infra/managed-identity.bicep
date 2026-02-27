// managed-identity.bicep
// Creates a user-assigned managed identity for use by the App Service

@description('Azure region for deployment')
param location string = resourceGroup().location

@description('Unique suffix based on resource group ID')
var uniqueSuffix = uniqueString(resourceGroup().id)

@description('Managed identity resource name')
var managedIdentityName = 'mid-appmodassist-${uniqueSuffix}'

resource managedIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: managedIdentityName
  location: location
}

@description('Resource ID of the managed identity')
output managedIdentityId string = managedIdentity.id

@description('Client ID of the managed identity (used as AZURE_CLIENT_ID)')
output managedIdentityClientId string = managedIdentity.properties.clientId

@description('Principal ID of the managed identity (used for role assignments)')
output managedIdentityPrincipalId string = managedIdentity.properties.principalId

@description('Name of the managed identity')
output managedIdentityName string = managedIdentity.name
