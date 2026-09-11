#!/usr/bin/env python3
"""The studio's service definitions and the persistent directories they own.

Host preparation installs packages once; these definitions change with the code that runs under
them. The blog arrived with a release and needed a media directory and a storage path in the unit,
neither of which a release applied, so the API refused to start on a host prepared before it.
Every release now rewrites them from the reviewed definition here.
"""
import subprocess
import sys
from pathlib import Path

STATE = Path("/var/lib/studio")
# Persistent state the services own. Release artifacts are replaced on every deployment; these
# directories outlive them and are never part of a release.
DIRECTORIES = [STATE, STATE / "blog-media"]
ASSEMBLIES = {"api": "Api", "worker": "Worker"}
UNIT = """[Unit]
Description=Quinntyne Brown Studio {service}
After=network-online.target
Wants=network-online.target
[Service]
User=qbs
Group=qbs
EnvironmentFile=/opt/studio/config/production.env
Environment=Blog__StoragePath=/var/lib/studio/blog-media
WorkingDirectory=/opt/studio/current/{service}
ExecStart=/usr/bin/dotnet QuinntyneBrownStudio.{assembly}.dll
Restart=on-failure
RestartSec=5
TimeoutStopSec=90
NoNewPrivileges=true
PrivateTmp=true
ProtectSystem=strict
ProtectHome=true
ReadWritePaths=/var/lib/studio
SyslogIdentifier=qbs-{service}
[Install]
WantedBy=multi-user.target
"""


def apply():
    """Converge directories and units on the reviewed definition. Safe to repeat."""
    for directory in DIRECTORIES:
        subprocess.run(["install", "-d", "-o", "qbs", "-g", "qbs", "-m", "750", str(directory)], check=True)
    reload = False
    for service, assembly in ASSEMBLIES.items():
        unit = Path(f"/etc/systemd/system/qbs-{service}.service")
        content = UNIT.format(service=service, assembly=assembly)
        if not unit.is_file() or unit.read_text() != content:
            unit.write_text(content)
            reload = True
    if reload:
        subprocess.run(["systemctl", "daemon-reload"], check=True)
    subprocess.run(["systemctl", "enable", "qbs-api", "qbs-worker"], check=True)


if __name__ == "__main__":
    sys.exit(apply())
