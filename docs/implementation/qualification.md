# Qualification commands

`backend/src/Qbs.Qualification` produces evidence for G-RAW, G-UPLOAD, G-AI and G-ENV. It runs real conversion, browser transfers, configured Azure analysis, or read-only environment checks. Missing inputs produce a **Blocked** report with exit code 2. Failed measurements return 1; completed measurements return 0. Every report retains `gateClosed: false`: the studio reviews the measured evidence and records approval in [the evidence register](../specs/decisions.md#evidence-register).

Build and invoke from the repository root with the Windows .NET 10 SDK:

```powershell
dotnet publish backend/src/Qbs.Qualification -c Release -o .artifacts/qualification
dotnet .artifacts/qualification/Qbs.Qualification.dll raw --manifest camera-fixtures.json --report .artifacts/raw-report.json
dotnet .artifacts/qualification/Qbs.Qualification.dll upload --manifest upload-fixtures.json --report .artifacts/upload-report.json
dotnet .artifacts/qualification/Qbs.Qualification.dll ai --manifest ai-fixtures.json --report .artifacts/ai-report.json
dotnet .artifacts/qualification/Qbs.Qualification.dll environment --manifest environment-evidence.json --report .artifacts/environment-report.json
```

Paths in a manifest resolve relative to that manifest. Reports must use a different path from their inputs. Fixture SHA-256 digests pin the exact evidence bytes. Do not commit client photographs, credentials, generated tokens or private qualification reports.

## Camera conversion

A camera manifest contains a nonempty `fixtures` array. Each entry supplies `path`, `camera`, `sha256`, and the expected oriented preview `width` and `height`. Record camera model/encoding and approved visual reference in the camera label and accompanying evidence. Expected dimensions describe the output after the production converter's maximum 2,400-pixel edge resize.

The command invokes the production `IRawPreviewConverter`, verifies the original digest before and after conversion, decodes the emitted JPEG, compares oriented dimensions and writes a preview beside the report. Results include original/preview digests, dimensions, conversion duration and process peak memory. Set `Raw__Executable` to the approved Windows LibRaw executable for RAW input. Inspect the saved previews against the studio's orientation and quality references before approving camera coverage. The automated JPEG regression is a synthetic converter check; it does not qualify a camera.

## Browser upload capacity

Install the pinned dependencies and browsers in `e2e`. The upload manifest supplies:

- `origin`: the HTTPS product origin, with the applications under `/admin` and `/client`.
- `sessionId`: a designated qualification session that the supplied administrator can manage.
- `playwrightModule`: the path to `e2e/node_modules/@playwright/test/index.mjs`.
- `profile`: `capacity` for the OD-04 run or `diagnostic` for a smaller setup check.
- `hostRamGb`: `16` for the approved capacity host; record independent host evidence with the report.
- `maximumHours`: optional bounded duration, default 24.
- `fixtures`: entries containing `path` and `sha256`.

Set `QBS_QUALIFICATION_EMAIL` and `QBS_QUALIFICATION_PASSWORD` only in the runner's environment. `Qualification__NodeExecutable` can identify an installed Node 24 executable. No credential is placed in arguments or reports. The runner validates HTTPS normally; use the host's configured certificate.

Capacity mode requires exactly 1,000 files of exactly 250,000,000 bytes each. It uses desktop Chromium, the actual administrator screen, the application's hash workers and upload adapter, four-transfer limit and resumable blocks. CDP applies 100 Mbps upload and 50 ms latency. After completed work appears, the runner interrupts the browser for at least 60 seconds, reloads, reselects and resumes. It waits for preview readiness and records individual outcomes, actual duration, Chromium version, page heap, observed worker heaps and Windows browser-process memory samples. A worker may finish between discovery and sampling; missing worker/process measurements prevent a capacity measurement pass. Diagnostic mode is explicitly labelled and never substitutes for capacity qualification.

## AI evaluation

Configure `Azure__AiEndpoint`, `Azure__AiDeployment`, `Azure__AiModelVersion` and the Windows operator's Azure credential chain. The manifest supplies `approvedBy`, a studio-approved `minimumAgreement` between 0 and 1, and annotated `fixtures` spanning `Wedding`, `Event`, `Headshot` and `FamilyPortrait`.

Each fixture supplies `path`, `sha256`, `service`, and `expectedOutcomes` with all three keys: `sharpness`, `exposure`, `closed-eyes`. Values are `Promising`, `Issue`, `Uncertain` or `NotApplicable`. The command converts actual input, calls the production Azure adapter and records the full advisory result, model/prompt provenance, duration and agreement with the annotations. Outcome agreement is measurable; usefulness and the representativeness of low light, intentional motion and groups remain studio judgments. Neither the adapter nor the tool selects, publishes or assigns photographs.

## Environment readiness

Configure `ConnectionStrings__Studio` explicitly using the approved named LocalDB connection and integrated authentication. The environment manifest supplies `origin`, `expectedDatabase`, and an `evidence` array with `kind`, `path` and `sha256`. Required kinds are `backup-restore`, `identity-storage-isolation`, `tls`, `monitoring`, `azure-roles` and `email-sender`.

The command verifies the existing LocalDB connection, database identity, applied migrations and absence of pending migrations, then calls the HTTPS health endpoint with certificate validation enabled. It neither migrates nor writes production data. It verifies the supplied operator evidence digests, rather than treating their existence as proof of successful backup/restore or isolation. Review those records against the [Windows runbook](../../deploy/README.md).

## Local implementation evidence

[Qualification acceptance tests](../../backend/tests/Qbs.AcceptanceTests/QualificationAcceptanceTests.cs) were captured failing before command dispatch was implemented, then passed for real JPEG measurement, changed-fixture rejection and blocked reports for all four missing-input modes. Reports from local synthetic checks leave all four external gates open.

A local diagnostic can set `allowLocalCertificate: true` only with the `diagnostic` profile and a loopback origin. The report records that certificate verification was bypassed; capacity and environment qualification always require valid TLS. A one-JPEG diagnostic through the packaged browser/API/LocalDB worker measured readiness in 8.25 seconds locally. Its short-lived hash worker was not captured by the memory sampler, so `memoryMeasured` is false and the run supplies no capacity acceptance evidence.
