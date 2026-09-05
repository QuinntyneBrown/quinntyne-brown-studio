"""Validate design traceability and artifacts without modifying documentation.

Run from any directory: python docs/detailed-designs/verify.py
Set PLANTUML_JAR or put plantuml on PATH for independent syntax validation.
"""
from pathlib import Path
import os
import re
import shutil
import struct
import subprocess
import sys
import hashlib
import json

BASE = Path(__file__).resolve().parent
DOCS = BASE.parent
errors = []


def check(condition, message):
    if not condition:
        errors.append(message)


def anchor(text):
    text = re.sub(r"\[([^]]+)\]\([^)]+\)", r"\1", text)
    return re.sub(r"[^\w\- ]", "", text.lower()).replace(" ", "-")


l1 = (DOCS / "specs/L1.md").read_text(encoding="utf-8")
l2 = (DOCS / "specs/L2.md").read_text(encoding="utf-8")
parents = set(re.findall(r"^### (L1-\d+) —", l1, re.M))
requirements = {}
acceptance_ids = []
for section in re.split(r"(?=^### L2-\d+)", l2, flags=re.M)[1:]:
    match = re.match(
        r"### (L2-\d+) — .+\n\n\*\*Parent:\*\* \[(L1-\d+)\]\([^\n]+\)\n\n(.+?)(?=\n\n)",
        section,
        re.S,
    )
    check(match is not None, f"Malformed requirement section: {section[:60]}")
    if not match:
        continue
    rid, parent, statement = match.groups()
    check(rid not in requirements, f"Duplicate requirement {rid}")
    check(parent in parents, f"Unknown parent {parent} for {rid}")
    requirements[rid] = (parent, statement)
    criteria = re.findall(r"^\*\*(AC-L2-\d+-\d+)\*\*", section, re.M)
    check(bool(criteria), f"No acceptance criteria for {rid}")
    acceptance_ids.extend(criteria)
    for criterion in criteria:
        check(criterion.startswith(f"AC-{rid}-"), f"Wrong criterion parent: {criterion}")
check(len(set(acceptance_ids)) == len(acceptance_ids), "Duplicate acceptance identifiers")

coverage = {rid: [] for rid in requirements}
features = sorted(BASE.glob("*/*/README.md"))
check(len(features) == 23, f"Expected 23 feature designs, found {len(features)}")
for readme in features:
    content = readme.read_text(encoding="utf-8")
    headings = re.findall(r"^## (.+)$", content, re.M)
    check(headings == ["Overview", "Description", "Requirements", "Diagrams"], f"Feature headings: {readme}")
    section = content.split("## Requirements\n", 1)[-1].split("## Diagrams\n", 1)[0]
    rows = re.findall(r"^\| `(L2-\d+)` \| `(L1-\d+)` \| (.+) \|$", section, re.M)
    check(bool(rows), f"No requirement table rows: {readme}")
    check(not re.search(r"^\| `L1-", section, re.M), f"Standalone L1 row: {readme}")
    seen = set()
    for rid, parent, statement in rows:
        check(rid not in seen, f"Duplicate {rid} in {readme}")
        seen.add(rid)
        check(requirements.get(rid) == (parent, statement), f"Requirement wording or parent mismatch: {rid} in {readme}")
        if rid in coverage:
            coverage[rid].append(readme)
    for required in ["c4-context", "c4-container", "c4-component", "class-structure"]:
        check((readme.parent / "diagrams" / f"{required}.puml").is_file(), f"Missing {required}: {readme}")
    check(bool(list((readme.parent / "diagrams").glob("sequence-*.puml"))), f"No sequence diagram: {readme}")
    prose = content.split("## Requirements\n", 1)[0] + content.split("## Diagrams\n", 1)[-1]
    prose = re.sub(r"`[^`]+`", "", prose)
    banned = re.findall(r"\b(?:we|you|our|must|very|robust|leverage|seamless|simply|obviously)\b", prose, re.I)
    check(not banned, f"House-style words {banned}: {readme}")
for rid, pages in coverage.items():
    check(bool(pages), f"Uncovered requirement {rid}")

register = (BASE / "acceptance.md").read_text(encoding="utf-8")
registered = re.findall(r"^\| `(AC-L2-\d+-\d+)` \|", register, re.M)
check(sorted(registered) == sorted(acceptance_ids), "Acceptance register differs from specification")
for line in register.splitlines():
    if line.startswith("| `AC-"):
        cells = [cell.strip() for cell in line.split("|")[1:-1]]
        check(cells[2] in {"Not implemented", "Partial", "Complete"}, f"Unknown acceptance status: {line}")
        if cells[2] != "Not implemented":
            check("[" in cells[3], f"Implementation needs linked evidence or a stated gap: {line}")

for markdown in list(BASE.rglob("*.md")) + list((DOCS / "specs").glob("*.md")):
    content = markdown.read_text(encoding="utf-8")
    # Mask code spans so contract notation is not parsed as Markdown links.
    content = re.sub(r"`[^`]+`", "", content)
    for target in re.findall(r"!?\[[^\]\n]*\]\(([^)\n]+)\)", content):
        if target.startswith(("https://", "http://", "mailto:")):
            continue
        filepart, _, fragment = target.partition("#")
        resolved = (markdown.parent / filepart).resolve() if filepart else markdown
        check(resolved.exists(), f"Broken link {target} in {markdown}")
        if fragment and resolved.is_file() and resolved.suffix == ".md":
            headings = re.findall(r"^#{1,6} (.+)$", resolved.read_text(encoding="utf-8"), re.M)
            check(fragment in {anchor(h) for h in headings}, f"Broken anchor {target} in {markdown}")

sources = sorted(BASE.rglob("*.puml"))
manifest_file = BASE / "diagram-manifest.json"
manifest = json.loads(manifest_file.read_text(encoding="utf-8")) if manifest_file.exists() else {}
for source in sources:
    content = source.read_text(encoding="utf-8")
    image = source.with_suffix(".png")
    check(image.is_file(), f"Missing PNG: {source}")
    if image.is_file():
        data = image.read_bytes()
        check(data[:8] == b"\x89PNG\r\n\x1a\n", f"Invalid PNG: {image}")
        if data[:8] == b"\x89PNG\r\n\x1a\n":
            width, height = struct.unpack(">II", data[16:24])
            check(width > 20 and height > 20, f"Empty image: {image}")
            check(width < 4096 and height < 4096, f"Possible renderer clipping ({width}x{height}): {image}")
        # Checkout timestamps do not indicate whether a committed render is current.
        # Compare the tracked source/render pair to the reviewed content baseline.
        key = source.relative_to(BASE).as_posix()
        expected = manifest.get(key, {})
        check(expected.get("sourceSha256") == hashlib.sha256(content.encode("utf-8")).hexdigest(), f"Changed diagram source needs a reviewed render: {source}")
        check(expected.get("imageSha256") == hashlib.sha256(data).hexdigest(), f"Changed diagram PNG needs a reviewed baseline: {image}")
    check(content.count("@startuml") == 1 and content.count("@enduml") == 1, f"Diagram delimiters: {source}")
    if source.name.startswith("c4-"):
        check(bool(re.search(r"!include <C4/C4_(Context|Container|Component)>", content)), f"Missing C4 include: {source}")
        check("Rel(" in content, f"No macro relationships: {source}")
        check(not re.search(r"^\s*(?:rectangle|component|node)\s", content, re.M), f"Raw C4 shape: {source}")
        check(not re.search(r"\s[-.]+[<>]|[<>][-.]+\s", content), f"Bare C4 arrow: {source}")
        check("!includeurl" not in content and "!include https" not in content, f"Online include: {source}")
    if source.name.startswith("sequence-"):
        check('box "Frontend - ' in content, f"Missing frontend tier: {source}")
        if "design-system" not in source.parts:
            check('box "Backend - ' in content, f"Missing backend tier: {source}")
        for rid in re.findall(r"\bL2-\d+\b", content):
            check(rid in requirements, f"Unknown sequence trace {rid}: {source}")
        check(bool(re.search(r"L2-\d+", content)), f"Untraced sequence: {source}")

runner = None
jar_candidates = [os.environ.get("PLANTUML_JAR", ""), "C:/tools/plantuml.jar", str(Path.home() / "plantuml.jar")]
for candidate in jar_candidates:
    if candidate and Path(candidate).is_file() and shutil.which("java"):
        runner = ["java", "-jar", candidate]
        break
if runner is None and shutil.which("plantuml"):
    runner = ["plantuml"]
if runner:
    for start in range(0, len(sources), 25):
        result = subprocess.run(runner + ["-checkonly", "-charset", "UTF-8"] + [str(p) for p in sources[start:start + 25]], capture_output=True, text=True)
        check(result.returncode == 0, f"PlantUML syntax check failed: {result.stdout}\n{result.stderr}")
else:
    print("PlantUML unavailable: syntax check skipped; artifact and trace checks still run.")

if errors:
    print("\n".join(errors))
    print(f"FAILED: {len(errors)} documentation validation errors")
    sys.exit(1)
print(f"PASS: {len(features)} features, {len(requirements)} L2 requirements, {len(acceptance_ids)} acceptance criteria, {len(sources)} diagram/PNG pairs; links, style, and traceability valid.")
