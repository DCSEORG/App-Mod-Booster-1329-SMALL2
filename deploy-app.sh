#!/usr/bin/env bash
# deploy-app.sh
# Deploys database objects and application code:
#   1. Install Python dependencies
#   2. Import database schema
#   3. Configure database roles for managed identity
#   4. Deploy stored procedures
#   5. Build and publish ASP.NET application
#   6. Zip and deploy to App Service
#
# Usage: bash deploy-app.sh
# Prereqs: az login, .NET 8 SDK, python3

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

# ── Load shared variables ─────────────────────────────────────────────────────
# shellcheck source=AgentVariables.sh
source "$SCRIPT_DIR/AgentVariables.sh"

echo "═══════════════════════════════════════════════════════════"
echo "  Expenses App – Application Deployment"
echo "═══════════════════════════════════════════════════════════"

# ── Validate required variables ───────────────────────────────────────────────
if [[ -z "${SQL_SERVER_FQDN:-}" ]]; then
    echo "✗  SQL_SERVER_FQDN is not set."
    echo "   Run deploy-infra.sh first, or set the variables in AgentVariables.sh"
    exit 1
fi

if [[ -z "${MANAGED_IDENTITY_NAME:-}" ]]; then
    echo "✗  MANAGED_IDENTITY_NAME is not set."
    echo "   Run deploy-infra.sh first, or set the variables in AgentVariables.sh"
    exit 1
fi

if [[ -z "${APP_SERVICE_NAME:-}" ]]; then
    echo "✗  APP_SERVICE_NAME is not set."
    echo "   Run deploy-infra.sh first, or set the variables in AgentVariables.sh"
    exit 1
fi

# Export variables needed by the Python scripts
export SQL_SERVER_FQDN
export SQL_DATABASE="${SQL_DATABASE:-Northwind}"
export MANAGED_IDENTITY_NAME

cd "$SCRIPT_DIR"

# ── 1. Install Python dependencies ────────────────────────────────────────────
echo ""
echo "1/6 Installing Python dependencies…"
pip3 install --quiet pyodbc azure-identity
echo "  ✓  pyodbc and azure-identity installed"

# ── 2. Import database schema ─────────────────────────────────────────────────
echo ""
echo "2/6 Importing database schema…"
echo "  Waiting 30 s for SQL Server readiness…"
sleep 30
python3 run-sql.py
echo "  ✓  Schema imported"

# ── 3. Configure database roles ───────────────────────────────────────────────
echo ""
echo "3/6 Configuring database roles for managed identity: $MANAGED_IDENTITY_NAME…"
python3 run-sql-dbrole.py
echo "  ✓  Database roles configured"

# ── 4. Deploy stored procedures ───────────────────────────────────────────────
echo ""
echo "4/6 Deploying stored procedures…"
python3 run-sql-stored-procs.py
echo "  ✓  Stored procedures deployed"

# ── 5. Build ASP.NET application ──────────────────────────────────────────────
echo ""
echo "5/6 Building ASP.NET application…"
dotnet publish src/ExpensesApp/ExpensesApp.csproj \
    --configuration Release \
    --output        "$SCRIPT_DIR/publish" \
    --nologo
echo "  ✓  Application built"

# ── 6. Zip and deploy to App Service ─────────────────────────────────────────
echo ""
echo "6/6 Packaging and deploying to App Service: $APP_SERVICE_NAME…"

# Create zip with files at root (not in subdirectory)
cd "$SCRIPT_DIR/publish"
zip -r "$SCRIPT_DIR/app.zip" . -x "*.pdb"
cd "$SCRIPT_DIR"

echo "  Deploying app.zip to App Service…"
az webapp deploy \
    --resource-group "$RESOURCE_GROUP" \
    --name           "$APP_SERVICE_NAME" \
    --src-path       "$SCRIPT_DIR/app.zip" \
    --type           zip \
    --output         none

echo "  ✓  Application deployed"

# ── Summary ───────────────────────────────────────────────────────────────────
echo ""
echo "═══════════════════════════════════════════════════════════"
echo "  ✓  Application deployment complete"
echo "═══════════════════════════════════════════════════════════"
echo ""
echo "  🌍 App URL    : ${APP_SERVICE_URL:-https://$APP_SERVICE_NAME.azurewebsites.net}/Index"
echo "  🔌 API docs   : ${APP_SERVICE_URL:-https://$APP_SERVICE_NAME.azurewebsites.net}/swagger"
echo ""
echo "  Note: Allow 1-2 minutes for the app to warm up on first request."
echo "═══════════════════════════════════════════════════════════"
