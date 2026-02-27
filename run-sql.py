#!/usr/bin/env python3
"""
run-sql.py
Imports the database schema into Azure SQL using Azure AD authentication.
Uses AzureCliCredential so the deployer does not need a SQL password.
"""

import os
import sys
import re
import struct
import pyodbc
from azure.identity import AzureCliCredential

# ── Configuration ─────────────────────────────────────────────────────────────
SERVER   = os.environ.get("SQL_SERVER_FQDN", "")
DATABASE = os.environ.get("SQL_DATABASE",    "Northwind")
SQL_SCRIPT_FILE = "Database-Schema/database_schema.sql"

if not SERVER:
    print("✗  SQL_SERVER_FQDN environment variable is not set.")
    sys.exit(1)

# ── Acquire Azure AD token ────────────────────────────────────────────────────
print("Acquiring Azure AD token via Azure CLI…")
credential = AzureCliCredential()
token = credential.get_token("https://database.windows.net/.default")

# Pack the token into the binary format expected by the ODBC driver
token_bytes = token.token.encode("utf-16-le")
token_struct = struct.pack(f"<I{len(token_bytes)}s", len(token_bytes), token_bytes)

# ── Connect ───────────────────────────────────────────────────────────────────
connection_string = (
    f"DRIVER={{ODBC Driver 18 for SQL Server}};"
    f"SERVER={SERVER},1433;"
    f"DATABASE={DATABASE};"
    f"Encrypt=yes;"
    f"TrustServerCertificate=no;"
    f"Connection Timeout=30;"
)

print(f"Connecting to {SERVER}/{DATABASE}…")
SQL_COPT_SS_ACCESS_TOKEN = 1256
conn = pyodbc.connect(
    connection_string,
    attrs_before={SQL_COPT_SS_ACCESS_TOKEN: token_struct}
)
conn.autocommit = True
cursor = conn.cursor()

# ── Read and parse SQL file ───────────────────────────────────────────────────
print(f"Reading SQL script: {SQL_SCRIPT_FILE}")
with open(SQL_SCRIPT_FILE, "r", encoding="utf-8") as f:
    sql_content = f.read()

# Split on GO statements (batch separator) – handle both Unix (\n) and Windows (\r\n) line endings
sql_content = sql_content.replace('\r\n', '\n')
batches = [b.strip() for b in re.split(r'\nGO\b', sql_content, flags=re.IGNORECASE) if b.strip()]

print(f"Executing {len(batches)} SQL batch(es)…")
errors = 0
for i, batch in enumerate(batches, start=1):
    if not batch or batch.upper() == "GO":
        continue
    try:
        cursor.execute(batch)
        print(f"  ✓  Batch {i}/{len(batches)}")
    except pyodbc.Error as exc:
        print(f"  ✗  Batch {i}/{len(batches)}: {exc}")
        errors += 1

cursor.close()
conn.close()

if errors:
    print(f"\n✗  Completed with {errors} error(s).")
    sys.exit(1)
else:
    print("\n✓  Schema imported successfully.")
