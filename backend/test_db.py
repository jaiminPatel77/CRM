import json
import subprocess
import os
import re

# ==============================================================================
# Crm - Database Connectivity Tester
# Reads appsettings.Production.json and tests connection via sqlcmd
# ==============================================================================

def test_connection():
    config_path = os.path.expanduser("~/crm-api/appsettings.json")
    
    if not os.path.exists(config_path):
        print(f"FAILED: Configuration file not found at {config_path}")
        return

    try:
        with open(config_path, 'r') as f:
            # Use regex to strip potential comments if any, though json.load is strict
            content = f.read()
            data = json.loads(content)
            
        conn_string = data.get("ConnectionStrings", {}).get("DefaultConnection", "")
        if not conn_string:
            print("FAILED: No 'DefaultConnection' found in configuration.")
            return

        # Simple Regex to extract key components
        server = re.search(r"Server=([^;]+)", conn_string, re.I)
        user = re.search(r"User Id=([^;]+)", conn_string, re.I)
        password = re.search(r"Password=([^;]+)", conn_string, re.I)

        if not all([server, user, password]):
            print("FAILED: Could not parse Server, User Id, or Password from connection string.")
            print(f"Current string: {conn_string}")
            return

        server = server.group(1)
        user = user.group(1)
        password = password.group(1)

        print(f"Testing connection to {server} as {user}...")

        # Run sqlcmd
        # -C (Trust Server Certificate) is often needed for local dev
        cmd = [
            "/opt/mssql-tools18/bin/sqlcmd",
            "-S", server,
            "-U", user,
            "-P", password,
            "-Q", "SELECT 'CONNECTION SUCCESSFUL!'",
            "-C"
        ]

        result = subprocess.run(cmd, capture_output=True, text=True)

        if result.returncode == 0:
            print("====================================================")
            print("✅ SUCCESS: Database connection established!")
            print("====================================================")
            print(result.stdout.strip())
        else:
            print("====================================================")
            print("❌ FAILED: Database connection failed.")
            print("====================================================")
            print("Error Details:")
            print(result.stderr or result.stdout)

    except Exception as e:
        print(f"ERROR: An unexpected error occurred: {e}")

if __name__ == "__main__":
    test_connection()
