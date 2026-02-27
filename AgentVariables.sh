#!/usr/bin/env bash
# AgentVariables.sh
# Shared variables for deploy-infra.sh and deploy-app.sh
# Edit these values before running the deployment scripts.

# ── Azure subscription / resource group ──────────────────────────────────────
export RESOURCE_GROUP="rg-appmodassist"
export LOCATION="uksouth"
export SUBSCRIPTION_ID=""          # Leave empty to use the current subscription

# ── Entra ID deployer credentials (used as Azure SQL admin) ──────────────────
# Run: az ad signed-in-user show --query id -o tsv
export ADMIN_OBJECT_ID=""          # Your Entra ID Object ID
# Run: az account show --query user.name -o tsv
export ADMIN_LOGIN=""              # Your Entra ID UPN (e.g. you@company.com)

# ── Deployment outputs (populated by deploy-infra.sh) ────────────────────────
# These are set automatically – do not edit manually.
export MANAGED_IDENTITY_NAME=""
export MANAGED_IDENTITY_CLIENT_ID=""
export MANAGED_IDENTITY_PRINCIPAL_ID=""
export SQL_SERVER_FQDN=""
export SQL_DATABASE="Northwind"
export APP_SERVICE_NAME=""
export APP_SERVICE_URL=""
