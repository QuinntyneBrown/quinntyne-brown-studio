#!/usr/bin/env python3
"""Install a verified, immutable release. Run as root through managed Run Command."""
import argparse
from contextlib import contextmanager
import hashlib
import json
import os
from pathlib import Path
import re
import shutil
import subprocess
import sys
import tarfile
import tempfile
import time
import urllib.parse
import urllib.request

sys.path.insert(0, str(Path(__file__).resolve().parent))
import gateway
import services


def host_config(root):
    """The reviewed host description written by host preparation. A release without it cannot
    know which addresses the gateway must publish, and must not activate blind."""
    path = root / "host.json"
    if not path.is_file():
        raise RuntimeError("Host is not prepared: /opt/studio/host.json is missing")
    return json.loads(path.read_text())


def write_json(path, value):
    temporary = path.with_suffix(".tmp")
    temporary.write_text(json.dumps(value) + "\n")
    os.replace(temporary, path)


def unpack(archive, target, sha):
    with tarfile.open(archive, "r:gz") as tar:
        names = set()
        for item in tar.getmembers():
            path = Path(item.name)
            if (path.is_absolute() or ".." in path.parts or "\\" in item.name
                    or not (item.isfile() or item.isdir()) or item.name in names):
                raise ValueError("Unsafe archive member")
            names.add(item.name)
        tar.extractall(target, filter="data")
    verify_directory(target, sha)


def verify_directory(target, sha):
    manifest = json.loads((target / "manifest.json").read_text())
    if manifest["sha"] != sha:
        raise ValueError("Release SHA mismatch")
    actual = {p.relative_to(target).as_posix(): hashlib.sha256(p.read_bytes()).hexdigest()
              for p in target.rglob("*") if p.is_file() and p != target / "manifest.json"}
    if actual != manifest["files"]:
        raise ValueError("Release manifest mismatch")
    for required in ["api/QuinntyneBrownStudio.Api.dll", "worker/QuinntyneBrownStudio.Worker.dll",
                     "qualification/QuinntyneBrownStudio.Qualification.dll",
                     "marketing/index.html", "admin/index.html", "client/index.html"]:
        if required not in actual:
            raise ValueError("Incomplete release: " + required)


def fetch(origin, path):
    with urllib.request.urlopen(origin + path, timeout=10) as response:
        if response.status != 200:
            raise RuntimeError("HTTP health check failed: " + path)
        return response.headers.get("Content-Type", ""), response.read().decode("utf-8", "replace")


def healthy(origin):
    consecutive = 0
    for attempt in range(30):
        try:
            for service in ["qbs-api", "qbs-worker", "caddy"]:
                subprocess.run(["systemctl", "is-active", "--quiet", service], check=True)
            for path in ["/api/health", "/", "/admin/", "/client/"]:
                fetch(origin, path)
            # A status code alone cannot tell the blog apart from the marketing shell the gateway
            # falls back to, so check what actually answered: the page the API renders carries its
            # own canonical link, and the feed is not HTML.
            if f'<link rel="canonical" href="{origin}/blog" />' not in fetch(origin, "/blog")[1]:
                raise RuntimeError("The gateway did not serve the blog from the API")
            if "xml" not in fetch(origin, "/blog/feed.xml")[0]:
                raise RuntimeError("The gateway did not serve the blog feed from the API")
            consecutive += 1
            if consecutive >= 2:
                return
            time.sleep(5)
        except Exception:
            consecutive = 0
            if attempt == 29:
                raise
            time.sleep(5)
    raise RuntimeError("Services did not remain healthy for two consecutive checks")


def prune(root, keep=5):
    """Delete only installer-owned inactive release directories after a successful activation."""
    releases = (root / "releases").resolve()
    state = json.loads((root / "state.json").read_text())
    protected = {(root / "current").resolve()}
    if state.get("previous"):
        protected.add(Path(state["previous"]).resolve())
    candidates = []
    for path in releases.iterdir():
        if path.is_symlink() or not path.is_dir() or not re.fullmatch("[0-9a-f]{40}", path.name):
            continue
        if path.resolve().parent != releases:
            continue
        try:
            if json.loads((path / "manifest.json").read_text())["sha"] != path.name:
                continue
        except (OSError, ValueError, KeyError):
            continue
        candidates.append(path)
    candidates.sort(key=lambda path: path.stat().st_mtime, reverse=True)
    protected.update(candidates[:keep])
    for path in candidates:
        if path not in protected:
            shutil.rmtree(path)


def activate(root, sha, sequence, origin, rollback=False):
    # Read before anything is stopped or switched: a host that cannot be configured must
    # leave the running release alone.
    host = host_config(root)
    state_path = root / "state.json"
    state = json.loads(state_path.read_text()) if state_path.exists() else {"sequence": 0, "sha": None}
    if sequence <= state["sequence"]:
        if sha == state["sha"] and sequence == state["sequence"]:
            if not (root / "current").exists() or (root / "current").resolve().name != sha:
                raise RuntimeError("Recorded release is not active; explicit recovery required")
            verify_directory(root / "releases" / sha, sha)
            services.apply()
            gateway.apply(host)
            healthy(origin)
            return
        raise RuntimeError("Stale release rejected")
    target = root / "releases" / sha
    if not (target / "manifest.json").is_file():
        raise RuntimeError("Release is not installed")
    verify_directory(target, sha)
    current = root / "current"
    previous = str(current.resolve()) if current.exists() else None
    try:
        subprocess.run(["systemctl", "stop", "qbs-api", "qbs-worker"], check=True)
        if not rollback:
            subprocess.run(["systemd-run", "--wait", "--pipe", "--collect", "--uid=qbs",
                            "--property=EnvironmentFile=" + str(root / "config/production.env"),
                            "--working-directory=" + str(target / "api"),
                            "/usr/bin/dotnet", str(target / "api/QuinntyneBrownStudio.Api.dll"), "--migrate"], check=True)
        link = root / "current.next"
        link.unlink(missing_ok=True)
        link.symlink_to(target, target_is_directory=True)
        os.replace(link, current)
        # Service definitions and routes travel with the code that needs them: host preparation
        # is not repeated on release, so both are rewritten here from this release's definitions.
        services.apply()
        gateway.apply(host)
        subprocess.run(["systemctl", "restart", "qbs-api", "qbs-worker"], check=True)
        healthy(origin)
        write_json(state_path, {"sha": sha, "sequence": sequence, "previous": previous})
        (root / "failure.json").unlink(missing_ok=True)
    except Exception as error:
        write_json(root / "failure.json", {"sha": sha, "previous": previous, "error": str(error)})
        # Never restart old binaries against a possibly partially changed schema.
        subprocess.run(["systemctl", "stop", "qbs-api", "qbs-worker"], check=False)
        subprocess.run(["journalctl", "-u", "qbs-api", "-u", "qbs-worker", "-n", "100", "--no-pager"], check=False)
        raise


def download(config, sha, archive):
    query = urllib.parse.urlencode({"api-version": "2018-02-01", "resource": "https://storage.azure.com/",
                                   "client_id": config["clientId"]})
    request = urllib.request.Request("http://169.254.169.254/metadata/identity/oauth2/token?" + query,
                                     headers={"Metadata": "true"})
    # The metadata endpoint must bypass any configured outbound proxy.
    with urllib.request.build_opener(urllib.request.ProxyHandler({})).open(request, timeout=30) as response:
        token = json.load(response)["access_token"]
    url = f"https://{config['storage']}.blob.core.windows.net/releases/{sha}.tar.gz"
    request = urllib.request.Request(url, headers={"Authorization": "Bearer " + token, "x-ms-version": "2023-11-03"})
    with urllib.request.urlopen(request, timeout=120) as response, archive.open("wb") as destination:
        shutil.copyfileobj(response, destination)


@contextmanager
def release_lock(root):
    import fcntl
    with (root / "deploy.lock").open("w") as lock:
        fcntl.flock(lock, fcntl.LOCK_EX)
        yield


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("sha")
    parser.add_argument("sequence", type=int)
    parser.add_argument("--digest")
    parser.add_argument("--rollback", action="store_true")
    args = parser.parse_args()
    if not re.fullmatch("[0-9a-f]{40}", args.sha) or args.sequence < 1:
        parser.error("Invalid release identity")
    if not args.rollback and not re.fullmatch("[0-9a-f]{64}", args.digest or ""):
        parser.error("A SHA-256 digest is required")
    root = Path("/opt/studio")
    with release_lock(root):
        config = json.loads((root / "config/deployment.json").read_text())
        destination = root / "releases" / args.sha
        if not args.rollback:
            with tempfile.TemporaryDirectory(dir=root / "releases") as temporary:
                temporary = Path(temporary)
                archive = temporary / "release.tar.gz"
                download(config, args.sha, archive)
                if hashlib.sha256(archive.read_bytes()).hexdigest() != args.digest:
                    raise ValueError("Archive checksum mismatch")
                unpack(archive, temporary / "unpacked", args.sha)
                if destination.exists():
                    verify_directory(destination, args.sha)
                    if (destination / "manifest.json").read_bytes() != (temporary / "unpacked/manifest.json").read_bytes():
                        raise ValueError("Immutable release already exists with different content")
                else:
                    os.replace(temporary / "unpacked", destination)
        activate(root, args.sha, args.sequence, config["origin"], args.rollback)
        try:
            prune(root)
        except OSError as error:
            print("Release is healthy but old release cleanup failed: " + str(error))
        print("Release verified: " + args.sha)


if __name__ == "__main__":
    main()
