"""Configure the prepared host from non-secret Bicep outputs."""
import json
import os
from pathlib import Path
import re
import subprocess
import sys
import urllib.parse

config = json.loads(Path(sys.argv[1]).read_text())
origin = urllib.parse.urlsplit(config["origin"])
if origin.scheme != "https" or origin.path or origin.query or origin.fragment or not re.fullmatch(r"[a-z0-9.-]+", origin.netloc):
    raise ValueError("Expected an HTTPS DNS origin")
root = Path("/opt/studio")
env = []
for key, value in config["environment"].items():
    if not re.fullmatch(r"[A-Za-z_][A-Za-z0-9_]*", key) or any(c in str(value) for c in "\r\n\x00"):
        raise ValueError("Invalid environment value")
    # systemd EnvironmentFile quoting, not shell evaluation.
    escaped = str(value).replace("\\", "\\\\").replace('"', '\\"')
    env.append(f'{key}="{escaped}"')
environment = root / "config/production.env"
environment.write_text("\n".join(env) + "\n")
subprocess.run(["chown", "root:qbs", str(environment)], check=True)
os.chmod(environment, 0o640)
(root / "config/deployment.json").write_text(json.dumps({key: config[key] for key in ["origin", "storage", "clientId"]}))
Path("/etc/caddy/Caddyfile").write_text('''HOSTNAME {
    encode gzip
    header X-Content-Type-Options nosniff
    handle /api/* {
        reverse_proxy 127.0.0.1:7444
    }
    redir /admin /admin/ 308
    redir /client /client/ 308
    handle_path /admin/* {
        root * /opt/studio/current/admin
        try_files {path} /index.html
        file_server
    }
    handle_path /client/* {
        root * /opt/studio/current/client
        try_files {path} /index.html
        file_server
    }
    handle {
        root * /opt/studio/current/marketing
        try_files {path} /index.html
        file_server
    }
}
'''.replace("HOSTNAME", origin.netloc))
subprocess.run(["caddy", "validate", "--config", "/etc/caddy/Caddyfile"], check=True)
subprocess.run(["systemctl", "restart", "caddy"], check=True)
if (root / "current").exists():
    from release import healthy
    subprocess.run(["systemctl", "restart", "qbs-api", "qbs-worker"], check=True)
    healthy(config["origin"])
