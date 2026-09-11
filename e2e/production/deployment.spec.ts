import { test } from "@playwright/test";
import { DeployedStudio } from "../page-objects/deployed-studio";
import { PublicSitePage } from "../page-objects/public-site-page";
import { QuotePage } from "../page-objects/quote-page";
import { AccountPage } from "../page-objects/account-page";
import { BlogListPage } from "../page-objects/blog/blog-list.page";

const origin = DeployedStudio.origin();

// Given provisioned Azure production resources and the release activated from main,
// when the deployed origin is visited over the certificate its gateway presents,
// then the marketing site, calculator, blog, administration and client applications
// are served, the API answers published reads from Azure SQL, and administration data
// stays refused without an account.
test("AC-AZ-09 AC-L2-068-01 AC-L2-070-07 the deployed studio serves every application over trusted TLS", async ({
  page,
  request,
}, info) => {
  const studio = new DeployedStudio(request, origin);
  await studio.live();
  await studio.reads("galleries");
  await studio.reads("studios");
  await studio.refusesAnonymously("admin/sessions");
  await studio.redirects("/admin", "/admin/");
  await studio.redirects("/client", "/client/");
  await studio.servesTheBlog();
  await studio.redirects("/blog/", "/blog", 301);

  const publicSite = new PublicSitePage(page, origin);
  await publicSite.open();
  await publicSite.heading("Photography with feeling.");
  await publicSite.loaded();
  await publicSite.capture(info.outputPath("marketing-home.png"));

  // A deep link proves the gateway falls back to the application shell rather than 404.
  const quote = new QuotePage(page, origin);
  await quote.openLive();
  await quote.capture(info.outputPath("marketing-quote.png"));

  // The studio blog is a Razor page behind the same gateway; a proxy that has not been given
  // its route serves the marketing shell here instead, which is not a 404 and not a failure.
  const blog = new BlogListPage(page, origin);
  await blog.open();
  await blog.listed();
  await blog.capture(info.outputPath("blog-list.png"));

  const administrator = new AccountPage(page, origin);
  await administrator.open("login", "admin");
  await administrator.heading("Welcome back.");
  await administrator.capture(info.outputPath("admin-login.png"));

  const client = new AccountPage(page, origin);
  await client.open("login", "client");
  await client.heading("Welcome back.");
});

// Given hostnames the studio claims but does not serve, when one is visited over its own
// certificate, then it answers with a permanent redirect to the one origin that does.
test("AC-AZ-09 every other studio hostname redirects to the origin", async ({
  request,
}) => {
  const aliases = DeployedStudio.aliases();
  test.skip(
    !aliases.redirects.length && !aliases.client,
    "This deployment serves its origin under no other name.",
  );
  const studio = new DeployedStudio(request, origin);
  for (const alias of aliases.redirects)
    await studio.permanentlyRedirects(alias, "/");
  if (aliases.client) await studio.permanentlyRedirects(aliases.client, "/client/");
});
