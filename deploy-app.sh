#!/usr/bin/env bash
# deploy-app.sh
# Deploys database objects and application code for the Expense Management app.
#
# Prerequisites:
#   - deploy-infra.sh has been run successfully
#   - SQL_SERVER_FQDN, APP_SERVICE_NAME, MANAGED_IDENTITY_NAME, AZURE_CLIENT_ID
#     are exported (or set in AgentVariables.sh)
#   - Azure CLI logged in and az account set to the correct subscription
#
# Usage:
#   bash deploy-app.sh
# ---------------------------------------------------------------------------
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
source "$SCRIPT_DIR/AgentVariables.sh"

echo "=================================================="
echo "  Expense Management – Application Deployment"
echo "=================================================="
echo "  SQL Server           : ${SQL_SERVER_FQDN}"
echo "  App Service          : ${APP_SERVICE_NAME}"
echo "  Managed Identity     : ${MANAGED_IDENTITY_NAME}"
echo "=================================================="

# ---------------------------------------------------------------------------
# 1. Install Python dependencies
# ---------------------------------------------------------------------------
echo ""
echo "🐍 [1/6] Installing Python dependencies..."
pip3 install --quiet pyodbc azure-identity
echo "  ✓ pyodbc and azure-identity installed"

# ---------------------------------------------------------------------------
# 2. Import database schema
# ---------------------------------------------------------------------------
echo ""
echo "🗄️  [2/6] Importing database schema..."
python3 "$SCRIPT_DIR/run-sql.py"
echo "  ✓ Schema imported"

# ---------------------------------------------------------------------------
# 3. Configure database roles for managed identity
# ---------------------------------------------------------------------------
echo ""
echo "👤 [3/6] Configuring database roles for managed identity..."
python3 "$SCRIPT_DIR/run-sql-dbrole.py"
echo "  ✓ Database roles configured"

# ---------------------------------------------------------------------------
# 4. Deploy stored procedures
# ---------------------------------------------------------------------------
echo ""
echo "📦 [4/6] Deploying stored procedures..."
python3 "$SCRIPT_DIR/run-sql-stored-procs.py"
echo "  ✓ Stored procedures deployed"

# ---------------------------------------------------------------------------
# 5. Build and zip the application
# ---------------------------------------------------------------------------
echo ""
echo "🔨 [5/6] Building and packaging application..."

cd "$SCRIPT_DIR/src/ExpenseApp"
dotnet restore
dotnet publish -c Release -o "$SCRIPT_DIR/publish" --nologo

cd "$SCRIPT_DIR/publish"
# Files must be at ZIP root (not in subdirectory) for Azure App Service deployment
# Include only runtime-necessary files: DLLs, configs, static files
zip -r "$SCRIPT_DIR/app.zip" . \
  --exclude "*.pdb" \
  --exclude "*.xml" \
  --exclude "*.development.json" \
  2>/dev/null || true

echo "  ✓ app.zip created ($(du -sh "$SCRIPT_DIR/app.zip" | cut -f1))"

# ---------------------------------------------------------------------------
# 6. Deploy to App Service
# ---------------------------------------------------------------------------
echo ""
echo "🚀 [6/6] Deploying to App Service: ${APP_SERVICE_NAME}..."

az webapp deploy \
  --resource-group "$RESOURCE_GROUP" \
  --name           "$APP_SERVICE_NAME" \
  --src-path       "$SCRIPT_DIR/app.zip" \
  --type           zip \
  --async          false

echo "  ✓ Deployment complete"

# ---------------------------------------------------------------------------
# Done
# ---------------------------------------------------------------------------
APP_URL=$(az webapp show \
  --resource-group "$RESOURCE_GROUP" \
  --name           "$APP_SERVICE_NAME" \
  --query          "defaultHostName" \
  --output         tsv)

echo ""
echo "✅ Application deployed successfully!"
echo ""
echo "   🌐 App URL : https://${APP_URL}/Index"
echo ""
echo "   ℹ️  If you see 'showing sample data' in the app header,"
echo "      check that AZURE_CLIENT_ID matches the managed identity Client ID"
echo "      and that the SQL firewall allows the App Service outbound IP."
