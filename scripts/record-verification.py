"""Record actual successful local test results, without asserting external qualification."""
from pathlib import Path
from datetime import datetime, timezone
import json
import hashlib
import subprocess
import xml.etree.ElementTree as ET

ROOT = Path(__file__).resolve().parents[1]
trx = ROOT / '.artifacts/acceptance/backend-green.trx'
browser = ROOT / 'frontend/test-results/results.json'
ns = {'t': 'http://microsoft.com/schemas/VisualStudio/TeamTest/2010'}
tree = ET.parse(trx)
tests = [{'name': item.attrib['testName'], 'outcome': item.attrib['outcome']}
         for item in tree.findall('.//t:UnitTestResult', ns)]
frontend = json.loads(browser.read_text(encoding='utf-8'))
stats = frontend['stats']
if not tests or any(test['outcome'] != 'Passed' for test in tests):
    raise SystemExit('Backend verification contains failed or skipped tests; no passing report written.')
if stats['unexpected'] or stats['skipped'] or stats['flaky']:
    raise SystemExit('Browser verification contains failed, skipped or flaky tests; no passing report written.')
report = {
    'recordedAt': datetime.now(timezone.utc).isoformat(),
    'baseCommit': subprocess.check_output(['git', 'rev-parse', 'HEAD'], cwd=ROOT, text=True).strip(),
    'sourceState': 'Local working tree; not a CI result or an implementation commit.',
    'backend': {'total': len(tests), 'passed': len(tests), 'tests': tests, 'trxSha256': hashlib.sha256(trx.read_bytes()).hexdigest()},
    'frontend': {'passed': stats['expected'], 'unexpected': stats['unexpected'], 'flaky': stats['flaky'],
                 'browsers': ['Chromium', 'Firefox', 'WebKit'], 'viewports': [[390, 844], [768, 1024], [1440, 900]],
                 'reportSha256': hashlib.sha256(browser.read_bytes()).hexdigest()},
    'externalGates': ['G-RAW', 'G-UPLOAD', 'G-AI', 'G-ENV'],
    'evidenceScope': 'Local controlled tests plus three real SQL LocalDB tests. No live Azure qualification.'
}
(ROOT / 'docs/implementation/verification.json').write_text(json.dumps(report, indent=2) + '\n', encoding='utf-8')
print(f'Recorded {len(tests)} backend and {stats["expected"]} browser passes.')
