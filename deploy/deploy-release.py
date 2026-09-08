"""Deploy only the artifact produced by this verified workflow run."""
import base64
import hashlib
import importlib.util
import json
import os
from pathlib import Path
import re
import subprocess
import sys

spec = importlib.util.spec_from_file_location("run_command", Path(__file__).with_name("run-command.py"))
remote = importlib.util.module_from_spec(spec)
spec.loader.exec_module(remote)
sha = os.environ.get("QBS_ROLLBACK_SHA") or os.environ["GITHUB_SHA"]
rollback = "QBS_ROLLBACK_SHA" in os.environ
if not re.fullmatch(r"[0-9a-f]{40}", sha):
    raise ValueError("Expected a full commit SHA")
sequence = int(os.environ["GITHUB_RUN_ID"])
group = os.environ["QBS_RESOURCE_GROUP"]
host = remote.az("deployment", "group", "show", "-g", group, "-n", "qbs-production")["properties"]["outputs"]["host"]["value"]
vm = remote.az("vm", "show", "-g", group, "-n", os.environ["QBS_VM_NAME"])
arguments = [sha, str(sequence)]
if rollback:
    arguments.append("--rollback")
else:
    storage = host["storage"]
    archive = Path(".artifacts/release") / (sha + ".tar.gz")
    digest = hashlib.sha256(archive.read_bytes()).hexdigest()
    if archive.with_suffix(".gz.sha256").read_text().strip() != digest:
        raise ValueError("Downloaded artifact checksum mismatch")
    exists = remote.az("storage", "blob", "exists", "--account-name", storage, "--auth-mode", "login",
                       "--container-name", "releases", "--name", archive.name)["exists"]
    if exists:
        # Never overwrite a commit's release, even on workflow reruns.
        existing = archive.with_suffix(".existing")
        remote.az("storage", "blob", "download", "--account-name", storage, "--auth-mode", "login",
                  "--container-name", "releases", "--name", archive.name, "--file", str(existing), "--overwrite")
        if hashlib.sha256(existing.read_bytes()).hexdigest() != digest:
            raise ValueError("This commit already has a different immutable release; use a new commit")
    else:
        remote.az("storage", "blob", "upload", "--account-name", storage, "--auth-mode", "login",
                  "--container-name", "releases", "--name", archive.name, "--file", str(archive), "--overwrite", "false")
    arguments += ["--digest", digest]
encoded = base64.b64encode((Path(__file__).parent / "linux/release.py").read_bytes()).decode()
script = "set -eu\n" + f"echo '{encoded}' | base64 --decode > /opt/studio/bin/release.py\n"
script += "python3 /opt/studio/bin/release.py " + " ".join(arguments) + "\n"
remote.execute(vm["id"], vm["location"], script, Path(".artifacts/deployment/run-command.json"))
Path(".artifacts/deployment/release.json").write_text(json.dumps({"sha": sha, "run": sequence, "rollback": rollback}))
with open(os.environ.get("GITHUB_OUTPUT", os.devnull), "a") as output:
    # The browser smoke that follows must address the origin this deployment served.
    output.write(f"origin={host['origin']}\n")
with open(os.environ.get("GITHUB_STEP_SUMMARY", os.devnull), "a") as summary:
    summary.write(f"Deployed `{sha}` to {host['origin']}\n")
