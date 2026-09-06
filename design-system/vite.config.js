import { copyFile, mkdir } from 'node:fs/promises';
import { resolve } from 'node:path';
import { fileURLToPath } from 'node:url';
import { defineConfig } from 'vite';

const root = new URL('.', import.meta.url);
const sourceRoot = fileURLToPath(root);

export default defineConfig({
  appType: 'spa',
  build: {
    rollupOptions: {
      input: {
        index: resolve(sourceRoot, 'index.html'),
        preview: resolve(sourceRoot, 'preview.html'),
      },
    },
  },
  plugins: [
    {
      // The deployed artifact must carry its own contracts: hosts read the
      // navigation configuration and consumers read the component manifest.
      name: 'copy-static-contracts',
      async writeBundle() {
        await mkdir(new URL('dist/', root), { recursive: true });
        await Promise.all([
          copyFile(
            new URL('component-manifest.json', root),
            new URL('dist/component-manifest.json', root),
          ),
          copyFile(
            new URL('staticwebapp.config.json', root),
            new URL('dist/staticwebapp.config.json', root),
          ),
          copyFile(new URL('404.html', root), new URL('dist/404.html', root)),
        ]);
      },
    },
  ],
});
