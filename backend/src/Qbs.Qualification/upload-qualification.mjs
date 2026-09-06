import { readFile, writeFile, stat } from "node:fs/promises";
import { createReadStream } from "node:fs";
import { createHash } from "node:crypto";
import { resolve } from "node:path";
import { pathToFileURL } from "node:url";
import { execFile } from "node:child_process";
import { promisify } from "node:util";
import { UploadPage } from "./upload-page.mjs";

const [manifestPath, directory, output] = process.argv.slice(2);
const report = {
  gate: "G-UPLOAD",
  status: "Blocked",
  gateClosed: false,
  message: "",
  observations: [],
};
let browser;
try {
  const manifest = JSON.parse(await readFile(manifestPath, "utf8"));
  const origin = new URL(manifest.origin);
  if (origin.protocol !== "https:" || origin.username || origin.password)
    throw new Error("Supply a credential-free HTTPS product origin.");
  const fixtures = manifest.fixtures;
  if (!Array.isArray(fixtures) || !fixtures.length || fixtures.length > 1000)
    throw new Error("Supply between one and 1,000 pinned fixture files.");
  const capacity = manifest.profile === "capacity";
  if (!capacity && manifest.profile !== "diagnostic")
    throw new Error("Choose the capacity or diagnostic profile explicitly.");
  const localCertificate = manifest.allowLocalCertificate === true;
  if (
    localCertificate &&
    (capacity || !["localhost", "127.0.0.1", "[::1]"].includes(origin.hostname))
  )
    throw new Error(
      "An untrusted local certificate is allowed only for a diagnostic run on loopback.",
    );
  if (capacity && (fixtures.length !== 1000 || manifest.hostRamGb !== 16))
    throw new Error(
      "Capacity qualification requires 1,000 boundary files and the approved 16 GB host profile.",
    );
  const email = process.env.QBS_QUALIFICATION_EMAIL,
    password = process.env.QBS_QUALIFICATION_PASSWORD;
  if (!email || !password)
    throw new Error(
      "Supply QBS_QUALIFICATION_EMAIL and QBS_QUALIFICATION_PASSWORD for a designated qualification administrator.",
    );
  const paths = [];
  for (const fixture of fixtures) {
    const path = resolve(directory, fixture.path);
    const info = await stat(path);
    if (
      info.size < 1 ||
      info.size > 250000000 ||
      (capacity && info.size !== 250000000)
    )
      throw new Error(
        "A fixture does not meet the selected file-size profile.",
      );
    const hash = createHash("sha256");
    for await (const chunk of createReadStream(path)) hash.update(chunk);
    if (
      hash.digest("hex").toLowerCase() !== String(fixture.sha256).toLowerCase()
    )
      throw new Error("A fixture digest differs from the approved manifest.");
    paths.push(path);
  }
  const { chromium, expect } = await import(
    pathToFileURL(resolve(directory, manifest.playwrightModule)).href
  );
  browser = await chromium.launch({ args: ["--enable-precise-memory-info"] });
  const context = await browser.newContext({
    viewport: { width: 1440, height: 900 },
    ignoreHTTPSErrors: localCertificate,
  });
  const page = await context.newPage();
  const screen = new UploadPage(page, expect, origin.origin);
  await screen.signIn(email, password);
  await screen.open(manifest.sessionId);
  const network = await context.newCDPSession(page),
    system = await browser.newBrowserCDPSession();
  await network.send("Network.enable");
  const connection = {
    offline: false,
    latency: 50,
    downloadThroughput: 25000000,
    uploadThroughput: 12500000,
  };
  await network.send("Network.emulateNetworkConditions", connection);
  const started = Date.now(),
    samples = [];
  let interrupted = false,
    interruptionMs = 0;
  const sampleMemory = async () => {
    const pageHeap = await network.send("Runtime.getHeapUsage");
    const { processInfo } = await system.send("SystemInfo.getProcessInfo");
    const ids = processInfo
      .map((process) => Number(process.id))
      .filter((id) => Number.isSafeInteger(id) && id > 0);
    let processes = [];
    if (process.platform === "win32" && ids.length) {
      const { stdout } = await promisify(execFile)(
        "powershell.exe",
        [
          "-NoProfile",
          "-NonInteractive",
          "-Command",
          `Get-Process -Id ${ids.join(",")} -ErrorAction SilentlyContinue | Select-Object Id,WorkingSet64,PeakWorkingSet64 | ConvertTo-Json -Compress`,
        ],
        { windowsHide: true },
      );
      const parsed = stdout.trim() ? JSON.parse(stdout) : [];
      processes = Array.isArray(parsed) ? parsed : [parsed];
    }
    const workers = [];
    const { targetInfos } = await system.send("Target.getTargets");
    for (const target of targetInfos.filter(
      (target) => target.type === "worker",
    )) {
      let sessionId;
      try {
        ({ sessionId } = await system.send("Target.attachToTarget", {
          targetId: target.targetId,
          flatten: false,
        }));
        const memory = await new Promise(async (resolveMemory, reject) => {
          const listener = (event) => {
            if (event.sessionId !== sessionId) return;
            const response = JSON.parse(event.message);
            if (response.id === 1) {
              clearTimeout(timer);
              system.off("Target.receivedMessageFromTarget", listener);
              resolveMemory(response.result ?? null);
            }
          };
          const timer = setTimeout(() => {
            system.off("Target.receivedMessageFromTarget", listener);
            resolveMemory(null);
          }, 1000);
          system.on("Target.receivedMessageFromTarget", listener);
          try {
            await system.send("Target.sendMessageToTarget", {
              sessionId,
              message: JSON.stringify({
                id: 1,
                method: "Runtime.getHeapUsage",
              }),
            });
          } catch (error) {
            clearTimeout(timer);
            system.off("Target.receivedMessageFromTarget", listener);
            reject(error);
          }
        });
        if (memory) workers.push({ targetId: target.targetId, ...memory });
      } catch {
        /* A completed hash worker can disappear between discovery and sampling. */
      } finally {
        if (sessionId)
          await system
            .send("Target.detachFromTarget", { sessionId })
            .catch(() => {});
      }
    }
    samples.push({
      elapsedMs: Date.now() - started,
      pageHeap,
      workers,
      processes,
    });
  };
  await screen.select(paths);
  const maximumMs = (manifest.maximumHours ?? 24) * 3600000;
  let states = [];
  while (Date.now() - started < maximumMs) {
    await sampleMemory();
    states = await screen.states();
    if (
      capacity &&
      !interrupted &&
      states.some((state) => /Ready|Processing/.test(state))
    ) {
      interrupted = true;
      const stopped = Date.now();
      await context.setOffline(true);
      await new Promise((resolveDelay) => setTimeout(resolveDelay, 60000));
      await context.setOffline(false);
      interruptionMs = Date.now() - stopped;
      await screen.resume(paths);
      await network.send("Network.emulateNetworkConditions", connection);
      continue;
    }
    if (
      states.length === paths.length &&
      states.every((state) => /Ready|Rejected|Failed/.test(state))
    )
      break;
    await new Promise((resolveDelay) => setTimeout(resolveDelay, 2000));
  }
  const allReady =
    states.length === paths.length &&
    states.every((state) => /Ready/.test(state));
  report.observations.push({
    name: "TLS policy",
    passed: !localCertificate,
    message: localCertificate
      ? "Loopback diagnostic certificate bypass; TLS deployment is not qualified."
      : "Browser certificate verification enabled.",
    metrics: {},
  });
  const memoryMeasured =
    samples.some((sample) => sample.workers.length) &&
    samples.some((sample) => sample.processes.length);
  const passed =
    allReady &&
    (!capacity || (interrupted && interruptionMs >= 60000 && memoryMeasured));
  report.status = passed ? "Measured" : "Failed";
  report.message = capacity
    ? "Capacity measurements require studio review before G-UPLOAD can close."
    : "Diagnostic sample only. It is not the 1,000-file capacity qualification.";
  report.observations.push({
    name: "Browser upload",
    passed,
    message: allReady
      ? "All selected originals reached preview readiness."
      : "Some files did not reach readiness within the configured run duration.",
    metrics: {
      profile: manifest.profile,
      fileCount: paths.length,
      browserVersion: browser.version(),
      elapsedMs: Date.now() - started,
      interruptionMs,
      uploadBitsPerSecond: 100000000,
      latencyMs: 50,
      memoryMeasured,
      samples,
      states,
    },
  });
} catch (error) {
  report.message = error instanceof Error ? error.message : String(error);
} finally {
  if (browser) await browser.close();
  await writeFile(output, JSON.stringify(report, null, 2));
}
process.exitCode =
  report.status === "Measured" ? 0 : report.status === "Blocked" ? 2 : 1;
