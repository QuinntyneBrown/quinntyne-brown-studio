import { test, expect } from "@playwright/test";
import { AccountPage } from "../page-objects/account-page";

// Given valid product credentials, when a visitor signs in and out, then only
// that product's authorized workspace and navigation open, and sign out returns
// to sign in with protected navigation and its mobile menu removed.
for (const site of ["admin", "client"]) {
  test(`P01 AC-L2-003-01 AC-L2-032-01 ${site} sign in and sign out`, async ({
    page,
  }) => {
    const account = new AccountPage(page);
    account.role = site === "admin" ? "Administrator" : "Client";
    await account.mock();
    await account.open("login", site);
    await account.heading("Welcome back.");
    await account.workspaceNavigationHidden();
    await account.login();
    await account.heading(site === "admin" ? "Sessions" : "Your sessions");
    await account.workspaceNavigationVisible(site);
    await account.signOut();
    await account.heading("Welcome back.");
    await account.workspaceNavigationHidden();
  });

  // Given a logged-out visitor, when any account screen is opened, then no
  // protected workspace links or empty mobile menu are rendered, and the brand
  // link points to the public sign-in route.
  test(`P01 AC-L2-003-01 AC-L2-032-01 ${site} account screens hide protected navigation`, async ({
    page,
  }) => {
    const account = new AccountPage(page);
    await account.mock();
    for (const [path, heading] of [
      ["login", "Welcome back."],
      ["forgot-password", "Find your way back."],
      ["reset-password?token=test", "A fresh start."],
      ["accept-invitation?token=test", "Your photographs await."],
    ]) {
      await account.open(path, site);
      await account.heading(heading);
      await account.workspaceNavigationHidden();
    }
  });

  // Given a logged-out visitor, when a protected route is opened directly,
  // then the guard returns to sign in without exposing workspace navigation.
  test(`P01 AC-L2-003-01 AC-L2-032-01 ${site} protected routes require sign in`, async ({
    page,
  }) => {
    const account = new AccountPage(page);
    await account.mock();
    await account.open(site === "admin" ? "sessions" : "galleries", site);
    await account.signInRoute();
    await account.workspaceNavigationHidden();
  });

  // Given credentials for the other product, when sign-in is attempted, then
  // access is denied and that product's protected navigation remains absent.
  test(`P01 AC-L2-003-01 AC-L2-032-01 ${site} navigation requires the workspace role`, async ({
    page,
  }) => {
    const account = new AccountPage(page);
    account.role = site === "admin" ? "Client" : "Administrator";
    await account.mock();
    await account.open("login", site);
    await account.login();
    await account.message(
      site === "admin"
        ? "This account does not have access to studio administration."
        : "This account does not have access to client collections.",
    );
    await account.passwordRetained();
    await account.workspaceNavigationHidden();
  });

  for (const sessionState of ["expired", "unavailable"]) {
    // Given an authorized workspace, when the next route's session check finds
    // an expired session or fails, then sign in replaces the workspace and its
    // protected navigation is removed instead of retaining stale access.
    test(`P01 AC-L2-003-01 AC-L2-032-01 ${site} ${sessionState} session clears protected navigation`, async ({
      page,
    }) => {
      const account = new AccountPage(page);
      account.authenticated = true;
      account.role = site === "admin" ? "Administrator" : "Client";
      await account.mock();
      await account.open(site === "admin" ? "sessions" : "galleries", site);
      await account.heading(site === "admin" ? "Sessions" : "Your sessions");
      await account.workspaceNavigationVisible(site);
      if (sessionState === "expired") account.authenticated = false;
      else
        account.fixture.failures.set("auth.session", {
          status: 503,
          message: "Session unavailable.",
        });
      await account.navigateWorkspace(
        site === "admin" ? "Equipment" : "Your albums",
      );
      await account.signInRoute();
      await account.workspaceNavigationHidden();
    });
  }
}

// Given valid or invalid invitation/recovery links, when credentials are saved,
// then valid links complete and rejected links preserve the password for correction.
for (const mode of ["accept-invitation", "reset-password"]) {
  test(`P01 AC-L2-062-01 ${mode} handles invalid and successful tokens`, async ({
    page,
  }) => {
    const account = new AccountPage(page);
    account.role = "Client";
    await account.mock();
    await account.open(`${mode}?token=expired`, "client");
    await account.savePassword();
    await account.message("This link is invalid or expired.");
    await account.passwordRetained();
    account.fixture.operations.set(
      mode === "accept-invitation"
        ? "auth.acceptInvitation"
        : "auth.resetPassword",
      () => {
        account.authenticated = true;
      },
    );
    await account.open(`${mode}?token=valid`, "client");
    await account.savePassword();
    if (mode === "accept-invitation") await account.heading("Your sessions");
    else await account.message("Password updated. You can sign in now.");
  });
}

// Given eligible and unknown account addresses, when recovery is requested, then
// the same neutral response is shown without disclosing account existence.
test("P01 AC-L2-062-01 recovery feedback is account neutral", async ({
  page,
}) => {
  const account = new AccountPage(page);
  await account.mock();
  await account.open("forgot-password", "client");
  for (const email of ["known@example.test", "unknown@example.test"]) {
    await account.recover(email);
    await account.message(
      "If the account is eligible, recovery instructions will be sent.",
    );
  }
  expect(account.submissions).toBe(2);
});

test("P01 AC-L2-062-01 an incomplete invitation is corrected without submission", async ({
  page,
}) => {
  const account = new AccountPage(page);
  await account.mock();
  await account.open("accept-invitation", "client");
  await account.savePassword();
  await account.message(
    "This link is incomplete. Open the complete link from your email.",
  );
  await account.passwordRetained();
  expect(account.submissions).toBe(0);
});

// Given a signed-in administrator, when sign out fails, then the workspace
// remains available for retry; successful retry removes protected navigation.
test("P01 AC-L2-032-01 failed sign out offers a recoverable error", async ({
  page,
}) => {
  const account = new AccountPage(page);
  account.authenticated = true;
  account.failLogout = true;
  await account.mock();
  await account.open("sessions");
  await account.heading("Sessions");
  await account.signOut();
  await account.message("Sign out is unavailable. Try again.");
  await account.workspaceNavigationVisible("admin");
  account.failLogout = false;
  await account.signOut();
  await account.heading("Welcome back.");
  await account.workspaceNavigationHidden();
});
