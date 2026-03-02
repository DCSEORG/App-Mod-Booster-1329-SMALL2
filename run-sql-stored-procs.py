#!/usr/bin/env python3
"""
run-sql-stored-procs.py
Deploys the stored procedures defined in stored-procedures.sql into ExpenseDB.
Uses Azure AD (CLI) authentication – no passwords stored anywhere.

Dependencies (installed by deploy-app.sh):
    pip3 install pyodbc azure-identity
"""

import os
import struct
import pyodbc
from azure.identity import AzureCliCredential

# ---------------------------------------------------------------------------
# Configuration
# ---------------------------------------------------------------------------
SERVER          = os.environ.get("SQL_SERVER_FQDN", "<your-sql-server>.database.windows.net")
DATABASE        = "ExpenseDB"
SQL_SCRIPT_FILE = "stored-procedures.sql"
DRIVER          = "{ODBC Driver 18 for SQL Server}"

SQL_COPT_SS_ACCESS_TOKEN = 1256


def get_access_token() -> bytes:
    """Obtain an Azure SQL access token via the Azure CLI credential."""
    credential = AzureCliCredential()
    token = credential.get_token("https://database.windows.net/.default")
    token_bytes = token.token.encode("utf-16-le")
    return struct.pack(f"<I{len(token_bytes)}s", len(token_bytes), token_bytes)


def parse_sql_file(filepath: str) -> list[str]:
    """Split a SQL script on GO statements, returning non-empty batches."""
    with open(filepath, "r", encoding="utf-8") as f:
        content = f.read()
    batches = []
    for batch in content.split("\nGO"):
        stripped = batch.strip()
        if stripped:
            batches.append(stripped)
    return batches


def run_sql_script(server: str, database: str, script_file: str) -> None:
    """Execute all batches in *script_file* against *database* on *server*."""
    print(f"🔗 Connecting to {server} / {database} ...")
    token_struct = get_access_token()

    connection_string = (
        f"Driver={DRIVER};"
        f"Server=tcp:{server},1433;"
        f"Database={database};"
        "Encrypt=yes;"
        "TrustServerCertificate=no;"
        "Connection Timeout=30;"
    )

    conn = pyodbc.connect(
        connection_string,
        attrs_before={SQL_COPT_SS_ACCESS_TOKEN: token_struct},
    )
    conn.autocommit = True
    cursor = conn.cursor()

    print(f"📄 Deploying stored procedures from {script_file} ...")
    batches = parse_sql_file(script_file)
    total   = len(batches)
    success = 0
    failure = 0

    for idx, batch in enumerate(batches, start=1):
        preview = batch[:80].replace("\n", " ")
        try:
            cursor.execute(batch)
            print(f"  ✓ [{idx}/{total}] {preview}")
            success += 1
        except pyodbc.Error as exc:
            print(f"  ✗ [{idx}/{total}] {preview}")
            print(f"        ERROR: {exc}")
            failure += 1

    cursor.close()
    conn.close()

    print(f"\n✅ Done – {success} succeeded, {failure} failed out of {total} batches.")
    if failure > 0:
        raise SystemExit(1)


if __name__ == "__main__":
    run_sql_script(SERVER, DATABASE, SQL_SCRIPT_FILE)
