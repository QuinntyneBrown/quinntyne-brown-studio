import { Page } from "@playwright/test";

/**
 * The captions the demonstrations are narrated with.
 *
 * A recording of a browser driving itself is not a demonstration: a viewer cannot tell an
 * intention from an accident, and half of what the studio does is a rule they cannot see. So each
 * scene says what is about to happen and why it matters, and the recording pauses long enough to
 * read it.
 *
 * The overlay is injected into the page rather than composited afterwards, because there is no
 * video editor in this toolchain and a caption that travels with the frame cannot drift out of
 * sync with it. It is `pointer-events: none` throughout, so nothing here can intercept a click the
 * demonstration is trying to make. Every colour and face is inline and mirrors the design tokens,
 * because the overlay owes nothing to the application's stylesheet.
 *
 * Nothing in this file is used by the acceptance suites. It exists for `demo.spec.ts` alone.
 */

const OVERLAY_ID = "qbs-demo-caption";

/** A chapter card and the moment it appeared, for the chapter table in docs/demo. */
export interface ChapterMark {
  number: string;
  title: string;
  /** Seconds since the narrator started, which is when the recording started. */
  at: number;
}

interface Caption {
  strand: string;
  title: string;
  detail: string;
  chapter: boolean;
  number?: string;
}

/** Roughly how long a reader needs, from the length of what they are being asked to read. */
function readingTime(words: number): number {
  // Captions are short by design, so this is the pace of somebody skimming a line while watching
  // the screen behind it rather than reading prose. The floor gives a two-word caption a moment to
  // land; the ceiling stops one long caption dominating the recording.
  return Math.min(6_500, Math.max(1_500, Math.round((words / 230) * 60_000) + 700));
}

export class Narrator {
  readonly chapters: ChapterMark[] = [];
  private readonly started = Date.now();

  constructor(
    private readonly page: Page,
    /** The eyebrow above every caption, naming which application this recording is about. */
    private readonly strand: string,
  ) {}

  /** Seconds since the recording began. */
  get elapsed(): number {
    return Math.round((Date.now() - this.started) / 100) / 10;
  }

  /**
   * Puts a caption on the screen and waits for it to be read.
   *
   * Returns once the pause is over, so a scene reads as: say what is coming, then do it.
   */
  async say(title: string, detail: string): Promise<void> {
    await this.render({ strand: this.strand, title, detail, chapter: false });
    await this.page.waitForTimeout(readingTime(`${title} ${detail}`.split(/\s+/).length));
  }

  /**
   * A full-screen title card between chapters.
   *
   * The screens change abruptly when a different person signs in or a different application
   * opens, and without a beat between them a viewer reads the cut as a glitch rather than as a
   * new scene. The card clears itself once it has been read: the applications navigate without
   * reloading, so nothing else would take it down before the next caption.
   */
  async chapter(number: string, title: string, detail: string): Promise<void> {
    this.chapters.push({ number, title, at: this.elapsed });
    await this.render({ strand: this.strand, title, detail, chapter: true, number });
    await this.page.waitForTimeout(2_400);
    await this.quiet();
  }

  /** Clears the caption, for the moments the screen should speak for itself. */
  async quiet(): Promise<void> {
    await this.page.evaluate((id) => document.getElementById(id)?.remove(), OVERLAY_ID);
  }

  /**
   * Scrolls the part of the screen being talked about into view.
   *
   * The caption occupies the bottom of the frame, so on several screens the thing being narrated
   * starts below the fold, and a recording that describes a grid the viewer can see only the top
   * edge of is describing nothing.
   */
  async reveal(pixels = 340): Promise<void> {
    await this.page.evaluate((by) => window.scrollBy({ top: by, behavior: "smooth" }), pixels);
    // Long enough for the scroll to finish, so the caption that follows lands on a still frame.
    await this.page.waitForTimeout(750);
  }

  /** Back to the top of the screen, smoothly, before the next thing is shown. */
  async top(): Promise<void> {
    await this.page.evaluate(() => window.scrollTo({ top: 0, behavior: "smooth" }));
    await this.page.waitForTimeout(600);
  }

  /** A pause with the caption left standing, so an action can be watched after it is announced. */
  async beat(milliseconds = 1_200): Promise<void> {
    await this.page.waitForTimeout(milliseconds);
  }

  private async render(caption: Caption): Promise<void> {
    await this.page.evaluate(
      ({ id, text }) => {
        document.getElementById(id)?.remove();
        const overlay = document.createElement("div");
        overlay.id = id;
        overlay.setAttribute("aria-hidden", "true");
        // Ink, soft paper and the accent from design-system/assets/tokens.css, written out so the
        // overlay cannot be restyled by the screen it happens to be sitting on.
        const serif = "Georgia, 'Times New Roman', serif";
        const sans = "Arial, Helvetica, sans-serif";
        const shell = text.chapter
          ? `position:fixed;inset:0;z-index:2147483647;pointer-events:none;display:flex;
             align-items:center;justify-content:center;background:rgba(36,38,32,.96);
             font-family:${sans};color:#f5f5f0;`
          : `position:fixed;left:0;right:0;bottom:0;z-index:2147483647;pointer-events:none;
             padding:22px 36px 26px;background:linear-gradient(transparent,rgba(36,38,32,.94) 28%);
             font-family:${sans};color:#f5f5f0;`;
        overlay.setAttribute("style", shell.replace(/\s+/g, " "));
        overlay.innerHTML = text.chapter
          ? `<div style="max-width:58rem;padding:0 3rem;text-align:center">
               <div style="font-size:13px;letter-spacing:.32em;text-transform:uppercase;opacity:.6;
                           margin-bottom:22px">${text.strand} · ${text.number ?? ""}</div>
               <div style="font-family:${serif};font-size:54px;font-weight:400;line-height:1.1;
                           margin-bottom:20px">${text.title}</div>
               <div style="width:64px;height:2px;background:#8f9a7c;margin:0 auto 22px"></div>
               <div style="font-size:21px;line-height:1.5;opacity:.84">${text.detail}</div>
             </div>`
          : `<div style="max-width:66rem">
               <div style="font-size:12px;letter-spacing:.28em;text-transform:uppercase;opacity:.62;
                           margin-bottom:8px">Quinntyne Brown Studio · ${text.strand}</div>
               <div style="font-family:${serif};font-size:28px;font-weight:400;line-height:1.2;
                           margin-bottom:7px">${text.title}</div>
               <div style="font-size:17px;line-height:1.45;opacity:.88">${text.detail}</div>
             </div>`;
        document.body.appendChild(overlay);
      },
      { id: OVERLAY_ID, text: caption },
    );
  }
}
