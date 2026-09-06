"""Verify structural requirements independently of runtime acceptance tests."""
from pathlib import Path
import json
import re
import sys

ROOT = Path(__file__).resolve().parents[1]
errors = []
def check(condition, message):
    if not condition: errors.append(message)

for name, allowed in [('Domain', []), ('Application', ['Domain'])]:
    text = (ROOT / f'backend/src/Qbs.{name}/Qbs.{name}.csproj').read_text(encoding='utf-8-sig')
    refs = re.findall(r'ProjectReference Include="[^"]*Qbs\.(\w+)', text)
    check(all(r in allowed for r in refs), f'Clean Architecture dependency violation in {name}')
check('12.5.0' in (ROOT / 'backend/src/Qbs.Application/Qbs.Application.csproj').read_text(), 'MediatR license baseline changed')
for source in (ROOT / 'backend/src/Qbs.Api').rglob('*.cs'):
    if 'obj' in source.parts: continue
    check(not re.search(r'\.Map(?:Get|Post|Put|Delete|Patch)\(', source.read_text()), f'Minimal API product handler: {source}')
manifest = json.loads((ROOT / 'frontend/component-catalog.json').read_text())
catalog_sources = {entry['source'] for entry in manifest}
for source in (ROOT / 'frontend/projects').rglob('*.ts'):
    if '.spec.' in source.name: continue
    text = source.read_text(encoding='utf-8')
    if '@Component(' in text:
        check('templateUrl:' in text or 'templateUrl :' in text, f'Inline or missing template: {source}')
        check(source.with_suffix('.html').exists() and source.with_suffix('.css').exists(), f'Missing component concern: {source}')
        check(str(source.relative_to(ROOT)).replace('\\', '/') in catalog_sources, f'Uncatalogued component: {source}')
    if re.search(r'from [\'"]rxjs', text):
        check('/api/' in str(source).replace('\\', '/'), f'RxJS outside API boundary: {source}')
catalog = json.loads((ROOT / 'design-system/component-manifest.json').read_text(encoding='utf-8'))
published = {'component:' + c['id'] for c in catalog['components']}
published |= {'pattern:' + f['id'] for f in catalog['patterns']}
published |= {'dialog:' + f['id'] for f in catalog['dialogs']}
for entry in manifest:
    check((ROOT / entry['source']).exists(), f'Missing catalog source: {entry["source"]}')
    check(bool(entry['inputs']) and bool(entry['outputs']), f'Undocumented component contract: {entry["name"]}')
    check(entry.get('catalogEntry') in published, f'No design-system entry shows {entry["name"]}: {entry.get("catalogEntry")}')
def css_classes(path):
    text = re.sub(r'/\*.*?\*/', '', Path(path).read_text(encoding='utf-8'), flags=re.S)
    return set(re.findall(r'\.([a-z][a-z0-9_-]*)', text))

# The design system publishes the shared visual language: the studio stylesheet, the component
# library, and the application chrome. Feature-local layout classes stay with their feature.
published = css_classes(ROOT / 'design-system/assets/components.css') | css_classes(ROOT / 'design-system/assets/tokens.css')
consumed = css_classes(ROOT / 'frontend/styles.css') | css_classes(ROOT / 'frontend/projects/application/src/lib/shell/shell.css')
for stylesheet in (ROOT / 'frontend/projects/components/src').rglob('*.css'):
    consumed |= css_classes(stylesheet)
for name in sorted(consumed - published):
    check(False, f'Class .{name} is used by the shared frontend but is not published by the design system')

for project in ['marketing', 'admin', 'client', 'components', 'domain', 'api', 'application']:
    check((ROOT / f'frontend/projects/{project}').is_dir(), f'Missing frontend project: {project}')
if errors:
    print('\n'.join(errors)); sys.exit(1)
print(f'PASS: Clean Architecture, controllers, MediatR pin, separate component files, signal boundary, and {len(manifest)} catalog entries.')
