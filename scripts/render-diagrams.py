"""Render changed PlantUML sources and refresh the reviewed render baseline.

Run from any directory: python scripts/render-diagrams.py [--all] [path ...]

Sources are rendered with the local PlantUML runner (PLANTUML_JAR, C:/tools/plantuml.jar,
~/plantuml.jar, or plantuml on PATH). Every rendered pair is recorded in
docs/detailed-designs/diagram-manifest.json so that docs/detailed-designs/verify.py can tell
a reviewed render from a stale one. Review the rendered PNG before committing it.
"""
from pathlib import Path
import argparse
import hashlib
import json
import os
import shutil
import subprocess
import sys

ROOT = Path(__file__).resolve().parents[1]
DESIGNS = ROOT / "docs/detailed-designs"
MANIFEST = DESIGNS / "diagram-manifest.json"
BATCH = 25


def runner():
    candidates = [os.environ.get("PLANTUML_JAR", ""), "C:/tools/plantuml.jar", str(Path.home() / "plantuml.jar")]
    for candidate in candidates:
        if candidate and Path(candidate).is_file() and shutil.which("java"):
            return ["java", "-jar", candidate]
    if shutil.which("plantuml"):
        return ["plantuml"]
    return None


def digest_text(path):
    return hashlib.sha256(path.read_text(encoding="utf-8").encode("utf-8")).hexdigest()


def digest_bytes(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--all", action="store_true", help="render every diagram, not only changed sources")
    parser.add_argument("paths", nargs="*", help="optional .puml files or directories to limit the run")
    arguments = parser.parse_args()

    selected = []
    for given in arguments.paths or [DESIGNS]:
        path = Path(given)
        if not path.is_absolute():
            path = (Path.cwd() / path).resolve()
        selected.extend(sorted(path.rglob("*.puml")) if path.is_dir() else [path])
    sources = [path for path in selected if path.suffix == ".puml"]
    if not sources:
        print("No diagram sources selected.")
        return 0

    manifest = json.loads(MANIFEST.read_text(encoding="utf-8")) if MANIFEST.is_file() else {}
    stale = []
    for source in sources:
        key = source.relative_to(DESIGNS).as_posix()
        image = source.with_suffix(".png")
        recorded = manifest.get(key, {})
        if (
            arguments.all
            or not image.is_file()
            or recorded.get("sourceSha256") != digest_text(source)
            or recorded.get("imageSha256") != digest_bytes(image)
        ):
            stale.append(source)

    if not stale:
        print(f"Up to date: {len(sources)} diagram sources match their reviewed renders.")
        return 0

    command = runner()
    if command is None:
        print("PlantUML is unavailable. Set PLANTUML_JAR or put plantuml on PATH.", file=sys.stderr)
        return 1

    for start in range(0, len(stale), BATCH):
        batch = stale[start : start + BATCH]
        result = subprocess.run(
            command + ["-charset", "UTF-8", "-tpng"] + [str(path) for path in batch],
            capture_output=True,
            text=True,
        )
        if result.returncode != 0:
            print(result.stdout + result.stderr, file=sys.stderr)
            return result.returncode

    for source in stale:
        key = source.relative_to(DESIGNS).as_posix()
        manifest[key] = {"sourceSha256": digest_text(source), "imageSha256": digest_bytes(source.with_suffix(".png"))}
    MANIFEST.write_text(json.dumps(dict(sorted(manifest.items())), indent=2) + "\n", encoding="utf-8")

    print(f"Rendered and recorded {len(stale)} diagram(s):")
    for source in stale:
        print("  " + source.relative_to(ROOT).as_posix())
    print("Review each rendered PNG before committing it.")
    return 0


if __name__ == "__main__":
    sys.exit(main())
