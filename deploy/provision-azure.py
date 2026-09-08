"""Standard Bicep group deployment; emit host outputs only after successful apply."""
import json
import os
from pathlib import Path
import shutil
import subprocess

operation = os.environ["QBS_INFRA_OPERATION"]
if operation not in ["preview", "apply"]:
    raise ValueError("Expected preview or apply")
parameters = json.loads(Path("infra/main.parameters.production.json").read_text())
for parameter, variable in [("sshPublicKey", "QBS_SSH_PUBLIC_KEY"), ("administratorEmail", "QBS_ADMINISTRATOR_EMAIL"),
                            ("deployPrincipalId", "QBS_DEPLOY_PRINCIPAL_ID"), ("sqlAdministratorObjectId", "QBS_INFRA_PRINCIPAL_ID")]:
    value = os.environ[variable]
    if not value.strip():
        raise ValueError("Missing " + variable)
    parameters["parameters"][parameter] = {"value": value}
parameters["parameters"]["sqlAdministratorName"] = {"value": "id-qbs-infrastructure"}
directory = Path(".artifacts/infrastructure")
directory.mkdir(parents=True, exist_ok=True)
file = directory / "parameters.json"
file.write_text(json.dumps(parameters))
args = [shutil.which("az") or "az", "deployment", "group"]
common = ["--resource-group", os.environ["QBS_RESOURCE_GROUP"], "--template-file", "infra/main.bicep", "--parameters", "@" + str(file), "--only-show-errors"]
subprocess.run(args + ["what-if", *common], check=True)
if operation == "apply":
    result = subprocess.run(args + ["create", "--name", "qbs-production", *common, "--query", "properties.outputs", "-o", "json"],
                            check=True, capture_output=True, text=True)
    (directory / "outputs.json").write_text(result.stdout)
