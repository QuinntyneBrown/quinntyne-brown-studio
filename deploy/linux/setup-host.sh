#!/usr/bin/env bash
set -euo pipefail
[[ $(id -u) == 0 ]] || { echo 'Run as root.' >&2; exit 1; }
install -d -m 755 /opt/studio /opt/studio/bin /opt/studio/releases
exec 9>/opt/studio/deploy.lock
flock 9
export DEBIAN_FRONTEND=noninteractive
apt-get update
apt-get install -y software-properties-common ca-certificates curl python3
add-apt-repository -y ppa:dotnet/backports
apt-get update
apt-get install -y aspnetcore-runtime-10.0 libraw-bin libfontconfig1 caddy unattended-upgrades
id qbs >/dev/null 2>&1 || useradd --system --user-group --home-dir /var/lib/studio --create-home --shell /usr/sbin/nologin qbs
install -d -o root -g qbs -m 750 /opt/studio/config
install -d -o qbs -g qbs -m 750 /var/lib/studio /var/lib/studio/blog-media
if [[ ! -f /swapfile ]]; then
  fallocate -l 2G /swapfile
  chmod 600 /swapfile
  mkswap /swapfile
fi
swapon --show=NAME --noheadings | grep -qx /swapfile || swapon /swapfile
grep -q '^/swapfile ' /etc/fstab || echo '/swapfile none swap sw 0 0' >> /etc/fstab
for service in api worker; do
  if [[ $service == api ]]; then assembly=Api; else assembly=Worker; fi
  cat > "/etc/systemd/system/qbs-${service}.service" <<EOF
[Unit]
Description=Quinntyne Brown Studio ${service}
After=network-online.target
Wants=network-online.target
[Service]
User=qbs
Group=qbs
EnvironmentFile=/opt/studio/config/production.env
Environment=Blog__StoragePath=/var/lib/studio/blog-media
WorkingDirectory=/opt/studio/current/${service}
ExecStart=/usr/bin/dotnet QuinntyneBrownStudio.${assembly}.dll
Restart=on-failure
RestartSec=5
TimeoutStopSec=90
NoNewPrivileges=true
PrivateTmp=true
ProtectSystem=strict
ProtectHome=true
ReadWritePaths=/var/lib/studio
SyslogIdentifier=qbs-${service}
[Install]
WantedBy=multi-user.target
EOF
done
systemctl daemon-reload
systemctl enable qbs-api qbs-worker caddy
command -v dcraw_emu
dotnet --list-runtimes
