#!/usr/bin/env bash
# deploy-infra.sh
# Deploys all Azure infrastructure for the Expense Management application.
#
# Prerequisites:
#   - Azure CLI installed and logged in (az login)
#   - ADMIN_OBJECT_ID and ADMIN_LOGIN set in AgentVariables.sh or as env vars
#
# Usage:
#   bash deploy-infra.sh
# ---------------------------------------------------------------------------
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
source "$SCRIPT_DIR/AgentVariables.sh"

# ---------------------------------------------------------------------------
# Validate required variables
# ---------------------------------------------------------------------------
if [[ -z "${ADMIN_OBJECT_ID:-}" ]]; then
  echo "ℹ️  ADMIN_OBJECT_ID not set – fetching from Azure CLI..."
  ADMIN_OBJECT_ID=$(az ad signed-in-user show --query id -o tsv)
fi

if [[ -z "${ADMIN_LOGIN:-}" ]]; then
  echo "ℹ️  ADMIN_LOGIN not set – fetching from Azure CLI..."
  ADMIN_LOGIN=$(az account show --query user.name -o tsv)
fi

echo "=================================================="
echo "  Expense Management – Infrastructure Deployment"
echo "=================================================="
echo "  Resource Group : $RESOURCE_GROUP"
echo "  Location       : $LOCATION"
echo "  SQL Admin      : $ADMIN_LOGIN ($ADMIN_OBJECT_ID)"
echo "=================================================="

# ---------------------------------------------------------------------------
# 1. Deploy resource group
# ---------------------------------------------------------------------------
echo ""
echo "📦 [1/5] Deploying resource group..."
az group create \
  --name     "$RESOURCE_GROUP" \
  --location "$LOCATION" \
  --output   table

# ---------------------------------------------------------------------------
# 2. Deploy Bicep templates
# ---------------------------------------------------------------------------
echo ""
echo "🔐 [2a/5] Deploying managed identity..."
MI_OUTPUT=$(az deployment group create \
  --resource-group "$RESOURCE_GROUP" \
  --template-file  "$SCRIPT_DIR/infra/managed-identity.bicep" \
  --query          "properties.outputs" \
  --output         json)

export MANAGED_IDENTITY_ID=$(echo "$MI_OUTPUT"           | jq -r '.managedIdentityId.value')
export AZURE_CLIENT_ID=$(echo "$MI_OUTPUT"               | jq -r '.managedIdentityClientId.value')
export MANAGED_IDENTITY_PRINCIPAL_ID=$(echo "$MI_OUTPUT" | jq -r '.managedIdentityPrincipalId.value')

# Derive the managed identity name from the resource ID (last segment)
export MANAGED_IDENTITY_NAME=$(echo "$MANAGED_IDENTITY_ID" | awk -F'/' '{print $NF}')

echo "  ✓ Managed Identity : $MANAGED_IDENTITY_NAME"
echo "  ✓ Client ID        : $AZURE_CLIENT_ID"

echo ""
echo "🌐 [2b/5] Deploying App Service..."
APP_OUTPUT=$(az deployment group create \
  --resource-group "$RESOURCE_GROUP" \
  --template-file  "$SCRIPT_DIR/infra/app-service.bicep" \
  --parameters \
      managedIdentityId="$MANAGED_IDENTITY_ID" \
      managedIdentityClientId="$AZURE_CLIENT_ID" \
      managedIdentityPrincipalId="$MANAGED_IDENTITY_PRINCIPAL_ID" \
  --query "properties.outputs" \
  --output json)

export APP_SERVICE_NAME=$(echo "$APP_OUTPUT" | jq -r '.appServiceName.value')
APP_URL=$(echo "$APP_OUTPUT"                  | jq -r '.appServiceUrl.value')

echo "  ✓ App Service : $APP_SERVICE_NAME"
echo "  ✓ App URL     : $APP_URL"

echo ""
echo "🗄️  [2c/5] Deploying Azure SQL..."
SQL_OUTPUT=$(az deployment group create \
  --resource-group "$RESOURCE_GROUP" \
  --template-file  "$SCRIPT_DIR/infra/sql-database.bicep" \
  --parameters \
      adminObjectId="$ADMIN_OBJECT_ID" \
      adminLogin="$ADMIN_LOGIN" \
      managedIdentityPrincipalId="$MANAGED_IDENTITY_PRINCIPAL_ID" \
      managedIdentityName="$MANAGED_IDENTITY_NAME" \
  --query "properties.outputs" \
  --output json)

export SQL_SERVER_FQDN=$(echo "$SQL_OUTPUT" | jq -r '.sqlServerFqdn.value')
SQL_SERVER_NAME=$(echo "$SQL_OUTPUT"         | jq -r '.sqlServerName.value')
DATABASE_NAME=$(echo "$SQL_OUTPUT"           | jq -r '.databaseName.value')

echo "  ✓ SQL Server : $SQL_SERVER_FQDN"
echo "  ✓ Database   : $DATABASE_NAME"

# ---------------------------------------------------------------------------
# 3. Configure App Service settings
# ---------------------------------------------------------------------------
echo ""
echo "⚙️  [3/5] Configuring App Service settings..."

CONNECTION_STRING="Server=tcp:${SQL_SERVER_FQDN},1433;Database=${DATABASE_NAME};Authentication=Active Directory Managed Identity;User Id=${AZURE_CLIENT_ID};"

az webapp config appsettings set \
  --resource-group "$RESOURCE_GROUP" \
  --name           "$APP_SERVICE_NAME" \
  --settings \
      AZURE_CLIENT_ID="$AZURE_CLIENT_ID" \
      SQL_SERVER_FQDN="$SQL_SERVER_FQDN" \
      ASPNETCORE_ENVIRONMENT="Production" \
  --output table

az webapp config connection-string set \
  --resource-group  "$RESOURCE_GROUP" \
  --name            "$APP_SERVICE_NAME" \
  --connection-string-type SQLAzure \
  --settings "DefaultConnection=$CONNECTION_STRING" \
  --output table

echo "  ✓ App settings updated"

# ---------------------------------------------------------------------------
# 4. Wait for SQL Server to be ready
# ---------------------------------------------------------------------------
echo ""
echo "⏳ [4/5] Waiting 30 seconds for SQL Server to become ready..."
sleep 30

# ---------------------------------------------------------------------------
# 5. Add IP to SQL firewall
# ---------------------------------------------------------------------------
echo ""
echo "🔒 [5/5] Configuring SQL firewall rules..."

MY_IP=$(curl -s https://api.ipify.org)
echo "  → Deployment IP: $MY_IP"

# Allow Azure services
az sql server firewall-rule create \
  --resource-group "$RESOURCE_GROUP" \
  --server         "$SQL_SERVER_NAME" \
  --name           "AllowAllAzureIPs" \
  --start-ip-address "0.0.0.0" \
  --end-ip-address   "0.0.0.0" \
  --output table

# Allow deployment machine
az sql server firewall-rule create \
  --resource-group    "$RESOURCE_GROUP" \
  --server            "$SQL_SERVER_NAME" \
  --name              "AllowDeploymentIP" \
  --start-ip-address  "$MY_IP" \
  --end-ip-address    "$MY_IP" \
  --output table

echo "  ✓ Firewall rules created"

# ---------------------------------------------------------------------------
# Done
# ---------------------------------------------------------------------------
echo ""
echo "✅ Infrastructure deployment complete!"
echo ""
echo "   App Service  : $APP_URL"
echo "   SQL Server   : $SQL_SERVER_FQDN"
echo "   Identity     : $MANAGED_IDENTITY_NAME"
echo ""
echo "   Next step: bash deploy-app.sh"
echo ""
echo "   💡 Export these for deploy-app.sh:"
echo "      export SQL_SERVER_FQDN=\"$SQL_SERVER_FQDN\""
echo "      export APP_SERVICE_NAME=\"$APP_SERVICE_NAME\""
echo "      export MANAGED_IDENTITY_NAME=\"$MANAGED_IDENTITY_NAME\""
echo "      export AZURE_CLIENT_ID=\"$AZURE_CLIENT_ID\""
