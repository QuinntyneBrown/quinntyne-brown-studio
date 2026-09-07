import { spawn } from 'node:child_process';
import { fileURLToPath } from 'node:url';
const assembly = process.env.RECKONER_BROWSER_HOST_DLL ?? fileURLToPath(new URL('../../../reckoner/backend/tests/Reckoner.BrowserHost/bin/Debug/net10.0/Reckoner.BrowserHost.dll', import.meta.url));
const child = spawn(process.env.RECKONER_DOTNET ?? 'C:/Program Files/dotnet/x64/dotnet.exe', [assembly, '--BrowserFixture=Studio'], { windowsHide: true, stdio: 'inherit', env: { ...process.env, ASPNETCORE_ENVIRONMENT: 'Development' } });
child.on('error', error => { console.error(error); process.exitCode = 1; });
child.on('exit', code => { process.exitCode = code ?? 1; });
let stopping = false;
async function stop() {
  if (stopping) return; stopping = true;
  try { await fetch('http://127.0.0.1:4390/__test/shutdown', { method: 'POST', headers: { 'X-Reckoner-Test-Run': process.env.RECKONER_TEST_RUN_ID } }); } catch {}
  setTimeout(() => child.kill(), 5000).unref();
}
process.on('SIGINT', stop);
process.on('SIGTERM', stop);
