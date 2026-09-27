"""One-time operator bootstrap for the design-system catalog host.

Creates one Free Azure Static Web App in the tagged shared resource group and stores
its deployment token as the repository secret the Deploy design system workflow reads.
Run with an Azure account able to create resources in that group and a GitHub account
with repository administration access. Rerun after resetting the site's API key to
rotate the token. The token reaches GitHub through standard input only; it is never
printed, logged, or passed on a command line.
"""
import argparse
import json
import shutil
import subprocess

GROUP = "rg-qbs-shared"
NAME = "qbs-design-system"
SECRET = "SWA_DESIGN_SYSTEM_DEPLOYMENT_TOKEN"
PROJECT = "quinntyne-brown-studio"


def run(program, *args, **options):
    return subprocess.run([shutil.which(program) or program, *args], check=True, text=True, **options)


def azure(*args):
    result = run("az", *args, "--only-show-errors", "-o", "json", stdout=subprocess.PIPE)
    return json.loads(result.stdout) if result.stdout.strip() else None


def main(argv=None):
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--subscription", default="4a1b5113-89f9-4d27-acfe-581493385536")
    parser.add_argument("--repository", default="QuinntyneBrown/quinntyne-brown-studio")
    parser.add_argument("--location", default="westus2",
                        help="A Static Web Apps management region; the catalog itself is served globally.")
    parser.add_argument("--sku", default="Free", choices=["Free", "Standard"],
                        help="Free is limited to ten sites per region in this subscription; Standard is billed monthly.")
    args = parser.parse_args(argv)
    azure("account", "set", "--subscription", args.subscription)
    group = azure("group", "show", "--name", GROUP)
    if (group.get("tags") or {}).get("project") != PROJECT:
        raise RuntimeError(f"Refusing to use unrelated resource group {GROUP}; run bootstrap-azure.py first")
    azure("provider", "register", "--namespace", "Microsoft.Web", "--wait")
    site = azure("staticwebapp", "create", "--resource-group", GROUP, "--name", NAME,
                 "--location", args.location, "--sku", args.sku, "--tags", f"project={PROJECT}")
    token = azure("staticwebapp", "secrets", "list", "--resource-group", GROUP, "--name", NAME)["properties"]["apiKey"]
    run("gh", "secret", "set", SECRET, "--repo", args.repository, input=token)
    print(f"Catalog host https://{site['defaultHostname']} is ready and {SECRET} is set on {args.repository}. "
          "Run Deploy design system to publish the catalog.")


if __name__ == "__main__":
    main()
