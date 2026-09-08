"""AC-AZ-06: an ARM request accepted by Azure is not guest execution success."""
import importlib.util
import json
from pathlib import Path
import tempfile
import unittest
from unittest.mock import patch


class RunCommandAcceptanceTests(unittest.TestCase):
    def setUp(self):
        path = Path(__file__).resolve().parents[3] / "deploy/run-command.py"
        spec = importlib.util.spec_from_file_location("run_command", path)
        self.remote = importlib.util.module_from_spec(spec)
        spec.loader.exec_module(self.remote)

    def test_AC_AZ_06_failed_guest_is_failure_even_when_provisioned(self):
        failed = {"properties": {"provisioningState": "Succeeded", "instanceView": {"executionState": "Failed", "exitCode": 1}}}
        with tempfile.TemporaryDirectory() as directory, patch.object(self.remote, "az", side_effect=[{}, failed, None]):
            evidence = Path(directory) / "result.json"
            with self.assertRaisesRegex(RuntimeError, "Remote deployment failed"):
                self.remote.execute("/vm", "canadacentral", "exit 1", evidence)
            self.assertEqual(json.loads(evidence.read_text()), failed)

    def test_AC_AZ_06_waits_for_guest_exit_before_success(self):
        running = {"properties": {"instanceView": {"executionState": "Running"}}}
        success = {"properties": {"instanceView": {"executionState": "Succeeded", "exitCode": 0}}}
        with tempfile.TemporaryDirectory() as directory, patch.object(self.remote, "az", side_effect=[{}, running, success, None]), patch.object(self.remote.time, "sleep"):
            result = self.remote.execute("/vm", "canadacentral", "true", Path(directory) / "result.json")
            self.assertEqual(result, success)
