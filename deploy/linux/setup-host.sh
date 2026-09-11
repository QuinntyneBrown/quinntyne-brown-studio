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
if [[ ! -f /swapfile ]]; then
  fallocate -l 2G /swapfile
  chmod 600 /swapfile
  mkswap /swapfile
fi
swapon --show=NAME --noheadings | grep -qx /swapfile || swapon /swapfile
grep -q '^/swapfile ' /etc/fstab || echo '/swapfile none swap sw 0 0' >> /etc/fstab
# Service definitions and the directories they own live with the code, not with the host:
# a release can change them, and does. See services.py.
python3 "$(dirname "$0")/services.py"
systemctl enable qbs-api qbs-worker caddy
command -v dcraw_emu
dotnet --list-runtimes
