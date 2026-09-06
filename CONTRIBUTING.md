# Contributing to Quinntyne Brown Studio

You can contribute by fixing bugs, improving documentation and accessibility, refining the design system, testing workflows, or proposing features. Participation is governed by the [Code of Conduct](CODE_OF_CONDUCT.md).

## Before you start

Read the [project overview and setup](README.md#run-locally), [repository conventions](AGENTS.md), and the relevant [requirements and decisions](docs/specs/decisions.md). The [detailed designs](docs/detailed-designs/README.md) connect features to their contracts and acceptance criteria.

Search [existing issues](https://github.com/QuinntyneBrown/quinntyne-brown-studio/issues) before opening one. For substantial features, dependencies, or architecture changes, describe the problem, proposed behavior, and acceptance criteria in an issue so maintainers can discuss the scope before implementation. Small fixes and documentation corrections can go directly to a pull request.

Use [SUPPORT.md](SUPPORT.md) for usage questions and [SECURITY.md](SECURITY.md) for private vulnerability reports.

## Development workflow

1. Fork the repository and create a descriptive branch from the current `main` branch.
2. Follow the [local setup](README.md#run-locally). The full application and persistence tests require Windows and LocalDB; the HTML mocks and standalone design system can be explored independently.
3. Make one coherent change, including its documentation and relevant acceptance coverage.
4. Run the checks for the affected area below and inspect the diff for unrelated edits or generated files.
5. Open a pull request against `main` using the provided template. Link the issue and acceptance criteria, explain the resulting behavior, and record the exact validation performed.
6. Address review feedback and rerun checks affected by subsequent edits. Describe any unverified behavior or blocked checks explicitly.

## Acceptance-driven changes

For behavior changes, begin with a failing acceptance test tied to explicit Given-When-Then criteria, then implement the behavior that makes it pass. Include invalid input, permission boundaries, retries, and failure recovery when they are part of the change. Keep the requirements, tests, and implementation aligned.

Backend acceptance tests exercise the API from `backend/tests/`. Database fakes and EF InMemory belong only in explicitly supplied test factories. Persistence scenarios use uniquely named disposable LocalDB databases; never point a test at a development or production database containing studio data.

Frontend acceptance tests live in the standalone `e2e/` project. Each screen has one page object that owns selectors and interactions; tests express intent without selectors. Normal browser acceptance uses controlled service implementations. The separate packaged integration workflow exercises the real HTTP adapters and LocalDB.

Do not add tests that assert directory layout, naming, imports, banned APIs, or specification traceability. Those are code-review and tooling concerns. Documentation-only changes need accurate commands, working links, and a rendering review; they do not need a new application acceptance test.

## Coding conventions

The full rules live in [AGENTS.md](AGENTS.md). The main constraints are:

- Use .NET, Microsoft.Extensions dependency injection, Options, and Configuration. Keep MediatR pinned to **12.5.0**.
- Keep backend dependencies pointing inward. Controllers bind requests, dispatch through MediatR, and return responses; commands, queries, handlers, and validators live in Application feature slices. Use one file per type and matching folders and namespaces.
- Use EF Core's SQL Server provider with LocalDB for normal development and production, including Identity and the outbox. Apply migrations explicitly; database failure must never select an in-memory replacement.
- Place Angular screens and routing in `application`, studio-aware regions in `domain`, and studio-agnostic presentation primitives in `components`. Product projects contain bootstrap code.
- Consume services through an interface and an `InjectionToken`; provider bindings belong in `application`. Keep HTTP adapters in `api` and import shared contracts through `@qbs/domain/models`.
- Prefer signals, keep behavior in services, and split each component's class, template, and styles into separate files.
- Build and review reusable UI in the standalone design system before consuming it. Reference design tokens from component styles; update `design-system/assets/tokens.css` and its `frontend/styles.css` mirror together.

Preserve the existing style and keep formatting changes limited to the files you touch. Keep dependency lockfiles in sync with intentional dependency changes. Include the purpose and license of any added dependency or copied asset, and retain its notices.

## Validation

The [README verification sequence](README.md#build-and-verify) gives the full commands. The [verification workflow](.github/workflows/verify.yml) is the executable reference for CI. Run commands from the repository root unless a working directory is shown below; install dependencies with `npm ci` in each affected JavaScript project first.

| Changed area                                                         | Checks                                                                                                                                                                                                                                                                        |
| -------------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Backend                                                              | `dotnet restore backend/QuinntyneBrownStudio.slnx --locked-mode`, `dotnet build backend/QuinntyneBrownStudio.slnx -c Release --no-restore`, then `dotnet test backend/QuinntyneBrownStudio.slnx -c Release --no-build --logger trx --results-directory .artifacts/acceptance` |
| Angular application or library                                       | In `frontend/`: `npm run build:libs` then `npm run build:apps`. In `e2e/`: `npx playwright install`, `npm run typecheck`, then `npm test`                                                                                                                                     |
| Design system                                                        | In `design-system/`: `npx playwright install`, `npm test`, `npm run build`, then `npm run check:artifact`                                                                                                                                                                     |
| Authentication, persistence, uploads, or cross-application workflows | `./scripts/smoke-platform.ps1`, after installing frontend and e2e dependencies and Playwright browsers                                                                                                                                                                        |
| Component catalog or detailed designs                                | `python scripts/verify-architecture.py` and `python docs/detailed-designs/verify.py`                                                                                                                                                                                          |
| Documentation only                                                   | Check local links and section anchors, preview the Markdown, and compare documented commands with the scripts and manifests they describe                                                                                                                                     |

`npm run test:chromium` in `e2e/` is useful for a focused local iteration; the full browser matrix remains the CI check. For HTML prototype changes, follow the [prototype verification instructions](docs/mocks/README.md#verification).

Detailed-design diagram changes require regenerated and reviewed images and updated manifest hashes; see [Maintaining the documentation](docs/implementation/README.md#maintaining-the-documentation). Do not describe passing controlled tests as camera, capacity, AI, or deployment qualification. Record that evidence through the [qualification workflow](docs/implementation/qualification.md).

## Pull request review

Explain the problem and what changes for a user or operator. Include screenshots for visible changes, using fictional data, and explain migration or configuration effects when applicable. State which checks passed, failed, or were not run and why. Keep credentials, captured mail, private photographs, generated build output, and local database files out of the submission.

Maintainers review correctness, acceptance coverage, accessibility, maintainability, and alignment with the approved scope. Review timing depends on maintainer availability. A proposal can be revised or declined if it conflicts with the project's requirements or supported runtime.

## Contribution licensing and credit

By submitting a contribution, you agree to make your original contribution available under the project's [MIT License](LICENSE), and confirm that you have the right to contribute it. Clearly identify third-party material and retain its license and attribution; see [THIRD_PARTY_NOTICES.md](THIRD_PARTY_NOTICES.md).

The project does not require a separate contributor license agreement or a signed-off-by trailer. Git history and pull requests record contributions; see [CONTRIBUTORS.md](CONTRIBUTORS.md) for recognition of code and non-code work.
