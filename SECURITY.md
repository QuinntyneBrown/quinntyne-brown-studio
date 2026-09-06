# Security policy

## Reporting a vulnerability

Report suspected vulnerabilities privately to Quinntyne Brown at [quinntynebrown@gmail.com](mailto:quinntynebrown@gmail.com), with the subject `Quinntyne Brown Studio security report`. Do not include exploit details in public issues, pull requests, or comments.

Include as much of the following as is available:

- The affected commit or version, component, and Windows/runtime configuration.
- Reproduction steps or a minimal proof of concept using synthetic accounts and data.
- The expected security boundary and the observed behavior.
- Potential impact, required access, and whether exploitation requires authentication.
- Redacted logs, screenshots, and any proposed mitigation.
- A preferred reply address and whether you want public credit after resolution.

Do not send passwords, access tokens, private client photographs, invitation or recovery links, or unredacted database dumps. If a report needs sensitive attachments, first ask by email how to share them. Test only systems and accounts you own or are authorized to assess.

## Supported scope

Security fixes target the current `main` branch. There is no maintained release/backport schedule at present. Reports concerning older commits are still useful; include the commit and whether the issue also affects current code.

The supported application runtime is the Windows API and worker running under one Windows account with SQL Server Express LocalDB, as defined by [OD-10](docs/specs/decisions.md#od-10--localdb-persistence-and-windows-hosting). The Angular applications, standalone design system, and maintained build and deployment tooling are also within the reporting scope. Assets in `deploy/legacy/` are historical and unsupported.

Relevant issues include authorization failures, exposure of private photographs or account data, unsafe uploads or image processing, invitation and recovery token misuse, cross-site scripting, request forgery, dependency vulnerabilities, and credential exposure. A suspected vulnerability can be reported even if its impact is not yet confirmed.

## Handling reports

The maintainer will review the report, request clarification when needed, and coordinate a fix and disclosure with the reporter. Reports and identifying details are shared only as needed for investigation and remediation. Please coordinate public disclosure until affected users have a mitigation or fix. Reporter credit is given only with consent.

Response and fix timing depend on maintainer availability and the issue's impact; there is no guaranteed service level or paid bug bounty. If you have not received a response, follow up on the same email thread. Security fixes and any advisories will be linked from the repository when published.

## Operating the software

Follow the [Windows operating runbook](deploy/README.md) for HTTPS, Windows identity, database configuration, key storage, migrations, and backup/restore. Controlled development adapters and diagnostic endpoints are for local development. Production needs separately configured and qualified external services; the [evidence register](docs/specs/decisions.md#evidence-register) records the remaining release gates.

For ordinary defects and setup questions, use [SUPPORT.md](SUPPORT.md).
