# Azure deployment acceptance

Authority: the approved 2026-09-08 Azure deployment plan and OD-12. The custom
domain remains deferred. These are behavioral criteria, not source-layout tests.

| Criterion | Given | When | Then | Evidence |
| --- | --- | --- | --- | --- |
| AC-AZ-01 | An encrypted Azure SQL application database with Entra authentication | Production persistence is composed | The configured Azure SQL target is accepted | `AzureConnectionAcceptanceTests` |
| AC-AZ-02 | Unsafe or missing database configuration | The host configures persistence | Configuration fails without credentials in the error or a substitute database | Azure connection tests and existing DB04 API startup cases |
| AC-AZ-03 | A verified complete release | Installed or retried | The exact SHA activates; identical packaging is repeatable | `backend/tests/deployment/test_release.py` |
| AC-AZ-04 | A newer successful deployment | An older workflow attempts activation | No active files or services change | Release acceptance tests |
| AC-AZ-05 | A corrupt package, unsafe archive, or failed migration | Deployment is attempted | Invalid files do not activate; failure is surfaced | Release acceptance tests |
| AC-AZ-06 | An unhealthy activated release | Health verification runs | Success is not recorded; recovery evidence persists; prior state cannot falsely report success | Release acceptance tests |
| AC-AZ-07 | A retained schema-compatible release | Operator requests rollback | Validated files activate without reverse migration | Release acceptance tests |
| AC-AZ-08 | A push to main with passing verification | GitHub completes packaging | The same run's SHA deploys automatically; failed checks, other branches and PRs do not deploy | Workflow runtime evidence required |
| AC-AZ-09 | Provisioned production resources | Operator executes live qualification | TLS, authentication, persistence, processing, email, shared keys and restore work | Live qualification required; not established by local tests |

Local verification and live provisioning are separate. Bicep compilation validates
resource declarations; it does not prove subscription permission, model quota,
regional capacity, database identity propagation, or application qualification.
Use [the runbook](../../deploy/azure-release.md) to collect those records.

Local evidence, 2026-09-08: Windows backend suite passed 129/129; Linux ARM64
backend suite passed 117 with 12 existing Windows/LocalDB-only cases skipped.
Deployment acceptance passed 13 scenarios, including a real two-process lock test.
Bicep compilation, actionlint, shellcheck, PowerShell parsing, and repository
architecture/documentation checks passed. Azure provisioning, GitHub workflow
execution and production qualification have not been performed by these checks.
