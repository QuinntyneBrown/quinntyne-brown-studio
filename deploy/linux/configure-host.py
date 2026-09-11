"""Configure the prepared host from non-secret Bicep outputs."""
import json
import os
from pathlib import Path
import re
import subprocess
import sys

sys.path.insert(0, str(Path(__file__).resolve().parent))
import gateway

config = json.loads(Path(sys.argv[1]).read_text())
gateway.origin_of(config)
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
gateway.apply(config)
if (root / "current").exists():
    from release import healthy
    subprocess.run(["systemctl", "restart", "qbs-api", "qbs-worker"], check=True)
    healthy(config["origin"])
