import { test } from "@playwright/test";
import { mkdirSync, writeFileSync } from "node:fs";
import { resolve } from "node:path";
import { AccountPage } from "../page-objects/account-page";
import { ArticleEditorPage } from "../page-objects/blog/article-editor.page";
import { BlogListPage } from "../page-objects/blog/blog-list.page";
import { PublicArticleDetailPage } from "../page-objects/blog/article-detail.page";
import { BlogPost, Studio } from "./demo-data";
import { Narrator } from "./narrate";
import { photograph } from "./photographs";

// One demonstration: writing a blog post from a blank editor to a page a visitor can read.
//
// It runs independently of demo.spec.ts's three-part administration/site/client walkthrough: it
// signs in with the same bootstrap administrator, but neither reads nor depends on anything that
// recording sets up. Run it on its own with:
//
//     ./scripts/record-demo.ps1 -Spec demo/blog-post-demo.spec.ts
//
// or alongside the other recordings by leaving -Spec unset. It lands in docs/demo like the rest.

const origin = process.env["QBS_DEMO_ORIGIN"] ?? "https://localhost:7463";
const documentation = resolve(__dirname, "../../docs/demo");
const chapterLog = resolve(__dirname, "../../.artifacts/demo/chapters");

test("Writing a blog post", async ({ page }) => {
  const narrator = new Narrator(page, "Blog");
  const admin = origin + "/admin";
  const account = new AccountPage(page, origin);
  const editor = new ArticleEditorPage(page, origin);
  const blog = new BlogListPage(page, origin);
  const article = new PublicArticleDetailPage(page, origin);

  // ── Title ────────────────────────────────────────────────────────────────────────────────────
  await account.open("login", "admin");
  await narrator.chapter(
    "A demonstration",
    "Writing a blog post",
    "From a blank editor in the studio's workspace to a page anyone can read, in one take.",
  );
  await narrator.quiet();
  await narrator.beat(1_200);

  // ── 1. Signing in ────────────────────────────────────────────────────────────────────────────
  await narrator.chapter("One", "Signing in", "The same administrator account that runs the rest of the workspace.");
  await account.login(Studio.email, Studio.password);
  await account.heading("Sessions");
  await narrator.say(
    "Blog articles is a workspace link",
    "It sits beside sessions and rates, but it opens the studio's separate editorial workspace.",
  );

  // ── 2. Opening the editor ────────────────────────────────────────────────────────────────────
  await narrator.chapter("Two", "Opening the editor", "A blank article, ready to be written.");
  await page.getByRole("link", { name: "Blog articles", exact: true }).click();
  await page.getByTestId("new-article-btn").click();
  await editor.titleInput.waitFor();
  await narrator.say(
    "Draft until it is published on purpose",
    "The badge beside the title reads Draft from the moment the page opens.",
  );

  // ── 3. Writing it ────────────────────────────────────────────────────────────────────────────
  await narrator.chapter("Three", "Writing it", "A title, an abstract, and the body in Markdown.");
  await editor.fillArticle(BlogPost.title, BlogPost.body, BlogPost.abstract);
  await narrator.say(
    "Markdown, rendered on the public page",
    "Headings and paragraphs typed here become the formatted article a visitor reads.",
  );
  await narrator.reveal(280);
  const cover = await photograph(page, BlogPost.featuredImage.name, { ...BlogPost.featuredImage, portrait: false }, 41);
  await editor.uploadFeaturedImageFile({ name: cover.name, mimeType: "image/jpeg", buffer: cover.buffer });
  await narrator.say(
    "A featured image, uploaded on the spot",
    "It leads the article on the site and its listing card alike.",
  );

  // ── 4. Saving and publishing ─────────────────────────────────────────────────────────────────
  await narrator.chapter("Four", "Saving and publishing", "A draft first, then a decision to publish it.");
  await editor.save();
  await editor.statusBadge.waitFor();
  await narrator.say(
    "Saved as a draft",
    "Nobody outside the studio can see it yet. The URL now names the article that was just created.",
  );
  await editor.publish();
  await narrator.say("Published", "The badge flips, and the article is live at this moment.");

  // ── 5. Reading it on the site ────────────────────────────────────────────────────────────────
  await narrator.chapter("Five", "Reading it on the site", "What a visitor sees, seconds later.");
  await blog.open("/blog");
  await blog.listed();
  await blog.article(BlogPost.title).click();
  await article.title.waitFor();
  await narrator.say(
    "The same title, abstract-free image and body",
    "Nothing was typed twice: the editor's Markdown and featured image are what the visitor sees.",
  );
  await article.assertImageLoaded();
  const video = page.video();
  const duration = narrator.elapsed;
  await page.screenshot({ path: resolve(documentation, "blog-post-poster.png") });
  await narrator.beat(1_200);

  // ── Closing ──────────────────────────────────────────────────────────────────────────────────
  await narrator.chapter(
    "Writing a blog post",
    "Blank page to published, in one take",
    "Signed in, opened the editor, wrote it, added a cover image, saved a draft, published it, and read it back.",
  );
  await page.close();
  await video?.saveAs(resolve(documentation, "blog-post.webm"));
  mkdirSync(chapterLog, { recursive: true });
  writeFileSync(
    resolve(chapterLog, "blog-post.json"),
    JSON.stringify({ name: "blog-post", duration, chapters: narrator.chapters }, null, 2),
  );
});
