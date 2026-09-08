"""Standard Bicep group deployment; emit host outputs only after successful apply."""
import json
import os
from pathlib import Path
import shutil
import subprocess

operation = os.environ["QBS_INFRA_OPERATION"]
if operation not in ["preview", "apply"]:
    raise ValueError("Expected preview or apply")
parameters_file = os.environ.get("QBS_PARAMETERS_FILE", "infra/main.parameters.production.json")
parameters = json.loads(Path(parameters_file).read_text())
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
    # Capture the outputs only. A failed deployment must still print the reason it failed.
    deployment_name = os.environ.get("QBS_DEPLOYMENT_NAME", "qbs-production")
    result = subprocess.run(args + ["create", "--name", deployment_name, *common, "--query", "properties.outputs", "-o", "json"],
                            check=True, stdout=subprocess.PIPE, text=True)
    (directory / "outputs.json").write_text(result.stdout)
