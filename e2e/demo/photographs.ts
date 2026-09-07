import { Page } from "@playwright/test";

/**
 * Photograph-like JPEGs for the demonstration, drawn in the browser.
 *
 * The recording uploads real files through the real upload path: the browser hashes them, the
 * API accepts blocks, the worker decodes them and derives previews. A solid rectangle proves the
 * same thing but looks like a bug on screen, so these carry a gradient, some out-of-focus light
 * and a vignette. They are abstractions of a photograph, not stock photography; nothing here is
 * licensed from anybody.
 */

export interface Photograph {
  name: string;
  buffer: Buffer;
}

interface Palette {
  from: string;
  to: string;
  light: string;
  portrait: boolean;
}

const palettes: Record<string, Palette> = {
  "okafor-01.jpg": { from: "#c9a27a", to: "#5b4634", light: "#ffe8c2", portrait: false },
  "okafor-02.jpg": { from: "#8f9a7c", to: "#3f4a35", light: "#eef3d8", portrait: true },
  "okafor-03.jpg": { from: "#d7b39a", to: "#7a4f3f", light: "#fff1e6", portrait: false },
  "okafor-04.jpg": { from: "#7c8fa3", to: "#2f3a49", light: "#e3ecf7", portrait: true },
  "okafor-05.jpg": { from: "#b4a58a", to: "#4a4235", light: "#f7f1df", portrait: false },
  "okafor-06.jpg": { from: "#9c7f8a", to: "#3d2f36", light: "#f3e3ea", portrait: true },
};

/** Every photograph the demonstration uploads, in the order the session receives them. */
export const photographNames = Object.keys(palettes);

export async function photographs(page: Page): Promise<Photograph[]> {
  const files: Photograph[] = [];
  for (const [name, palette] of Object.entries(palettes)) {
    const encoded = await page.evaluate(
      ({ from, to, light, portrait, seed }) => {
        const canvas = document.createElement("canvas");
        canvas.width = portrait ? 1067 : 1600;
        canvas.height = portrait ? 1600 : 1067;
        const graphics = canvas.getContext("2d")!;
        const { width, height } = canvas;
        // A deterministic pseudo-random sequence, so re-recording produces the same pictures.
        let state = seed;
        const random = () => {
          state = (state * 1103515245 + 12345) % 2147483648;
          return state / 2147483648;
        };
        const backdrop = graphics.createLinearGradient(0, 0, width, height);
        backdrop.addColorStop(0, from);
        backdrop.addColorStop(1, to);
        graphics.fillStyle = backdrop;
        graphics.fillRect(0, 0, width, height);
        // Out-of-focus highlights, the way a lens renders lights behind a subject.
        for (let index = 0; index < 14; index++) {
          const radius = 40 + random() * 140;
          const x = random() * width;
          const y = random() * height * 0.7;
          const glow = graphics.createRadialGradient(x, y, 0, x, y, radius);
          glow.addColorStop(0, light + "aa");
          glow.addColorStop(0.6, light + "33");
          glow.addColorStop(1, light + "00");
          graphics.fillStyle = glow;
          graphics.beginPath();
          graphics.arc(x, y, radius, 0, Math.PI * 2);
          graphics.fill();
        }
        // A softly lit shape in the lower third, standing in for the subject.
        const subject = graphics.createRadialGradient(
          width * 0.5,
          height * 0.78,
          0,
          width * 0.5,
          height * 0.78,
          Math.min(width, height) * 0.42,
        );
        subject.addColorStop(0, to + "d0");
        subject.addColorStop(1, to + "00");
        graphics.fillStyle = subject;
        graphics.fillRect(0, 0, width, height);
        // Vignette, so the edges fall away as they do in a print.
        const vignette = graphics.createRadialGradient(
          width / 2,
          height / 2,
          Math.min(width, height) * 0.35,
          width / 2,
          height / 2,
          Math.max(width, height) * 0.75,
        );
        vignette.addColorStop(0, "rgba(0,0,0,0)");
        vignette.addColorStop(1, "rgba(0,0,0,0.55)");
        graphics.fillStyle = vignette;
        graphics.fillRect(0, 0, width, height);
        return canvas.toDataURL("image/jpeg", 0.86).split(",")[1];
      },
      { ...palette, seed: 7 + files.length * 131 },
    );
    files.push({ name, buffer: Buffer.from(encoded, "base64") });
  }
  return files;
}
