import { spawn } from 'node:child_process';
import { createServer } from 'node:http';
import { readFile } from 'node:fs/promises';
import { resolve, extname, sep } from 'node:path';
import { fileURLToPath } from 'node:url';
const frontend = fileURLToPath(new URL('../../frontend/', import.meta.url));
await new Promise((resolveBuild, reject) => {
  const child = spawn(process.execPath, [resolve(frontend, 'node_modules/@angular/cli/bin/ng.js'), 'build', 'marketing', '--configuration', 'production,reckoner-acceptance'], { cwd: frontend, windowsHide: true, stdio: 'inherit' });
  child.on('error', reject); child.on('exit', code => code === 0 ? resolveBuild() : reject(new Error('Studio marketing build failed.')));
});
const root = resolve(frontend, 'dist/marketing/browser');
const mime = { '.js': 'text/javascript', '.css': 'text/css', '.html': 'text/html', '.svg': 'image/svg+xml', '.png': 'image/png', '.webp': 'image/webp', '.ico': 'image/x-icon', '.woff2': 'font/woff2' };
const server = createServer(async (request, response) => {
  try {
    const path = decodeURIComponent(new URL(request.url, 'http://localhost').pathname);
    const target = resolve(root, '.' + path);
    if (target !== root && !target.startsWith(root + sep)) { response.writeHead(400); response.end(); return; }
    const file = extname(target) ? target : resolve(root, 'index.html');
    const body = await readFile(file);
    response.writeHead(200, { 'Content-Type': mime[extname(file)] ?? 'application/octet-stream', 'Cache-Control': 'no-store' }); response.end(body);
  } catch { response.writeHead(404); response.end('Not found'); }
});
server.listen(4420, 'localhost');
process.on('SIGINT', () => server.close()); process.on('SIGTERM', () => server.close());
