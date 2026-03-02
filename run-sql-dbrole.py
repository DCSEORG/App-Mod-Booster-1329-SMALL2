#!/usr/bin/env python3
"""
run-sql-dbrole.py
Grants the managed identity read/write/execute permissions inside ExpenseDB.
Before executing, the MANAGED-IDENTITY-NAME placeholder in script.sql is replaced
with the actual managed identity name supplied via the MANAGED_IDENTITY_NAME
environment variable (set by deploy-app.sh).

Cross-platform sed usage (Mac-compatible .bak extension):
    sed -i.bak "s/PATTERN/REPLACEMENT/g" file && rm -f file.bak

Dependencies (installed by deploy-app.sh):
    pip3 install pyodbc azure-identity
"""

import os
import struct
import subprocess
import shutil
import tempfile
import pyodbc
from azure.identity import AzureCliCredential

# ---------------------------------------------------------------------------
# Configuration
# ---------------------------------------------------------------------------
SERVER               = os.environ.get("SQL_SERVER_FQDN", "<your-sql-server>.database.windows.net")
DATABASE             = "ExpenseDB"
SQL_SCRIPT_FILE      = "script.sql"
MANAGED_IDENTITY_NAME = os.environ.get("MANAGED_IDENTITY_NAME", "mid-AppModAssist")
DRIVER               = "{ODBC Driver 18 for SQL Server}"

SQL_COPT_SS_ACCESS_TOKEN = 1256


def get_access_token() -> bytes:
    """Obtain an Azure SQL access token via the Azure CLI credential."""
    credential = AzureCliCredential()
    token = credential.get_token("https://database.windows.net/.default")
    token_bytes = token.token.encode("utf-16-le")
    return struct.pack(f"<I{len(token_bytes)}s", len(token_bytes), token_bytes)


def prepare_script(template_path: str, identity_name: str) -> str:
    """
    Copy the template SQL file to a temp file, replacing MANAGED-IDENTITY-NAME
    with the real identity name.  Returns the path to the temp file.
    Cross-platform: uses Python string replacement (no sed dependency).
    """
    with open(template_path, "r", encoding="utf-8") as f:
        content = f.read()

    content = content.replace("MANAGED-IDENTITY-NAME", identity_name)

    tmp = tempfile.NamedTemporaryFile(
        mode="w", suffix=".sql", delete=False, encoding="utf-8"
    )
    tmp.write(content)
    tmp.close()
    return tmp.name


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

    print(f"👤 Configuring DB role for managed identity: {MANAGED_IDENTITY_NAME}")
    prepared = prepare_script(script_file, MANAGED_IDENTITY_NAME)

    try:
        batches = parse_sql_file(prepared)
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
    finally:
        os.unlink(prepared)
        cursor.close()
        conn.close()

    print(f"\n✅ Done – {success} succeeded, {failure} failed out of {total} batches.")
    if failure > 0:
        raise SystemExit(1)


if __name__ == "__main__":
    run_sql_script(SERVER, DATABASE, SQL_SCRIPT_FILE)
