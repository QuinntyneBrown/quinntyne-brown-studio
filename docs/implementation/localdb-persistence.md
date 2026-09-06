# LocalDB persistence acceptance

The accepted target is LocalDB for normal development and production, with database fakes supplied only by tests. OD-10 records the decision. API and worker run on one Windows host under the same account.

| ID | Given | When | Then |
| --- | --- | --- | --- |
| DB-01 | Development with controlled external adapters and a migrated LocalDB database | An administrator saves rates and studios and the API restarts | Authentication and configuration survive, and the quote uses the saved rates. |
| DB-02 | Production with a migrated LocalDB database | An administrator saves configuration and the API restarts | Authentication and data survive using the normal persistence registration. |
| DB-03 | A committed job in the API database | A fresh worker composition opens the same database | The job is available with its saved state. |
| DB-04 | Missing production configuration or an unsupported connection | The API or worker starts | Startup fails with an actionable message; no memory fallback occurs. |
| DB-05 | Missing database or unapplied migrations | A normal host starts | Startup fails without creating or changing the database. |
| DB-06 | A fresh database, then saved records | Explicit migration runs twice | Schema initialization succeeds and rerunning migration preserves data. |
| DB-07 | A test host with explicitly supplied database fakes | Existing API acceptance cases run | They require no LocalDB connection. |

## Implementation

**Complete, 2026-09-06.** API and worker always register the SQL store and EF SQL Server provider for runtime persistence. The shared LocalDB connection resolver and database lifecycle validate configuration, accessibility, and pending migrations. Development external adapters are independent of storage. The normal hosts contain no `EnsureCreated` or database-fake selection; those implementations and the EF InMemory dependency now belong to the test project. HTTP contracts and existing migrations are unchanged.

Development startup migrates before serving and optionally provisions an administrator; existing accounts persist when credentials are omitted. Production requires explicit LocalDB configuration. EF tooling, migration, and provisioning use the same connection contract. Windows publishing replaces backend container packaging in CI, and superseded cloud/container assets are preserved under `deploy/legacy/`. [AGENTS.md](../../AGENTS.md), [OD-10](../specs/decisions.md#od-10--localdb-persistence-and-windows-hosting), the shared architecture, and the [Windows runbook](../../deploy/README.md) describe the supported target.

## Acceptance evidence

[LocalDbAcceptanceTests](../../backend/tests/QuinntyneBrownStudio.AcceptanceTests/LocalDbAcceptanceTests.cs) covers DB-01 through DB-06. [RuntimeStudioFactory](../../backend/tests/QuinntyneBrownStudio.AcceptanceTests/RuntimeStudioFactory.cs) controls only external dependencies and uses normal persistence and Identity registrations. [ApiCommand](../../backend/tests/QuinntyneBrownStudio.AcceptanceTests/ApiCommand.cs) invokes the real API executable for migration and provisioning. Existing acceptance cases use explicitly injected [fake persistence](../../backend/tests/QuinntyneBrownStudio.AcceptanceTests/FakePersistenceRegistration.cs), covering DB-07; existing SQL transaction and quote cases remain regression coverage.

The captured red run had **10 failures and 1 pass**: development lost authentication after restart, development jobs were absent from a fresh SQL worker composition, unsupported connections were accepted or lacked the required diagnostic, and startup accepted missing databases or unapplied migrations. Production restart persistence already passed. The additional migration/provisioning case verified existing behavior; no historical failing run is claimed for it. Earlier compile and fixture corrections are not counted as product failures.

| Verification | Result |
| --- | --- |
| Final backend Release acceptance | **97 passed, 0 failed, 0 skipped**; 12 new executions, including six real LocalDB cases, plus the 85 existing cases. `dotnet test backend/QuinntyneBrownStudio.slnx -c Release --no-restore`. |
| Migration and provisioning | Fresh database migration, repeated administrator provisioning, login, saved studio creation, repeated migration, and login/data after restart all passed through the real command/HTTP boundaries. |
| Release build and dependencies | Solution build passed with **0 warnings and 0 errors**; locked restore passed. MediatR remains 12.5.0. |
| Windows publishing | API and worker publish successfully with `-p:UseAppHost=false`; neither published directory includes EF InMemory. |
| Published host smoke | API and worker reject missing configuration, non-LocalDB configuration, and a missing database; both pass LocalDB startup in Production and Development. API health responds successfully. All smoke databases are uniquely named and cleaned up. |
| Supporting checks | Development PowerShell syntax, existing repository checker, documentation/diagram verification, and whitespace review passed. |

Local evidence is under `.artifacts/localdb/`: `red/red.trx`, `final/acceptance.trx`, build/publish logs, and `published-smoke.log`. The published-host smoke checks LocalDB startup; it does not claim live Azure queue, Maps, storage, or email qualification. No Windows service, external production migration, or cloud deployment was performed. Backup/restore and host/external-service qualification remain operational evidence under G-ENV. Frontend and design-system behavior did not change; their browser matrices were not rerun for this backend change.
