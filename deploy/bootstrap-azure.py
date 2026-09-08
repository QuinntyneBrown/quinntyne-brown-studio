"""One-time operator bootstrap: scoped OIDC identities and GitHub environments.

Run with an Azure account able to create groups, identities and role assignments,
and a GitHub account with repository administration access. No credentials are stored.
"""
import argparse
import json
from pathlib import Path
import shutil
import subprocess
import tempfile


def command(program, *args):
    result = subprocess.run([shutil.which(program) or program, *args], check=True, capture_output=True, text=True)
    return json.loads(result.stdout) if result.stdout.strip() else None


def azure(*args):
    return command("az", *args, "--only-show-errors", "-o", "json")


def github(method, route, payload):
    with tempfile.TemporaryDirectory() as temporary:
        file = Path(temporary) / "body.json"
        file.write_text(json.dumps(payload))
        return command("gh", "api", "--method", method, route, "--input", str(file))


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--subscription", default="4a1b5113-89f9-4d27-acfe-581493385536")
    parser.add_argument("--repository", default="QuinntyneBrown/quinntyne-brown-studio")
    parser.add_argument("--ssh-public-key", type=Path, required=True)
    parser.add_argument("--administrator-email", required=True)
    args = parser.parse_args()
    key = args.ssh_public_key.read_text().strip()
    if not key.startswith(("ssh-ed25519 ", "ssh-rsa ")) or "\n" in key:
        parser.error("Provide a single OpenSSH public key, never a private key")
    if "@" not in args.administrator_email or any(c in args.administrator_email for c in "\r\n"):
        parser.error("Provide an administrator email address")
    azure("account", "set", "--subscription", args.subscription)
    account = azure("account", "show")
    owner, repository = args.repository.split("/", 1)
    identifiers = command("gh", "api", f"repos/{args.repository}", "--jq", "{repository: .id, owner: .owner.id}")
    for provider in ["Microsoft.ManagedIdentity", "Microsoft.Compute", "Microsoft.Network", "Microsoft.Storage",
                     "Microsoft.Sql", "Microsoft.KeyVault", "Microsoft.Communication", "Microsoft.Maps",
                     "Microsoft.CognitiveServices", "Microsoft.OperationalInsights", "Microsoft.Insights"]:
        azure("provider", "register", "--namespace", provider, "--wait")
    for group in ["rg-qbs-shared", "rg-qbs-prod", "rg-qbs-staging"]:
        if azure("group", "exists", "--name", group):
            existing = azure("group", "show", "--name", group)
            if existing.get("tags", {}).get("project") != "quinntyne-brown-studio":
                raise RuntimeError(f"Refusing to adopt unrelated resource group {group}")
        azure("group", "create", "--name", group, "--location", "canadacentral", "--tags", "project=quinntyne-brown-studio")
    identities = {}
    for environment, name in [("infrastructure", "id-qbs-infrastructure"), ("production", "id-qbs-github"),
                              ("staging", "id-qbs-staging")]:
        identity = azure("identity", "create", "--resource-group", "rg-qbs-shared", "--name", name)
        identities[environment] = identity
        # GitHub issues either the plain or the immutable-identifier subject; accept both so
        # toggling that repository setting never locks the workflows out of Azure.
        subjects = {"github": f"repo:{args.repository}:environment:{environment}",
                    "github-immutable": f"repo:{owner}@{identifiers['owner']}/{repository}@{identifiers['repository']}:environment:{environment}"}
        for credential, subject in subjects.items():
            azure("identity", "federated-credential", "create", "--resource-group", "rg-qbs-shared", "--identity-name", name,
                  "--name", credential, "--issuer", "https://token.actions.githubusercontent.com",
                  "--subject", subject, "--audiences", "api://AzureADTokenExchange")
        route = f"repos/{args.repository}/environments/{environment}"
        github("PUT", route, {"reviewers": [], "wait_timer": 0,
                              "deployment_branch_policy": {"protected_branches": False, "custom_branch_policies": True}})
        policies = command("gh", "api", route + "/deployment-branch-policies")["branch_policies"]
        if any(policy["name"] != "main" or policy.get("type", "branch") != "branch" for policy in policies):
            raise RuntimeError(f"Reconcile existing non-main deployment policies on {environment} before proceeding")
        if not any(policy["name"] == "main" and policy.get("type", "branch") == "branch" for policy in policies):
            github("POST", route + "/deployment-branch-policies", {"name": "main", "type": "branch"})
        values = {"AZURE_CLIENT_ID": identity["clientId"], "AZURE_TENANT_ID": account["tenantId"],
                  "AZURE_SUBSCRIPTION_ID": args.subscription,
                  "QBS_RESOURCE_GROUP": "rg-qbs-staging" if environment == "staging" else "rg-qbs-prod",
                  "QBS_VM_NAME": "vm-qbs"}
        for variable, value in values.items():
            subprocess.run(["gh", "variable", "set", variable, "--repo", args.repository, "--env", environment, "--body", value], check=True)
    for group in ["rg-qbs-shared", "rg-qbs-prod", "rg-qbs-staging"]:
        for role in ["Contributor", "User Access Administrator"]:
            azure("role", "assignment", "create", "--assignee-object-id", identities["infrastructure"]["principalId"],
                  "--assignee-principal-type", "ServicePrincipal", "--role", role,
                  "--scope", f"/subscriptions/{args.subscription}/resourceGroups/{group}")
    values = {"QBS_DEPLOY_PRINCIPAL_ID": identities["production"]["principalId"],
              "QBS_INFRA_PRINCIPAL_ID": identities["infrastructure"]["principalId"],
              "QBS_SSH_PUBLIC_KEY": key, "QBS_ADMINISTRATOR_EMAIL": args.administrator_email}
    for variable, value in values.items():
        subprocess.run(["gh", "variable", "set", variable, "--repo", args.repository, "--env", "infrastructure", "--body", value], check=True)
    staging = identities["staging"]
    staging_values = {
        "QBS_SSH_PUBLIC_KEY": key,
        "QBS_ADMINISTRATOR_EMAIL": args.administrator_email,
        "QBS_DEPLOY_PRINCIPAL_ID": staging["principalId"],
        "QBS_INFRA_PRINCIPAL_ID": identities["infrastructure"]["principalId"],
    }
    for variable, value in staging_values.items():
        subprocess.run(["gh", "variable", "set", variable, "--repo", args.repository, "--env", "staging", "--body", value], check=True)
    print("Bootstrap complete. Run Provision studio infrastructure in preview mode, then apply mode.")


if __name__ == "__main__":
    main()
