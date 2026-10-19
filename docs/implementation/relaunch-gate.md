# Coming-soon relaunch gate

Implemented on 2026-09-13 under [OD-14](../specs/decisions.md#od-14--coming-soon-relaunch-gate) as requirement [L2-078](../specs/L2.md#l2-078--coming-soon-relaunch-gate), designed inside [serve-search-discoverable-pages](../detailed-designs/public-presentation/serve-search-discoverable-pages/README.md). The studio is relaunching and its deployed galleries hold no photographs yet, so the public is kept out of the empty client-rendered pages until they are filled. Each behavior began with a failing acceptance test named for its criterion.

## What was delivered

- One setting, `Launch:ComingSoon`, bound to `LaunchOptions` in `QuinntyneBrownStudio.Application`. `GetLaunchStateHandler` answers `LaunchState` for the requester: true only while the setting is on and the request carries no signed-in account. `LaunchController` publishes it at `GET /api/public/launch`.
- The server-rendered marketing shell (`_Layout.cshtml`) reads that query. While the gate applies it offers About, Blog, Contact, the quote call to action, and client login, drops `Our work` from the footer, and points the brand at `/blog`. The About page omits its portfolio link, and the blog listing, About, and Contact fold the state into their ETags so a copy cached while gated never stands in for the full shell after signing in.
- The Angular marketing application reads the endpoint through `LAUNCH_SERVICE` (`LaunchService` in `api`) and holds the answer in `LaunchGateService` behind `LAUNCH_GATE_SERVICE` (`application`). The `launchGate` guard on the home, portfolio, services, prints, promotions, and public gallery routes performs a full navigation to `/blog` before a gated page renders; the quote route carries no guard. `Shell` filters its navigation and footer to the server-rendered links while the gate applies. The acceptance composition binds `LAUNCH_SERVICE` to a controlled service like every other contract.
- `PublishLaunchArticleCommand` runs at API startup while the setting is on and publishes `LaunchArticle`, the `Coming soon` post, into a blog that holds no article at all. It is ordinary content afterwards.
- The Bicep parameter `comingSoon` writes `Launch__ComingSoon` into the host environment; the production and staging parameter files set it to `true`. The [release runbook](../../deploy/azure-release.md#relaunch-gate) records how to turn it off. The deployed browser smoke accepts either state of the marketing home.

## Decisions taken at implementation

- The gate is decided by the API per request, not by the gateway, so one setting shows the signed-in owner everything and the public only what exists, and the Caddy definition is unchanged.
- A launch state the API cannot supply gates nobody. The blog the gate leads to is rendered by the same API, so there is nowhere better to send a visitor, and a local launch without the API keeps working.
- The administration and client sign-in pages stay open: the studio's own accounts sign in through them, and they already refuse everything else without an account.
- The article is seeded only into a blog with no article at all, published or draft, so an administrator who deletes or replaces it is not overruled on the next restart.
- The setting is deliberately absent from every development configuration; a workstation gates nobody.

## Verification recorded on 2026-09-13

| Check | Result |
| --- | --- |
| Backend acceptance (`dotnet test backend/QuinntyneBrownStudio.slnx -c Release --filter FullyQualifiedName!~LocalDb`, Linux) | 172 passed, 5 skipped (the Windows/SQL-only cases), including the three `LaunchGateAcceptanceTests` cases |
| Angular builds (`npm run build:libs`, `npm run build:apps`, Node 24.15.0) | Passed |
| Application acceptance (`npm test` in `e2e`, the whole suite) | 246 passed across Chromium mobile, tablet, and desktop, including the twelve relaunch-gate executions |
| Deployment tests (`python3 -m unittest discover -s backend/tests/deployment`) | 20 passed |
| `python scripts/verify-architecture.py`, `python docs/detailed-designs/verify.py` | Passed |

The [acceptance register](../detailed-designs/acceptance.md) links every L2-078 criterion to these tests. The deployed browser smoke runs against the next activated release; the gate reaches production once the updated parameter file has been applied by the infrastructure workflow.
