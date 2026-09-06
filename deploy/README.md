# Deployment and operations

The Bicep template provisions one isolated environment per resource group. Container image builds and template compilation are safe local verification steps; deploying resources, publishing images, assigning external-service roles, and changing DNS are operator release actions. No live deployment has been performed for this implementation.

## Package

From the repository root with a working Linux container engine:

```powershell
docker build -f deploy/Dockerfile.api -t qbs-api:local .
docker build -f deploy/Dockerfile.worker -t qbs-worker:local .
docker build -f deploy/Dockerfile.gateway -t qbs-gateway:local .
az bicep build --file deploy/main.bicep --outfile .artifacts/azure-template.json
```

The worker image installs LibRaw and the Linux image-decoding dependencies. The gateway serves marketing, `/admin/`, and `/client/` with the API behind `/api/`. The design system is built separately with `npm run build` in [`design-system/`](../design-system/README.md); its output is `design-system/dist` and is published to Azure Static Web Apps by [its own workflow](../.github/workflows/deploy-design-system.yml). The artifact carries `staticwebapp.config.json`, so direct component, pattern and dialog URLs resolve, and it makes no product API request.

## Environment rollout

1. Record the target subscription, resource group, region, domain, Entra SQL administrator group, verified email sender, Maps account and approved AI deployment under G-ENV/G-AI. Use separate groups and service resources for development, staging and production.
2. Deploy `main.bicep` with `deployApplications=false`. Required parameters are `environment`, `imageTag`, `publicOrigin`, `sqlAdministratorObjectId`, and `sqlAdministratorName`. This creates ACR, SQL, private storage, queue, Key Vault, identities, monitoring, Container Apps environment and the independent catalog host.
3. Build the three images, publish them to the returned `registryServer`, and record their immutable digests. Use the same release tag for the second template invocation.
4. As the SQL Entra administrator, migrate the database and execute [database-access.sql](database-access.sql) using the identity names returned by the template. Runtime API and worker accounts have data access; schema migration uses a separate operator account. The operator's connection uses encrypted Entra authentication. Run `dotnet Qbs.Api.dll --migrate` with `ConnectionStrings__Studio` set to that operator connection.
5. Grant the API identity **Azure Maps Data Reader** on the selected Maps account. Grant the worker identity **Cognitive Services OpenAI User** on the selected AI resource and the Email send action on the selected Communication Services resource. These existing service scopes are intentionally not guessed by the template. Validate each identity with a real call in staging; assign no product storage or SQL roles to the gateway identity.
6. Deploy again with `deployApplications=true` and the configured service parameters. The same-origin HTTPS domain must match `publicOrigin` and Blob CORS. The API ingress is internal; only the gateway accepts public traffic. Bind the domain and certificate to the gateway, then publish the standalone catalog files to its separate host.
7. Provision the first administrator using `dotnet Qbs.Api.dll --provision-admin`, supplying `Bootstrap__Email` and `Bootstrap__Password` through the operator environment. Remove those variables afterward. No HTTP registration endpoint or built-in production credential exists.
8. Execute staging smoke checks for authentication, upload/SAS isolation, RAW conversion, queues, email, AI, SQL conflicts, retention and recovery. Record results and complete the external evidence gates before production release.

`main.bicep` uses a Basic SQL database and one 2 GiB worker as an initial deployable configuration. These values are not a validated capacity commitment. The measured G-UPLOAD exercise determines production capacity. The SQL firewall permits Azure-hosted connections; Entra authorization remains required. Organizations requiring private endpoints should supply their approved network topology before release.

## Configuration

Environment variables use `__` in place of `:`.

| Setting | Use |
| --- | --- |
| `ConnectionStrings__Studio` | EF Core and Identity SQL connection |
| `PublicOrigin` | Absolute product HTTPS origin used in account links |
| `AZURE_CLIENT_ID` | API or worker managed identity selected by Azure SDKs |
| `Azure__BlobEndpoint` | Storage account Blob endpoint; private `photos` container |
| `Azure__QueueEndpoint` | Full processing queue URL |
| `Azure__MapsClientId` | Maps account client identifier |
| `Azure__EmailEndpoint`, `Azure__EmailSender` | Verified Communication Services Email configuration |
| `Azure__AiEndpoint`, `Azure__AiDeployment`, `Azure__AiModelVersion` | Qualified vision deployment and provenance |
| `Retention__AdministratorEmail` | Recipient of one notice per session-expiry revision |
| `DataProtection__BlobUri`, `DataProtection__KeyUri` | Shared key ring and Key Vault wrapping key for API and worker |
| `Gateway__TrustForwardedHeaders` | Enable only behind the template's private API ingress |
| `APPLICATIONINSIGHTS_CONNECTION_STRING` | API OpenTelemetry export to Application Insights |
| `Raw__Executable` | Optional LibRaw executable override; default `dcraw_emu` |

The API registers the [Azure Monitor OpenTelemetry distribution](https://learn.microsoft.com/en-us/azure/azure-monitor/app/opentelemetry-enable) only when configured outside controlled development. Container console logs flow to the environment's Log Analytics workspace. Job rows hold attempt count, lease, state and sanitized failure text. Queue messages contain job identifiers; account-link email payloads are encrypted with the shared data-protection key ring.

## Recovery and release verification

Use `/api/health` for process liveness, then an authenticated read for database readiness. Inspect failed `BackgroundJob` records and container logs when processing stops; preview and analysis retries are available in session administration. Reconfirm the retention impact to retry a failed deletion. Never remove failed job rows to hide an incident. Queue delivery is at least once; consumers lease jobs and use immutable originals, revision checks, and idempotent email operation identifiers.

Take a database backup before migration. Retain the previous container image digests for an application rollback compatible with the current schema; do not automatically roll a schema backward. Configure and test Azure SQL point-in-time recovery and storage backup policies for the agreed recovery objectives. A confirmed physical photo deletion cannot be undone through the retention editor.

The local Docker engine returned an internal-server error during this run, so container execution remains unverified here. The CI container job builds all three images on Linux and exports them as reviewable artifacts; it does not push or deploy them. Actual Azure identity, DNS, sender, telemetry and restore exercises remain G-ENV evidence.
