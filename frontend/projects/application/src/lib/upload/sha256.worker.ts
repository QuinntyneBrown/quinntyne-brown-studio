/// <reference lib="webworker" />
import { createSHA256 } from 'hash-wasm';
addEventListener('message', async ({ data }: { data: File }) => {
  try {
    const hash = await createSHA256();
    hash.init();
    for (let offset = 0; offset < data.size; offset += 8 * 1024 * 1024) {
      hash.update(new Uint8Array(await data.slice(offset, offset + 8 * 1024 * 1024).arrayBuffer()));
    }
    postMessage({ digest: hash.digest('hex') });
  } catch {
    postMessage({ error: 'Unable to read this file.' });
  }
});
