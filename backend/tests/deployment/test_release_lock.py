"""AC-AZ-04: two independent installers cannot hold the host lock together."""
from pathlib import Path
import subprocess
import sys
import tempfile
import time
import unittest


class ReleaseLockAcceptanceTests(unittest.TestCase):
    def test_AC_AZ_04_concurrent_process_waits_for_active_installer(self):
        source = Path(__file__).resolve().parents[3] / "deploy/linux"
        code = """
import sys, time
from pathlib import Path
sys.path.insert(0, sys.argv[1])
from release import release_lock
root = Path(sys.argv[2])
(root / (sys.argv[3] + '-started')).touch()
with release_lock(root):
    (root / sys.argv[3]).touch()
    if sys.argv[3] == 'first':
        while not (root / 'release').exists():
            time.sleep(0.02)
"""
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            first = subprocess.Popen([sys.executable, "-c", code, str(source), directory, "first"])
            second = None
            try:
                self.wait_for(root / "first")
                second = subprocess.Popen([sys.executable, "-c", code, str(source), directory, "second"])
                self.wait_for(root / "second-started")
                time.sleep(0.1)
                self.assertFalse((root / "second").exists())
                (root / "release").touch()
                self.assertEqual(first.wait(timeout=10), 0)
                self.assertEqual(second.wait(timeout=10), 0)
                self.assertTrue((root / "second").exists())
            finally:
                for process in [first, second]:
                    if process is not None and process.poll() is None:
                        process.kill()
                        process.wait()

    def wait_for(self, path):
        deadline = time.monotonic() + 10
        while not path.exists():
            if time.monotonic() > deadline:
                self.fail("Installer process did not start")
            time.sleep(0.02)
