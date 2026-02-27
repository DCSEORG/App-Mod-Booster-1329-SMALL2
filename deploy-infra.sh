#!/usr/bin/env bash
# deploy-infra.sh
# Deploys all Azure infrastructure for the Expenses Management app:
#   1. Resource group
#   2. Bicep templates (managed identity, App Service, Azure SQL)
#   3. App Service configuration (connection string, managed identity)
#   4. SQL firewall rules
#
# Usage: bash deploy-infra.sh
# Prereqs: az login, Bicep CLI (az bicep install)

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

# ── Load shared variables ─────────────────────────────────────────────────────
# shellcheck source=AgentVariables.sh
source "$SCRIPT_DIR/AgentVariables.sh"

echo "═══════════════════════════════════════════════════════════"
echo "  Expenses App – Infrastructure Deployment"
echo "═══════════════════════════════════════════════════════════"

# ── Validate required variables ───────────────────────────────────────────────
if [[ -z "${ADMIN_OBJECT_ID:-}" ]]; then
    echo "✗  ADMIN_OBJECT_ID is not set in AgentVariables.sh"
    echo "   Run: az ad signed-in-user show --query id -o tsv"
    exit 1
fi

if [[ -z "${ADMIN_LOGIN:-}" ]]; then
    echo "✗  ADMIN_LOGIN is not set in AgentVariables.sh"
    echo "   Run: az account show --query user.name -o tsv"
    exit 1
fi

# ── Set subscription ──────────────────────────────────────────────────────────
if [[ -n "${SUBSCRIPTION_ID:-}" ]]; then
    echo "Setting subscription: $SUBSCRIPTION_ID"
    az account set --subscription "$SUBSCRIPTION_ID"
fi

# ── 1. Create resource group ──────────────────────────────────────────────────
echo ""
echo "1/4 Creating resource group: $RESOURCE_GROUP ($LOCATION)…"
az group create \
    --name     "$RESOURCE_GROUP" \
    --location "$LOCATION" \
    --output   none
echo "  ✓  Resource group ready"

# ── 2. Deploy Bicep templates ─────────────────────────────────────────────────
echo ""
echo "2/4 Deploying Bicep templates…"
DEPLOYMENT_OUTPUT=$(az deployment group create \
    --resource-group  "$RESOURCE_GROUP" \
    --template-file   "$SCRIPT_DIR/infra/main.bicep" \
    --parameters      adminObjectId="$ADMIN_OBJECT_ID" \
                      adminLogin="$ADMIN_LOGIN" \
    --output          json)

# Parse outputs
MANAGED_IDENTITY_NAME=$(echo "$DEPLOYMENT_OUTPUT"       | python3 -c "import sys,json; d=json.load(sys.stdin); print(d['properties']['outputs']['managedIdentityName']['value'])")
MANAGED_IDENTITY_CLIENT_ID=$(echo "$DEPLOYMENT_OUTPUT"  | python3 -c "import sys,json; d=json.load(sys.stdin); print(d['properties']['outputs']['managedIdentityClientId']['value'])")
MANAGED_IDENTITY_PRINCIPAL_ID=$(echo "$DEPLOYMENT_OUTPUT" | python3 -c "import sys,json; d=json.load(sys.stdin); print(d['properties']['outputs']['managedIdentityPrincipalId']['value'])")
SQL_SERVER_FQDN=$(echo "$DEPLOYMENT_OUTPUT"             | python3 -c "import sys,json; d=json.load(sys.stdin); print(d['properties']['outputs']['sqlServerFqdn']['value'])")
SQL_SERVER_NAME=$(echo "$DEPLOYMENT_OUTPUT"             | python3 -c "import sys,json; d=json.load(sys.stdin); print(d['properties']['outputs']['sqlServerName']['value'])")
APP_SERVICE_NAME=$(echo "$DEPLOYMENT_OUTPUT"            | python3 -c "import sys,json; d=json.load(sys.stdin); print(d['properties']['outputs']['appServiceName']['value'])")
APP_SERVICE_URL=$(echo "$DEPLOYMENT_OUTPUT"             | python3 -c "import sys,json; d=json.load(sys.stdin); print(d['properties']['outputs']['appServiceUrl']['value'])")

echo "  ✓  Managed Identity : $MANAGED_IDENTITY_NAME"
echo "  ✓  SQL Server       : $SQL_SERVER_FQDN"
echo "  ✓  App Service      : $APP_SERVICE_NAME"

# Export for use by deploy-app.sh when called in same session
export MANAGED_IDENTITY_NAME MANAGED_IDENTITY_CLIENT_ID MANAGED_IDENTITY_PRINCIPAL_ID
export SQL_SERVER_FQDN SQL_SERVER_NAME SQL_DATABASE APP_SERVICE_NAME APP_SERVICE_URL

# ── 3. Configure App Service ──────────────────────────────────────────────────
echo ""
echo "3/4 Configuring App Service settings…"

CONNECTION_STRING="Server=tcp:${SQL_SERVER_FQDN},1433;Database=${SQL_DATABASE};Authentication=Active Directory Managed Identity;User Id=${MANAGED_IDENTITY_CLIENT_ID};Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"

az webapp config appsettings set \
    --resource-group "$RESOURCE_GROUP" \
    --name           "$APP_SERVICE_NAME" \
    --settings       AZURE_CLIENT_ID="$MANAGED_IDENTITY_CLIENT_ID" \
    --output         none

az webapp config connection-string set \
    --resource-group "$RESOURCE_GROUP" \
    --name           "$APP_SERVICE_NAME" \
    --connection-string-type SQLAzure \
    --settings       DefaultConnection="$CONNECTION_STRING" \
    --output         none

echo "  ✓  App Service configured"

# ── 4. SQL Firewall rules ─────────────────────────────────────────────────────
echo ""
echo "4/4 Waiting for SQL Server to be ready…"
# Poll SQL Server availability instead of fixed sleep
SQL_READY=0
for i in $(seq 1 10); do
    if az sql server show --resource-group "$RESOURCE_GROUP" --name "$SQL_SERVER_NAME" \
        --query "state" -o tsv 2>/dev/null | grep -q "Ready"; then
        SQL_READY=1
        echo "  ✓  SQL Server is ready (attempt $i)"
        break
    fi
    echo "  ⏳ Waiting for SQL Server... attempt $i/10 (30s)"
    sleep 30
done

if [[ $SQL_READY -eq 0 ]]; then
    echo "  ⚠️  SQL Server not confirmed ready – proceeding anyway"
fi

echo "  Configuring SQL firewall rules…"

# Allow Azure services (0.0.0.0 – 0.0.0.0)
az sql server firewall-rule create \
    --resource-group  "$RESOURCE_GROUP" \
    --server          "$SQL_SERVER_NAME" \
    --name            "AllowAllAzureIPs" \
    --start-ip-address 0.0.0.0 \
    --end-ip-address   0.0.0.0 \
    --output           none

# Allow the deployment machine's current public IP
MY_IP=$(curl -s https://api.ipify.org)
echo "  Deployment IP: $MY_IP"
az sql server firewall-rule create \
    --resource-group   "$RESOURCE_GROUP" \
    --server           "$SQL_SERVER_NAME" \
    --name             "AllowDeploymentIP" \
    --start-ip-address "$MY_IP" \
    --end-ip-address   "$MY_IP" \
    --output           none

echo "  ✓  SQL firewall rules configured"

# ── Summary ───────────────────────────────────────────────────────────────────
echo ""
echo "═══════════════════════════════════════════════════════════"
echo "  ✓  Infrastructure deployment complete"
echo "═══════════════════════════════════════════════════════════"
echo "  Managed Identity : $MANAGED_IDENTITY_NAME"
echo "  SQL Server FQDN  : $SQL_SERVER_FQDN"
echo "  App Service      : $APP_SERVICE_NAME"
echo "  App URL          : $APP_SERVICE_URL/Index"
echo ""
echo "  Next step: bash deploy-app.sh"
echo "═══════════════════════════════════════════════════════════"
