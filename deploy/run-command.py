"""Execute a managed Azure VM command and check its guest exit status."""
import argparse
import json
from pathlib import Path
import subprocess
import shutil
import tempfile
import time
import uuid


def az(*args):
    result = subprocess.run([shutil.which("az") or "az", *args, "--only-show-errors", "-o", "json"], check=True, capture_output=True, text=True)
    return json.loads(result.stdout) if result.stdout.strip() else None


def execute(vm_id, location, script, evidence, name="qbs-release"):
    # A unique resource avoids reading a previous invocation's successful instance view.
    name = name + "-" + uuid.uuid4().hex[:12]
    url = f"https://management.azure.com{vm_id}/runCommands/{name}?api-version=2024-07-01"
    body = {"location": location, "properties": {"source": {"script": script}, "asyncExecution": False,
            "timeoutInSeconds": 1800, "treatFailureAsDeploymentFailure": True}}
    evidence.parent.mkdir(parents=True, exist_ok=True)
    try:
        with tempfile.TemporaryDirectory() as temporary:
            request = Path(temporary) / "command.json"
            request.write_text(json.dumps(body))
            az("rest", "--method", "put", "--url", url, "--body", "@" + str(request))
    except Exception:
        result = az("rest", "--method", "get", "--url", url + "&$expand=instanceView")
        evidence.write_text(json.dumps(result, indent=2))
        if result["properties"].get("provisioningState") == "Failed":
            az("rest", "--method", "delete", "--url", url)
        raise
    # Provisioning acceptance is not proof that the guest process succeeded.
    for _ in range(210):
        result = az("rest", "--method", "get", "--url", url + "&$expand=instanceView")
        evidence.write_text(json.dumps(result, indent=2))
        properties = result["properties"]
        view = properties.get("instanceView", {})
        state = view.get("executionState")
        if state in ["Succeeded", "Failed", "TimedOut", "Canceled"]:
            # Azure permits only 25 managed commands per VM. Preserve evidence, then remove the terminal resource.
            az("rest", "--method", "delete", "--url", url)
            print(view.get("output", ""))
            if state != "Succeeded" or view.get("exitCode") != 0:
                raise RuntimeError("Remote deployment failed; inspect the uploaded Run Command evidence")
            return result
        if properties.get("provisioningState") == "Failed":
            az("rest", "--method", "delete", "--url", url)
            raise RuntimeError("Run Command provisioning failed; inspect evidence")
        time.sleep(10)
    raise TimeoutError("Timed out waiting for guest deployment")


if __name__ == "__main__":
    parser = argparse.ArgumentParser()
    parser.add_argument("vm_id")
    parser.add_argument("location")
    parser.add_argument("script", type=Path)
    parser.add_argument("evidence", type=Path)
    parser.add_argument("--name", default="qbs-release")
    args = parser.parse_args()
    execute(args.vm_id, args.location, args.script.read_text(), args.evidence, args.name)
