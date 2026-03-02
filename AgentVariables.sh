#!/usr/bin/env bash
# AgentVariables.sh
# Shared variables sourced by deploy-infra.sh and deploy-app.sh.
# Edit these values before running either deployment script.

# -----------------------------------------------------------------------
# Azure resource settings
# -----------------------------------------------------------------------
export RESOURCE_GROUP="rg-expenseapp-dev"
export LOCATION="uksouth"

# -----------------------------------------------------------------------
# Entra ID admin for Azure SQL (mandatory – SQL auth is disabled)
# Run: az ad signed-in-user show --query id -o tsv
#      az account show --query user.name -o tsv
# -----------------------------------------------------------------------
export ADMIN_OBJECT_ID="${ADMIN_OBJECT_ID:-}"   # deployer's Entra Object ID
export ADMIN_LOGIN="${ADMIN_LOGIN:-}"            # deployer's UPN e.g. user@tenant.onmicrosoft.com

# -----------------------------------------------------------------------
# Derived names (populated after infra deployment – do not edit manually)
# -----------------------------------------------------------------------
export SQL_SERVER_FQDN="${SQL_SERVER_FQDN:-}"
export APP_SERVICE_NAME="${APP_SERVICE_NAME:-}"
export MANAGED_IDENTITY_NAME="${MANAGED_IDENTITY_NAME:-}"
export AZURE_CLIENT_ID="${AZURE_CLIENT_ID:-}"
