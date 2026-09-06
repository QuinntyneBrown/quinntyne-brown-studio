import { test, expect } from "@playwright/test";
import { AccountPage } from "../page-objects/account-page";

// Given valid product credentials, when a visitor signs in and out, then only
// that product's authorized workspace opens and sign out returns to sign in.
for (const site of ["admin", "client"]) {
  test(`P01 AC-L2-003-01 AC-L2-032-01 ${site} sign in and sign out`, async ({
    page,
  }) => {
    const account = new AccountPage(page);
    account.role = site === "admin" ? "Administrator" : "Client";
    await account.mock();
    await account.open("login", site);
    await account.login();
    await account.heading(site === "admin" ? "Sessions" : "Your sessions");
    await account.signOut();
    await account.heading("Welcome back.");
  });
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

test("P01 AC-L2-003-01 client credentials cannot open administration", async ({
  page,
}) => {
  const account = new AccountPage(page);
  account.role = "Client";
  await account.mock();
  await account.open();
  await account.login();
  await account.message(
    "This account does not have access to studio administration.",
  );
  await account.passwordRetained();
});

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
  account.failLogout = false;
  await account.signOut();
  await account.heading("Welcome back.");
});
