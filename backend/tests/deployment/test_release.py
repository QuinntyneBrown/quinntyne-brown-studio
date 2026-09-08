"""AC-AZ-03..06: execute the installer against isolated files and controlled services."""
import importlib.util
import io
import hashlib
import json
from pathlib import Path
import tarfile
import tempfile
import subprocess
import sys
import unittest
from unittest.mock import patch

SOURCE = Path(__file__).resolve().parents[3] / "deploy/linux/release.py"


class ReleaseAcceptanceTests(unittest.TestCase):
    def setUp(self):
        spec = importlib.util.spec_from_file_location("release", SOURCE)
        self.release = importlib.util.module_from_spec(spec)
        spec.loader.exec_module(self.release)
        self.temp = tempfile.TemporaryDirectory()
        self.addCleanup(self.temp.cleanup)
        self.root = Path(self.temp.name)
        (self.root / "releases").mkdir()
        self.old = "a" * 40
        self.new = "b" * 40
        self.prepare(self.old)
        self.prepare(self.new)
        (self.root / "current").symlink_to(self.root / "releases" / self.old, target_is_directory=True)
        (self.root / "state.json").write_text(json.dumps({"sha": self.old, "sequence": 10}))

    def prepare(self, sha):
        folder = self.root / "releases" / sha
        folder.mkdir()
        files = {}
        for name in ["api/QuinntyneBrownStudio.Api.dll", "worker/QuinntyneBrownStudio.Worker.dll",
                     "qualification/QuinntyneBrownStudio.Qualification.dll", "marketing/index.html", "admin/index.html", "client/index.html"]:
            file = folder / name
            file.parent.mkdir(exist_ok=True)
            file.write_bytes(b"acceptance fixture")
            files[name] = hashlib.sha256(file.read_bytes()).hexdigest()
        (folder / "manifest.json").write_text(json.dumps({"sha": sha, "files": files}))

    def activate(self, sequence=11, rollback=False):
        return self.release.activate(self.root, self.new, sequence, "https://studio.example", rollback)

    # Given a healthy release, when installed, then pointer and recorded SHA advance.
    @patch("subprocess.run")
    def test_AC_AZ_03_success_and_duplicate_are_safe(self, run):
        with patch.object(self.release, "healthy"):
            self.activate()
            self.assertEqual((self.root / "current").resolve().name, self.new)
            self.assertEqual(json.loads((self.root / "state.json").read_text())["sha"], self.new)
            run.reset_mock()
            self.activate()
            run.assert_not_called()

    # Given a newer deployed run, when an older run arrives, then nothing changes.
    @patch("subprocess.run")
    def test_AC_AZ_04_stale_release_is_rejected(self, run):
        with self.assertRaisesRegex(RuntimeError, "Stale"):
            self.activate(sequence=9)
        self.assertEqual((self.root / "current").resolve().name, self.old)
        run.assert_not_called()

    # Given migration failure, when installing, then old files remain active and deployment fails.
    @patch("subprocess.run")
    def test_AC_AZ_05_migration_failure_does_not_activate(self, run):
        def command(args, **kwargs):
            if "--migrate" in args:
                raise RuntimeError("migration failed")
        run.side_effect = command
        with self.assertRaisesRegex(RuntimeError, "migration failed"):
            self.activate()
        self.assertEqual((self.root / "current").resolve().name, self.old)
        self.assertEqual(json.loads((self.root / "state.json").read_text())["sequence"], 10)

    # Given unhealthy services, when activated, then release is not recorded successful.
    @patch("subprocess.run")
    def test_AC_AZ_06_unhealthy_release_retains_recovery_evidence(self, run):
        with patch.object(self.release, "healthy", side_effect=RuntimeError("unhealthy")):
            with self.assertRaisesRegex(RuntimeError, "unhealthy"):
                self.activate()
        self.assertEqual(json.loads((self.root / "state.json").read_text())["sha"], self.old)
        self.assertTrue((self.root / "failure.json").exists())

    @patch("subprocess.run")
    def test_AC_AZ_06_failed_activation_cannot_report_previous_release_success(self, run):
        with patch.object(self.release, "healthy", side_effect=RuntimeError("unhealthy")):
            with self.assertRaises(RuntimeError):
                self.activate()
        with self.assertRaisesRegex(RuntimeError, "explicit recovery"):
            self.release.activate(self.root, self.old, 10, "https://studio.example")

    # Given a retained compatible release, when explicitly rolled back, then no migration runs.
    @patch("subprocess.run")
    def test_AC_AZ_07_rollback_activates_retained_release_without_migrating(self, run):
        with patch.object(self.release, "healthy"):
            self.activate(rollback=True)
        self.assertEqual((self.root / "current").resolve().name, self.new)
        self.assertFalse(any("--migrate" in call.args[0] for call in run.call_args_list))

    # Given an archive escaping its release directory, when unpacked, then no file escapes.
    def test_AC_AZ_05_archive_traversal_is_rejected(self):
        archive = self.root / "bad.tar.gz"
        with tarfile.open(archive, "w:gz") as tar:
            member = tarfile.TarInfo("../escaped")
            member.size = 1
            tar.addfile(member, io.BytesIO(b"x"))
        with self.assertRaises(ValueError):
            self.release.unpack(archive, self.root / "target", self.new)
        self.assertFalse((self.root / "escaped").exists())

    # Given identical publish output, when packaging twice, then reruns have the same digest.
    def test_AC_AZ_03_package_is_reproducible_and_round_trips(self):
        first, second = self.root / "first.tar.gz", self.root / "second.tar.gz"
        for archive in [first, second]:
            subprocess.run([sys.executable, str(SOURCE.parent.parent / "package-release.py"),
                            str(self.root / "releases" / self.new), self.new, str(archive)], check=True, capture_output=True)
        self.assertEqual(first.read_bytes(), second.read_bytes())
        self.release.unpack(first, self.root / "unpacked", self.new)
        self.assertTrue((self.root / "unpacked/admin/index.html").exists())

    # Given changed bytes in a retained release, when rolling back, then services are untouched.
    @patch("subprocess.run")
    def test_AC_AZ_05_manifest_mismatch_rejects_release(self, run):
        (self.root / "releases" / self.new / "admin/index.html").write_text("corrupt")
        with self.assertRaisesRegex(ValueError, "manifest mismatch"):
            self.activate(rollback=True)
        run.assert_not_called()

    # Given several retained releases, when pruning, then active, previous and unrelated data survive.
    def test_AC_AZ_07_pruning_preserves_recovery_and_unowned_directories(self):
        extra = "c" * 40
        self.prepare(extra)
        unowned = self.root / "releases/operator-notes"
        unowned.mkdir()
        (self.root / "state.json").write_text(json.dumps({"sha": self.old, "sequence": 10,
            "previous": str(self.root / "releases" / self.new)}))
        self.release.prune(self.root, keep=0)
        self.assertTrue((self.root / "releases" / self.old).is_dir())
        self.assertTrue((self.root / "releases" / self.new).is_dir())
        self.assertTrue(unowned.is_dir())
        self.assertFalse((self.root / "releases" / extra).exists())


if __name__ == "__main__":
    unittest.main()
