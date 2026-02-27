// sql-database.bicep
// Creates Azure SQL Server and Database with Entra ID-only authentication

@description('Azure region for deployment')
param location string = 'uksouth'

@description('Object ID of the deployer (Entra ID administrator)')
param adminObjectId string

@description('UPN / login name of the deployer (Entra ID administrator)')
param adminLogin string

@description('Principal ID of the managed identity that needs database access')
param managedIdentityPrincipalId string

@description('Unique suffix based on resource group ID')
var uniqueSuffix = uniqueString(resourceGroup().id)

@description('SQL Server name (lowercase)')
var sqlServerName = 'sql-appmodassist-${uniqueSuffix}'

@description('Database name')
var databaseNameVar = 'Northwind'

resource sqlServer 'Microsoft.Sql/servers@2021-11-01' = {
  name: sqlServerName
  location: location
  properties: {
    // Disable SQL (password) authentication; Entra ID only
    administrators: {
      administratorType: 'ActiveDirectory'
      principalType: 'User'
      login: adminLogin
      sid: adminObjectId
      tenantId: subscription().tenantId
      azureADOnlyAuthentication: true
    }
    minimalTlsVersion: '1.2'
    publicNetworkAccess: 'Enabled'
  }
}

resource sqlDatabase 'Microsoft.Sql/servers/databases@2021-11-01' = {
  parent: sqlServer
  name: databaseNameVar
  location: location
  sku: {
    name: 'Basic'
    tier: 'Basic'
    capacity: 5
  }
  properties: {
    collation: 'SQL_Latin1_General_CP1_CI_AS'
    maxSizeBytes: 2147483648
    zoneRedundant: false
  }
}

// Firewall rule to allow Azure services (0.0.0.0 - 0.0.0.0)
resource firewallAllowAzureServices 'Microsoft.Sql/servers/firewallRules@2021-11-01' = {
  parent: sqlServer
  name: 'AllowAllAzureIPs'
  properties: {
    startIpAddress: '0.0.0.0'
    endIpAddress: '0.0.0.0'
  }
}

@description('Name of the SQL Server')
output sqlServerName string = sqlServer.name

@description('Fully qualified domain name of the SQL Server')
output sqlServerFqdn string = sqlServer.properties.fullyQualifiedDomainName

@description('Name of the database')
output databaseName string = sqlDatabase.name
