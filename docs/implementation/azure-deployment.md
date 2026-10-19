# Azure deployment acceptance

Authority: the approved 2026-09-08 Azure deployment plan and OD-12. Namecheap DNS is
authoritative for `quinntynebrown.studio`, which the production parameters declare as the public
origin. These are behavioral criteria, not source-layout tests.

| Criterion | Given | When | Then | Evidence |
| --- | --- | --- | --- | --- |
| AC-AZ-01 | An encrypted Azure SQL application database with Entra authentication | Production persistence is composed | The configured Azure SQL target is accepted | `AzureConnectionAcceptanceTests` |
| AC-AZ-02 | Unsafe or missing database configuration | The host configures persistence | Configuration fails without credentials in the error or a substitute database | Azure connection tests and existing DB04 API startup cases |
| AC-AZ-03 | A verified complete release | Installed or retried | The exact SHA activates; identical packaging is repeatable | `backend/tests/deployment/test_release.py` |
| AC-AZ-04 | A newer successful deployment | An older workflow attempts activation | No active files or services change | Release acceptance tests |
| AC-AZ-05 | A corrupt package, unsafe archive, or failed migration | Deployment is attempted | Invalid files do not activate; failure is surfaced | Release acceptance tests |
| AC-AZ-06 | An unhealthy activated release | Health verification runs | Success is not recorded; recovery evidence persists; prior state cannot falsely report success | Release acceptance tests |
| AC-AZ-07 | A retained schema-compatible release | Operator requests rollback | Validated files activate without reverse migration | Release acceptance tests |
| AC-AZ-08 | A push to main with passing verification | GitHub completes packaging | The same run's SHA deploys automatically; failed checks, other branches and PRs do not deploy | Met; run 34203990735 |
| AC-AZ-09 | Provisioned production resources | Operator executes live qualification | TLS, authentication, persistence, processing, email, shared keys and restore work | Partial; see the live record below |

Local verification and live provisioning are separate. Bicep compilation validates
resource declarations; it does not prove subscription permission, model quota,
regional capacity, database identity propagation, or application qualification.
Use [the runbook](../../deploy/azure-release.md) to collect those records.

## Live record, 2026-09-08

Production was provisioned into subscription `Pay-As-You-Go`
(`4a1b5113-89f9-4d27-acfe-581493385536`), resource groups `rg-qbs-shared` and
`rg-qbs-prod`, Canada Central. The origin is
`https://qbs-ynal4ns37wb6a.canadacentral.cloudapp.azure.com`.

Verified push `97db06a` to main deployed itself through **Verify and package** run
34203990735 and activated on the VM without a failure record. `e2e/production` then
passed against that origin, in the deployment job and again from an operator
workstation. That establishes, live:

- **TLS.** A publicly trusted certificate, obtained by the host itself, on every
  application route. No test relaxes certificate checking.
- **Persistence.** The VM's managed identity applied migrations to Azure SQL and the
  API answers published reads from it. No password exists anywhere in the deployment.
- **Isolation.** Administration data is refused without an account; the SQL server
  admits only the VM's address and only Entra authentication.
- **Delivery.** Marketing, calculator, administration and client applications are all
  served from one origin behind the gateway.

**Not yet qualified** and still required by G-ENV: email delivery through Azure
Communication Services, Azure AI suggestions, Azure Maps routing (the calculator
needs studio configuration first), the photo upload and worker path against real blob
storage, and a database restore rehearsal. `/api/health` is a liveness endpoint and
proves none of them.

First provisioning cost five defects, all now fixed: federated credentials that did
not match this repository's immutable OIDC subjects; an Azure Maps role definition
that does not exist; a managed run command read through a Windows batch wrapper, where
an unquoted `&` ended the command; a migration guard that demanded Windows whatever
the target; and three scripts that hid the CLI's reason for failing behind a
traceback. The subscription also had zero `standardBasv2Family` cores in Canada
Central; that quota was raised to 4 and needed no template change.

Local evidence, 2026-09-08: Windows backend suite passed 129/129; Linux ARM64
backend suite passed 118 with 12 existing Windows/LocalDB-only cases skipped.
Deployment acceptance passed 13 scenarios, including a real two-process lock test.
Bicep compilation, actionlint, shellcheck, PowerShell parsing, and repository
architecture/documentation checks passed. Azure provisioning, GitHub workflow
execution and production qualification have not been performed by these checks.
