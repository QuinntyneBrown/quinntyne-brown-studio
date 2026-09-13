"""AC-AZ-10: the catalog bootstrap creates one Free Static Web App and hands its token to GitHub privately."""
import contextlib
import importlib.util
import io
import json
from pathlib import Path
import unittest
from unittest.mock import patch

SOURCE = Path(__file__).resolve().parents[3] / "deploy/bootstrap-design-system.py"


class Completed:
    def __init__(self, payload):
        self.stdout = json.dumps(payload) if payload is not None else ""


class DesignSystemBootstrapAcceptanceTests(unittest.TestCase):
    def setUp(self):
        spec = importlib.util.spec_from_file_location("bootstrap_design_system", SOURCE)
        self.bootstrap = importlib.util.module_from_spec(spec)
        spec.loader.exec_module(self.bootstrap)
        self.calls = []
        self.group = {"name": "rg-qbs-shared", "tags": {"project": "quinntyne-brown-studio"}}

    def fake_run(self, command, **options):
        self.calls.append((command, options))
        words = command[1:]
        if words[:2] == ["group", "show"]:
            return Completed(self.group)
        if words[:2] == ["staticwebapp", "create"]:
            return Completed({"name": "qbs-design-system", "defaultHostname": "catalog.example.azurestaticapps.net"})
        if words[:3] == ["staticwebapp", "secrets", "list"]:
            return Completed({"properties": {"apiKey": "deployment-token-value"}})
        return Completed(None)

    def commands(self, *prefix):
        return [command for command, _ in self.calls if command[1:1 + len(prefix)] == list(prefix)]

    # Given the tagged shared resource group, when the operator bootstraps the catalog host without
    # choosing a tier, then one Free Static Web App is created there and its deployment token reaches the
    # repository secret the workflow reads, through standard input and never as a command-line argument.
    def test_AC_AZ_10_token_reaches_the_repository_secret_privately(self):
        with patch("subprocess.run", side_effect=self.fake_run), contextlib.redirect_stdout(io.StringIO()) as report:
            self.bootstrap.main([])
        self.assertIn("catalog.example.azurestaticapps.net", report.getvalue())
        self.assertNotIn("deployment-token-value", report.getvalue())
        [create] = self.commands("staticwebapp", "create")
        self.assertIn("rg-qbs-shared", create)
        self.assertIn("qbs-design-system", create)
        self.assertEqual(create[create.index("--sku") + 1], "Free")
        self.assertEqual(create[create.index("--location") + 1], "westus2")
        [(secret, options)] = [(command, options) for command, options in self.calls if command[1:3] == ["secret", "set"]]
        self.assertEqual(secret[3], "SWA_DESIGN_SYSTEM_DEPLOYMENT_TOKEN")
        self.assertEqual(secret[secret.index("--repo") + 1], "QuinntyneBrown/quinntyne-brown-studio")
        self.assertEqual(options["input"], "deployment-token-value")
        self.assertNotIn("deployment-token-value", secret)
        self.assertTrue(all("deployment-token-value" not in word for command, _ in self.calls for word in command))

    # Given a shared resource group that does not carry the project tag, when the bootstrap runs, then it
    # refuses before creating any resource or touching any secret.
    def test_AC_AZ_10_unrelated_group_is_refused_before_any_change(self):
        self.group = {"name": "rg-qbs-shared", "tags": {"project": "something-else"}}
        with patch("subprocess.run", side_effect=self.fake_run):
            with self.assertRaises(RuntimeError):
                self.bootstrap.main([])
        self.assertEqual(self.commands("staticwebapp"), [])
        self.assertEqual(self.commands("secret"), [])

    # Given a subscription that has used its ten Free sites, when the operator chooses the billed tier,
    # then that tier is requested and nothing else about the host changes.
    def test_AC_AZ_10_operator_can_choose_the_billed_tier(self):
        with patch("subprocess.run", side_effect=self.fake_run), contextlib.redirect_stdout(io.StringIO()):
            self.bootstrap.main(["--sku", "Standard"])
        [create] = self.commands("staticwebapp", "create")
        self.assertEqual(create[create.index("--sku") + 1], "Standard")
        self.assertIn("rg-qbs-shared", create)


if __name__ == "__main__":
    unittest.main()
