"""Package Linux publish output and the already verified Angular artifacts."""
import argparse
import hashlib
import gzip
import json
from pathlib import Path
import re
import tarfile

parser = argparse.ArgumentParser()
parser.add_argument("directory", type=Path)
parser.add_argument("sha")
parser.add_argument("output", type=Path)
args = parser.parse_args()
if not re.fullmatch("[0-9a-f]{40}", args.sha):
    parser.error("Expected a full commit SHA")
files = {p.relative_to(args.directory).as_posix(): hashlib.sha256(p.read_bytes()).hexdigest()
         for p in sorted(args.directory.rglob("*")) if p.is_file() and p != args.directory / "manifest.json"}
(args.directory / "manifest.json").write_text(json.dumps({"sha": args.sha, "files": files}, sort_keys=True))
with args.output.open("wb") as destination, gzip.GzipFile(fileobj=destination, mode="wb", filename="", mtime=0) as compressed:
    with tarfile.open(fileobj=compressed, mode="w") as archive:
        for path in sorted(args.directory.rglob("*")):
            if path.is_file():
                info = tarfile.TarInfo(path.relative_to(args.directory).as_posix())
                info.size = path.stat().st_size
                info.mode = 0o644
                with path.open("rb") as source:
                    archive.addfile(info, source)
digest = hashlib.sha256(args.output.read_bytes()).hexdigest()
args.output.with_suffix(args.output.suffix + ".sha256").write_text(digest + "\n")
print(digest)
