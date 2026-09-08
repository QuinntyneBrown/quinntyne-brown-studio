# Azure infrastructure and releases

Production is one Ubuntu 24.04 x64 VM in Canada Central with Azure SQL Basic.
Windows development keeps LocalDB. Infrastructure is declared in `infra/main.bicep`
and `infra/monitoring.bicep`; GitHub Actions uses standard Bicep group deployments.
Custom-domain setup in `domain-strategy.md` remains a separate follow-up.

## First deployment

1. Sign in with `az login` and `gh auth login`. Use subscription
   `4a1b5113-89f9-4d27-acfe-581493385536` (Pay-As-You-Go). The operator needs resource
   group creation and role-assignment rights plus repository administration access.
2. Create or select an OpenSSH public key. Keep the private key outside the repository.
   Run the bootstrap from the repository root:

   ```powershell
   python deploy/bootstrap-azure.py --ssh-public-key C:/Users/quinn/.ssh/qbs.pub --administrator-email YOUR_EMAIL
   ```

   This creates tagged `rg-qbs-shared` and `rg-qbs-prod` groups, separate infrastructure
   and release identities, and `infrastructure`/`production` GitHub environments
   restricted to `main`. It refuses to adopt unrelated groups. Both environments
   use OIDC; there are no long-lived Azure credential secrets. It does not alter
   repository branch protection or send test email.
3. Review `infra/main.parameters.production.json`, especially the VM size, regional
   OpenAI model/version and capacity. Check subscription quota before applying.
   No automatic region, model, or SKU fallback occurs. This provisions billable resources.
4. Merge the workflows to `main`. Run **Provision studio infrastructure** with
   `preview`, inspect the Bicep what-if, then run with `apply`. The apply workflow
   repeats what-if, provisions resources, creates the runtime database user, installs
   the host, and records its temporary `https://qbs-…canadacentral.cloudapp.azure.com`
   address. The GitHub infrastructure identity is the SQL Entra administrator;
   the VM identity receives only DDL/read/write database roles. A single runner-IP
   SQL firewall rule exists during user provisioning and is removed in `finally`.
   If the runner is forcibly terminated, remove any `bootstrap-*` rule before proceeding.
5. Re-run the failed deployment job from the initial `main` push, or push the next
   commit. **Verify and package** automatically deploys successful pushes to `main`.
   A manual verification run and a feature branch never deploy production. Deployment
   reads the storage account and origin directly from successful Bicep outputs.
6. Provision the initial administrator once using the existing `--provision-admin`
   command. Supply `Bootstrap__Email` and `Bootstrap__Password` only in a protected
   interactive host session or secret-fed command; never commit them or place them
   in an ordinary Run Command script, workflow log, or artifact. Execute as `qbs`
   with `/opt/studio/config/production.env`. Remove the bootstrap values afterwards.

SSH is closed by default. For an interactive recovery session, temporarily allow
only your current public IP on port 22, use the configured key, then remove that rule.
Do not add a public self-hosted GitHub runner to the VM.

## Release behavior

Verification retains Windows/LocalDB API acceptance, all browser scenarios, the
design-system checks, and adds Linux tests, native-library checks, deployment tests,
and Bicep compilation. Artifacts come from the same workflow run and commit SHA.
The production job checks that SHA is still `main` before starting.

Releases are checksum-verified archives in the private `releases` Blob container.
The build records every file digest and produces deterministic archive metadata.
An existing SHA is never overwritten with different contents. The VM identity
downloads the archive; the installer rejects traversal, links, incomplete packages,
and altered files before stopping services.

GitHub concurrency and a host file lock serialize deployments. A monotonically
ordered workflow run ID prevents stale installations after a newer successful run.
Both services stop before explicit migration. The `current` symlink switches the
API, worker, and all frontend roots together. Successful HTTPS checks and service
liveness record `/opt/studio/state.json`. `/api/health` is a liveness endpoint;
it does not independently prove storage, email, AI, or database readiness after startup.

After activation the workflow runs the browser smoke in `e2e/production` against the origin it just
served. It checks the presented certificate, the marketing site, the calculator deep link, both
sign-in screens, published reads answered from Azure SQL, and refusal of administration data without
an account. A smoke failure fails the deployment job and leaves the activated release in place for
inspection; treat it as a production incident, not a flaky test.

The managed Run Command is checked for both guest execution state and exit code.
Evidence is retained as workflow artifacts and on the VM. Service logs may contain
operational details; restrict artifact access accordingly. Do not place credentials
in deployment environment values: runtime services use the VM identity.

## Failure and recovery

A failed download or validation leaves the running release alone. A migration or
activation failure stops API and worker, records `failure.json`, and keeps release
directories. It never automatically starts old binaries against a possibly changed
database. Inspect deployment evidence and `journalctl -u qbs-api -u qbs-worker`.

For a schema-compatible recovery, run **Roll back studio release** from `main`,
enter a retained full SHA, and acknowledge schema compatibility. The installer
validates retained files, switches the pointer, and verifies HTTPS without running
reverse migrations. Otherwise repair forward, or use Azure SQL point-in-time restore
to a **new database**, update the connection configuration, and explicitly deploy
compatible binaries. Never overwrite the original database during recovery.

Azure SQL Basic retains seven days of point-in-time backups. Blob versions and
soft deletion retain photographs and keys; Key Vault has purge protection. Record
an actual restore drill before claiming the environment qualification gate is closed.
After successful activation the installer retains the five newest extracted
releases plus the active and previous releases, deleting only SHA-named directories
with matching installer manifests. Blob archives remain available for recovery;
manual rollback requires a release still retained on the VM.

## Verification and later domain setup

Run `python3 -m unittest discover -s backend/tests/deployment` on Linux,
`bash -n deploy/linux/setup-host.sh`, `shellcheck deploy/linux/setup-host.sh`,
and `az bicep build --file infra/main.bicep --outfile .artifacts/azure-template.json`.
Run the repository's normal .NET and browser acceptance suites as well.

After first deployment, verify HTTPS, all three application paths, protected-route
denial, sign-in, persistence across service restart, photo upload and worker processing,
and shared data protection. An operator should explicitly initiate a recovery email
and check delivery using the Azure-managed sender. Camera/AI/environment qualification
remains separate evidence, not an inferred pass from successful deployment.

Monitoring includes Application Insights availability, VM heartbeat through Azure
Monitor Agent, and an email action group. Availability can alert before the first
application release; deploy promptly after infrastructure setup.

When DNS is ready, set Bicep `publicOrigin` to the new HTTPS origin, update the email
domain/sender resources, and reapply configuration. This updates Blob CORS, invitation
origin and Caddy together. Do not change secure cookie settings or expose the API on
a separate origin. The independent design-system workflow continues unchanged.

References: [GitHub OIDC](https://learn.microsoft.com/en-us/azure/developer/github/connect-from-azure-openid-connect),
[Azure SQL Entra authentication](https://learn.microsoft.com/en-us/sql/connect/ado-net/sql/azure-active-directory-authentication),
[managed Run Command failure propagation](https://devblogs.microsoft.com/azure-vm-runtime/the-treatfailureasdeploymentfailure-flag/),
[Azure-managed email domains](https://learn.microsoft.com/en-us/azure/communication-services/quickstarts/email/add-azure-managed-domains).
