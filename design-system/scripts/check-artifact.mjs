/*
 * Serves the built artifact the way the static host will, using the checked-in
 * navigation configuration, and exercises the routes a visitor and the catalog
 * itself depend on. Run it after `npm run build`.
 */
import { createServer } from 'node:http';
import { readFile, stat } from 'node:fs/promises';
import { extname, join } from 'node:path';
import { fileURLToPath } from 'node:url';

const root = fileURLToPath(new URL('../dist/', import.meta.url));
const port = 4199;

let config;
try {
  config = JSON.parse(await readFile(join(root, 'staticwebapp.config.json'), 'utf8'));
} catch {
  console.error('No artifact to check. Run npm run build first.');
  process.exit(1);
}

const types = {
  '.html': 'text/html',
  '.js': 'text/javascript',
  '.css': 'text/css',
  '.json': 'application/json',
  '.svg': 'image/svg+xml',
};

const matches = (pattern, path) =>
  new RegExp(
    '^' +
      pattern
        .replace(/[.]/g, '\\.')
        .replace(/\*/g, '.*')
        .replace(/\{([^}]+)\}/g, (_, group) => `(${group.split(',').join('|')})`) +
      '$',
  ).test(path);

const server = createServer(async (request, response) => {
  const path = decodeURIComponent(request.url.split('?')[0]);
  const send = async (file, status = 200) => {
    const body = await readFile(join(root, file));
    response.writeHead(status, { 'Content-Type': types[extname(file)] ?? 'application/octet-stream' });
    response.end(body);
  };
  try {
    await stat(join(root, path.slice(1)));
    if (path !== '/') return await send(path.slice(1));
  } catch {
    // fall through to the navigation rules
  }
  if (path === '/') return await send('index.html');
  const excluded = (config.navigationFallback.exclude ?? []).some((pattern) => matches(pattern, path));
  if (excluded) return await send(config.responseOverrides['404'].rewrite.slice(1), 404);
  return await send(config.navigationFallback.rewrite.slice(1));
});

await new Promise((resolve) => server.listen(port, '127.0.0.1', resolve));

const manifest = JSON.parse(await readFile(new URL('../component-manifest.json', import.meta.url), 'utf8'));
const checks = [
  ['/', 200, '<title>Quinntyne Brown Studio Design System</title>'],
  [`/components/${manifest.components[0].id}`, 200, 'id="catalog"'],
  [`/patterns/${manifest.patterns[0].id}/${manifest.patterns[0].scenarios[0].id}`, 200, 'id="catalog"'],
  [`/dialogs/${manifest.dialogs[0].id}/${manifest.dialogs[0].scenarios[0].id}`, 200, 'id="catalog"'],
  ['/component-manifest.json', 200, '"schemaVersion"'],
  ['/preview.html', 200, 'id="preview"'],
  ['/404.html', 200, 'That catalog page is not here.'],
  ['/assets/absent-file.js', 404, 'That catalog page is not here.'],
];

const failures = [];
for (const [path, status, needle] of checks) {
  const result = await fetch(`http://127.0.0.1:${port}${path}`);
  const body = await result.text();
  if (result.status !== status || !body.includes(needle)) {
    failures.push(`${path} returned ${result.status} without the expected content`);
  }
}
server.close();

if (failures.length) {
  console.error(failures.map((message) => `- ${message}`).join('\n'));
  process.exit(1);
}
console.log(
  `Artifact checked: ${checks.length} host routes, including deep links, the published manifest, the isolated preview, and the missing-asset response.`,
);
