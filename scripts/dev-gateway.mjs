import https from 'node:https';
import http from 'node:http';
import { readFileSync, existsSync, statSync } from 'node:fs';
import { resolve, join, extname, sep } from 'node:path';
import { fileURLToPath } from 'node:url';

const root = resolve(fileURLToPath(new URL('..', import.meta.url)));
const key = process.env.QBS_TLS_KEY, cert = process.env.QBS_TLS_CERT;
if (!key || !cert) throw new Error('Set QBS_TLS_KEY and QBS_TLS_CERT to the development certificate files.');
const types = { '.html': 'text/html', '.js': 'application/javascript', '.css': 'text/css', '.json': 'application/json', '.jpg': 'image/jpeg', '.png': 'image/png', '.ico': 'image/x-icon' };
const tls = { key: readFileSync(key), cert: readFileSync(cert) };
const handler = catalogOnly => (request, response) => {
  const url = new URL(request.url ?? '/', 'https://localhost:7443');
  if (!catalogOnly && url.pathname.startsWith('/api/')) {
    const upstream = http.request({ host: '127.0.0.1', port: 7444, path: request.url, method: request.method, headers: { ...request.headers, 'x-forwarded-proto': 'https' } }, incoming => {
      response.writeHead(incoming.statusCode ?? 502, incoming.headers); incoming.pipe(response);
    });
    upstream.on('error', () => { response.writeHead(502, { 'Content-Type': 'application/json' }); response.end(JSON.stringify({ title: 'The local API is not running.' })); });
    request.pipe(upstream); return;
  }
  const site = catalogOnly ? 'design-system' : /^\/admin(?:\/|$)/.test(url.pathname) ? 'admin' : /^\/client(?:\/|$)/.test(url.pathname) ? 'client' : 'marketing';
  if (!catalogOnly && site !== 'marketing' && url.pathname === '/' + site) { response.writeHead(308, { Location: '/' + site + '/' }); response.end(); return; }
  const base = join(root, 'frontend', 'dist', site, 'browser');
  let local;
  try { local = decodeURIComponent(catalogOnly || site === 'marketing' ? url.pathname : url.pathname.slice(site.length + 1)); }
  catch { response.writeHead(400); response.end(); return; }
  let file = resolve(base, '.' + local);
  if (!file.startsWith(base + sep) && file !== base) { response.writeHead(404); response.end(); return; }
  if (!existsSync(file) || statSync(file).isDirectory()) file = join(base, 'index.html');
  if (!existsSync(file)) { response.writeHead(503); response.end('Build the frontend applications first.'); return; }
  response.writeHead(200, { 'Content-Type': types[extname(file)] ?? 'application/octet-stream', 'Cache-Control': 'no-store' }); response.end(readFileSync(file));
};
https.createServer(tls, handler(false)).listen(7443, '127.0.0.1', () => console.log('Studio: https://localhost:7443 · Admin: /admin/ · Client: /client/'));
http.createServer(handler(true)).listen(7445, '127.0.0.1', () => console.log('Independent catalog: http://127.0.0.1:7445'));
