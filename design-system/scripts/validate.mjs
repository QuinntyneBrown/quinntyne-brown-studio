import { access, readFile, readdir } from 'node:fs/promises';
import { fileURLToPath } from 'node:url';

const root = new URL('../', import.meta.url);
const failures = [];
const fail = (message) => failures.push(message);
const read = (path) => readFile(new URL(path, root), 'utf8');
const exists = async (url) => {
  try {
    await access(url);
    return true;
  } catch {
    return false;
  }
};

const manifest = JSON.parse(await read('component-manifest.json'));
const packageJson = JSON.parse(await read('package.json'));
const componentStyles = await read('assets/components.css');
const docs = await read('assets/docs.js');
const { componentMarkup, dialogMarkup, patternMarkup } = await import('../assets/catalog-content.js');

if (manifest.schemaVersion !== 1) fail('component-manifest.json must use schemaVersion 1.');
if (manifest.product?.version !== packageJson.version) {
  fail('Manifest and package versions must match.');
}
if (!manifest.product?.name || !manifest.product?.description) {
  fail('Manifest product metadata is incomplete.');
}

const categories = new Set((manifest.categories ?? []).map((category) => category.id));
if (!categories.size) fail('Manifest must declare categories.');

const ids = manifest.components.map((component) => component.id);
if (new Set(ids).size !== ids.length) fail('Component identifiers must be unique.');

for (const component of manifest.components) {
  const label = component.id ?? '(unnamed)';
  if (!/^[a-z][a-z0-9-]*$/.test(label)) fail(`Invalid component identifier: ${label}`);
  if (!component.name || !component.description) fail(`Missing name or description for ${label}.`);
  if (!categories.has(component.category)) fail(`Invalid category for ${label}: ${component.category}`);
  if (!component.classes?.length) fail(`${label} must declare the classes it owns.`);
  if (!component.states?.length) fail(`${label} must declare its states.`);
  if (!component.examples?.length) fail(`${label} needs at least one rendered example.`);
  for (const className of component.classes ?? []) {
    if (!componentStyles.includes(`.${className}`)) {
      fail(`${label} declares .${className}, which assets/components.css does not define.`);
    }
  }
  const exampleIds = (component.examples ?? []).map((example) => example.id);
  if (new Set(exampleIds).size !== exampleIds.length) fail(`${label} has duplicate example identifiers.`);
  for (const example of component.examples ?? []) {
    if (!example.id || !example.title || !example.description || !example.markup) {
      fail(`${label} has incomplete example metadata.`);
    }
    if (example.markup && !component.classes.some((className) => example.markup.includes(className))) {
      fail(`${label} example ${example.id} renders none of the classes it documents.`);
    }
    if (componentMarkup(component, example.id) !== example.markup) {
      fail(`${label} example ${example.id} is not resolvable through componentMarkup.`);
    }
  }
}

const checkFamilies = (families, kind, markup) => {
  if (!families?.length) fail(`Manifest must declare ${kind} families.`);
  for (const family of families ?? []) {
    if (!family.id || !family.name || !family.description) fail(`${kind} family metadata is incomplete.`);
    const scenarioIds = (family.scenarios ?? []).map((scenario) => scenario.id);
    if (!scenarioIds.length) fail(`${kind} ${family.id} needs at least one scenario.`);
    if (new Set(scenarioIds).size !== scenarioIds.length) {
      fail(`${kind} ${family.id} has duplicate scenario identifiers.`);
    }
    for (const scenario of family.scenarios ?? []) {
      if (!scenario.id || !scenario.name) fail(`${kind} ${family.id} has incomplete scenario metadata.`);
      if (!markup(family.id, scenario.id)) {
        fail(`${kind} ${family.id}/${scenario.id} has no rendered markup.`);
      }
    }
  }
};
checkFamilies(manifest.patterns, 'Pattern', patternMarkup);
checkFamilies(manifest.dialogs, 'Dialog', dialogMarkup);

for (const required of [
  'renderFoundations',
  'renderIndex',
  'renderComponent',
  'renderPattern',
  'renderDialog',
]) {
  if (!docs.includes(`function ${required}`)) fail(`Documentation application is missing ${required}.`);
}

const requiredScripts = ['start', 'build', 'preview', 'serve:test', 'validate', 'test:browser', 'test'];
for (const script of requiredScripts) {
  if (!packageJson.scripts?.[script]) fail(`Missing package script: ${script}`);
  else if (packageJson.scripts[script].includes('..')) {
    fail(`Package script escapes the design-system folder: ${script}`);
  }
}

const ignoredDirectories = new Set(['node_modules', 'dist', 'test-results', 'playwright-report', 'blob-report']);
const files = [];
const walk = async (directory, relative = '') => {
  for (const entry of await readdir(directory, { withFileTypes: true })) {
    if (ignoredDirectories.has(entry.name)) continue;
    const childUrl = new URL(`${entry.name}${entry.isDirectory() ? '/' : ''}`, directory);
    const childRelative = `${relative}${entry.name}${entry.isDirectory() ? '/' : ''}`;
    if (entry.isDirectory()) await walk(childUrl, childRelative);
    else files.push({ url: childUrl, relative: childRelative });
  }
};
await walk(root);

const forbiddenReferences = [
  ['..', 'frontend'].join('/'),
  ['frontend', 'projects'].join('/'),
  ['..', 'backend'].join('/'),
  ['docs', 'mocks'].join('/'),
];
let tokenDeclarationFiles = 0;
for (const file of files) {
  if (!/\.(?:css|html|js|mjs|json|md)$/.test(file.relative)) continue;
  const contents = await readFile(file.url, 'utf8');
  const normalized = contents.replaceAll('\\', '/');
  for (const reference of forbiddenReferences) {
    if (normalized.includes(reference)) {
      fail(`${file.relative} contains a forbidden external reference: ${reference}`);
    }
  }
  if (/--ink\s*:/.test(contents)) tokenDeclarationFiles += 1;
  if (/\.(?:js|mjs)$/.test(file.relative)) {
    for (const match of contents.matchAll(/(?:from\s+|import\s*)['"](\.\.?\/[^'"]+)['"]/g)) {
      const resolved = new URL(match[1], file.url);
      if (!fileURLToPath(resolved).startsWith(fileURLToPath(root))) {
        fail(`${file.relative} imports outside the design-system folder: ${match[1]}`);
      }
    }
  }
}
if (tokenDeclarationFiles !== 1) {
  fail(`Expected exactly one token declaration file, found ${tokenDeclarationFiles}.`);
}

for (const page of ['index.html', 'preview.html', '404.html']) {
  const html = await read(page);
  for (const match of html.matchAll(/(?:href|src)="([^"]+)"/g)) {
    const reference = match[1];
    if (reference.startsWith('#') || /^(?:https?:|data:)/.test(reference)) continue;
    const local = reference.startsWith('/') ? reference.slice(1) : reference;
    if (!local) continue;
    const target = new URL(local, root);
    if (!fileURLToPath(target).startsWith(fileURLToPath(root))) {
      fail(`${page} reference escapes the folder: ${reference}`);
    } else if (!(await exists(target))) {
      fail(`${page} references a missing file: ${reference}`);
    }
  }
}

if (failures.length) {
  console.error(failures.map((message) => `- ${message}`).join('\n'));
  process.exit(1);
}

const patternScenarios = manifest.patterns.reduce((total, family) => total + family.scenarios.length, 0);
const dialogScenarios = manifest.dialogs.reduce((total, family) => total + family.scenarios.length, 0);
console.log(
  `Validated standalone catalog: ${manifest.components.length} components, ${patternScenarios} screen-pattern states, ${dialogScenarios} dialog scenarios, and local-only assets.`,
);
