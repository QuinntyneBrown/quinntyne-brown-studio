# Support

## Find an answer

| Topic                                                      | Documentation                                                                                                             |
| ---------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------- |
| Local setup and prerequisites                              | [Getting started](README.md#run-locally)                                                                                  |
| LocalDB, migrations, certificates, backups, and deployment | [Windows operating runbook](deploy/README.md)                                                                             |
| Angular builds and browser tests                           | [Frontend guide](frontend/README.md) and [acceptance suite](e2e/README.md)                                                |
| Component catalog and visual patterns                      | [Design-system guide](design-system/README.md)                                                                            |
| Exploring the HTML prototypes                              | [Mock guide](docs/mocks/README.md)                                                                                        |
| Current behavior and release limitations                   | [Implementation status](docs/implementation/README.md) and [evidence register](docs/specs/decisions.md#evidence-register) |
| Contributing a fix                                         | [Contribution guide](CONTRIBUTING.md)                                                                                     |

For local startup failures, first confirm the required SDK and Node versions, that LocalDB is accessible under the current Windows account, and that the frontend libraries and applications have been built. The startup script writes API output to `.artifacts/api.log` and `.artifacts/api-error.log`. Review and redact these files before sharing excerpts.

## Ask a question or report a bug

Search [existing issues](https://github.com/QuinntyneBrown/quinntyne-brown-studio/issues) before opening a [new issue](https://github.com/QuinntyneBrown/quinntyne-brown-studio/issues/new/choose). Choose the question, bug report, or feature request template.

Include the commit or version, affected application, Windows and tool versions, exact commands or reproduction steps, expected and actual results, and what you have already tried. A small reproduction with fictional data is especially useful. For a feature request, explain the user problem and observable acceptance criteria.

Do not post passwords, tokens, invitation or recovery links, private photographs, personal client data, database files, or unredacted environment configuration. Suspected vulnerabilities belong in the private reporting channel in [SECURITY.md](SECURITY.md). Conduct concerns are covered by the [Code of Conduct](CODE_OF_CONDUCT.md).

## Expectations

Support is provided through the repository as maintainer and contributor time permits. There is no guaranteed response time, commercial support agreement, or promise that every feature request will be implemented. Issues may be closed as duplicates, outside the approved scope, or awaiting a reproducible example; additional evidence can support reopening them.

This tracker supports the software project. Photography bookings, client account assistance for a deployed studio, and print-order inquiries should go to the operator of that studio.
