# Staging and production promotion

This runbook is the controlled release path for
`https://staging.quinntynebrown.studio` and
`https://quinntynebrown.studio`. The staging host runs the same API, worker, and
three Angular applications as production, but in its own Azure resource group.

## One-time staging setup

1. Create the `staging` GitHub Environment and require a maintainer approval for
   deployments. Restrict it to `main`.
2. Run **Provision studio infrastructure** from `main` with
   `environment=staging`, first with `operation=preview`, then with
   `operation=apply`. The bootstrap command creates `rg-qbs-staging`, its
   managed identity, and the environment variables used by this workflow.
3. In Namecheap, point the `staging` A record at the staging VM public IP.
   Caddy obtains and renews the HTTPS certificate after DNS resolves.
4. Verify that the infrastructure output origin is exactly
   `https://staging.quinntynebrown.studio`. Do not reuse production's database,
   storage account, Key Vault, managed identity, or environment variables.

The staging parameter file is `infra/main.parameters.staging.json`. It sets the
staging origin and `noIndex`; the Bicep template provisions a separate SQL
database, Blob containers, queue, Key Vault, VM identity, and host. Staging
emails and administrator accounts must use test/studio addresses. Never copy
production photos or credentials into staging.

## Validate a release on staging

1. Find the successful **Verify and package** run on `main`. Record both its
   run ID and the commit SHA. The `studio-release` artifact is immutable and is
   retained for 30 days.
2. Run **Deploy studio release to staging** from `main`, entering that run ID
   and the full SHA. The workflow downloads that exact artifact, verifies its
   filename, deploys it to `qbs-staging`, and runs the production browser smoke
   suite against the staging origin.
3. Check the workflow evidence and manually verify HTTPS, `/`, `/admin/`,
   `/client/`, sign-in, a published read, protected-route denial, an invitation
   using a staging address, photo upload, and worker processing. Record the
   release SHA and the staging workflow URL.

Do not promote a failed or partially validated run. A smoke failure leaves the
release available for inspection but is not approval to continue.

## Promote the exact validated release

After staging validation, a maintainer runs **Promote verified release** from
`main` with the same run ID and SHA. The production Environment approval is
required before the job can activate the release. The workflow checks out the
requested SHA, downloads the artifact from that exact verification run, deploys
it to `qbs-production`, and runs the same HTTPS/browser smoke checks against
production. It does not rebuild from the current branch or accept a different
artifact.

The normal push-to-`main` deployment remains available for routine verified
releases. Use the promotion workflow when staging approval is required; never
manually upload a release to either VM.

## Rollback and verification

If promotion fails, inspect the deployment and smoke artifacts and the host
service logs before making another change. A download or manifest failure leaves
the active release unchanged. A migration or activation failure stops the
services and records `failure.json`; do not start old binaries against a
possibly changed schema.

For a schema-compatible rollback, run **Roll back studio release** from `main`,
enter the retained full SHA, and set `schema_compatible=true`. This switches the
retained release without reverse migrations and runs the HTTPS/browser smoke
checks. If the schema is not compatible, repair forward or restore the affected
Azure SQL database to a new database, update the environment configuration, and
deploy compatible binaries. Never overwrite the original database during
recovery.

After every promotion or rollback, verify the origin certificate, all three
application paths, `/api/health`, sign-in, a published read, and that staging
and production still resolve to different data dependencies. Keep the workflow
evidence with the release record.
