"""Create a shell payload from reviewed scripts and non-secret infrastructure outputs."""
import base64
import json
from pathlib import Path
import sys

outputs = json.loads(Path(sys.argv[1]).read_text())
host = outputs["host"]["value"]
script = ["#!/bin/bash", "set -euo pipefail", "install -d -m 755 /opt/studio/bin"]
for name in ["setup-host.sh", "services.py", "gateway.py", "configure-host.py", "release.py"]:
    # The infrastructure runner is Windows; normalize checkout CRLF before running Bash on Linux.
    encoded = base64.b64encode((Path(__file__).parent / "linux" / name).read_text(encoding="utf-8").encode()).decode()
    script.append(f"echo '{encoded}' | base64 --decode > /opt/studio/bin/{name}")
encoded = base64.b64encode(json.dumps(host).encode()).decode()
script += [f"echo '{encoded}' | base64 --decode > /opt/studio/host.json", "chmod 600 /opt/studio/host.json",
           "bash /opt/studio/bin/setup-host.sh", "python3 /opt/studio/bin/configure-host.py /opt/studio/host.json"]
Path(sys.argv[2]).write_text("\n".join(script) + "\n")
