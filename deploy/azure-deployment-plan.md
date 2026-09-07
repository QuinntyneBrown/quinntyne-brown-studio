# Azure deployment plan for quinntynebrown.studio

Cheapest always-on Azure hosting for the [domain strategy](domain-strategy.md), sized for a solo
photographer with little traffic. "Always-on" is the point: the free and consumption tiers that
cost nothing at idle (App Service Free, Container Apps scale-to-zero, SQL serverless auto-pause)
all make the first visitor wait for a cold start. The cheapest thing on Azure that never sleeps is
a small burstable Linux VM, so that is the host.

Status: **adopted 2026-09-06**. Implementation follows this document: the connection-validator
change, `infra/`, the two workflows, and OD-12 in `docs/specs/decisions.md`.

**This supersedes [OD-10](../docs/specs/decisions.md#od-10--localdb-persistence-and-windows-hosting).**
LocalDB is Windows-only. On Linux the database becomes Azure SQL Database Basic, which needs one
code change (below). LocalDB stays the Windows development default, and the
[runbook](README.md) remains the development procedure until OD-12 replaces it.

Checked on 2026-09-06 with the Azure MCP server: subscription `CLSandbox2`
(`74528fbf-d0fa-4d72-b3ef-dee45c2a8293`) is empty with no policy assignments; the design-system
Static Web App the workflow deploys to is not in it. Domain registered at Namecheap.

## Shape

```text
Namecheap (registrar: lock, auto-renew, 2FA, DS record)
  └─ Azure DNS zone quinntynebrown.studio (DNSSEC)
       ├─ @ / www / clients ──► static public IP ──► NSG (443, 80; 22 from the studio IP only)
       │                              └─ vm-qbs  Ubuntu 24.04, Standard_B2pls_v2, Canada Central
       │                                   caddy ──► api 127.0.0.1:7444 ──► Azure SQL Basic
       │                                   worker ──► Storage Queue / Blob / OpenAI / Email
       └─ design ─────────────────────► Static Web App Free (design system)
```

The VM holds no state. Photos and data-protection keys are in Blob, the key-wrapping key in Key
Vault, the database in Azure SQL. Losing the VM means rebuilding it from the setup script, not
restoring it, so there is no VM backup to pay for.

## Monthly cost (CAD, retail, canadacentral, pricing API on 2026-09-06)

| Item | Rate | Monthly |
| --- | --- | --- |
| VM `Standard_B2pls_v2` Linux, Arm, 2 vCPU / 4 GiB (recommended) | 0.0510/h | ≈ 37 |
| VM `Standard_B1ms` Linux, 1 vCPU / 2 GiB (floor) | 0.0319/h | ≈ 23 |
| VM `Standard_B2als_v2` Linux, x64, 2 vCPU / 4 GiB (if Arm is a problem) | 0.0579/h | ≈ 42 |
| Azure SQL Database Basic, 5 DTU, 2 GB, 7-day point-in-time restore | 0.2453/day | ≈ 7.5 |
| OS disk, Standard SSD E4 32 GB (derived from the E10 quote of 14.64) | | ≈ 4 |
| Standard static public IPv4 | 0.0069/h | ≈ 5 |
| DNS zone, Key Vault, Log Analytics under the free 5 GB, storage account | | ≈ 3–5 |
| Blob storage for originals and derivatives | | ≈ 0.03 per GB |
| Email, Maps, OpenAI | | usage; cents at studio volume |
| Static Web App Free | | 0 |

**Recommended total ≈ CAD 57 a month; floor ≈ CAD 43.** The earlier Windows sizing was ≈ 190
for the VM alone. A one-year reservation on the VM takes roughly a third off once the size is
settled. The 2 GB database limit is fine because photos live in Blob, not in SQL.

Why `B2pls_v2` over the floor: RAW conversion of one 45-megapixel file peaks near 1 GB and pegs a
core for seconds. On `B1ms` an upload batch slows the public site while it runs. Everything the
worker needs already ships for Arm: .NET 10, the `libraw-bin` package that provides `dcraw_emu`,
SkiaSharp's Linux native assets (already referenced), and Caddy. Check `Bpsv2` quota in
`canadacentral` before provisioning; fall back to `B2als_v2` if it is not offered.

## Regions

Everything in `canadacentral` (Toronto time zone, Canadian residency). Azure OpenAI in
`canadaeast` as a Standard regional deployment, because regional `gpt-4o` and `gpt-4.1-mini` are
listed for Canada East only; a Global deployment would process prompts outside Canada.
Communication Services data location `Canada`. Maps, DNS, and Static Web Apps are global.

## The one code change

`LocalDbConnection.Resolve` in `Infrastructure/Persistence` rejects any connection that is not
LocalDB with Windows integrated security. Production on Linux needs:

```text
Server=tcp:<server>.database.windows.net,1433;Database=studio;
Authentication=Active Directory Managed Identity;User Id=<id-qbs-prod client id>;Encrypt=True
```

Change the validator to accept exactly two shapes: the existing LocalDB shape, or an Azure SQL
target with `Authentication=Active Directory Managed Identity` or `Active Directory Default`,
`Encrypt=True`, an explicit non-system database, and no password or attached file. Keep the
development default. Add the acceptance case for the new shape beside the existing validation
tests. Nothing else in the code is Windows-specific: the RAW converter launches whatever
`Raw__Executable` names, data protection to Blob and Key Vault is already implemented, and the
`DefaultAzureCredential` registration picks up the VM's managed identity.

## Resources

Bicep in `infra/` (`main.bicep`, `main.parameters.production.json`), deployed with
`az deployment group what-if` then `create`. Reuse the storage, Key Vault, Log Analytics,
App Insights, Static Web App, and role-assignment blocks from `deploy/legacy/main.bicep`; drop
ACR and Container Apps; keep the SQL server with Entra-only authentication and change the
database SKU to Basic.

| Group | Resource | Notes |
| --- | --- | --- |
| `rg-qbs-shared` | DNS zone | DNSSEC on; DS record at Namecheap |
| | Communication Services + Email service | Custom domain, sender `studio@quinntynebrown.studio` |
| | Azure Maps, Azure OpenAI (`canadaeast`) | Entra auth only |
| | Static Web App Free | `design.quinntynebrown.studio`; new deployment token into `SWA_DESIGN_SYSTEM_DEPLOYMENT_TOKEN` |
| | Log Analytics | 30-day retention |
| | Identity `id-qbs-github` | OIDC for GitHub Actions only |
| `rg-qbs-prod` | VM `vm-qbs`, NIC, NSG, static public IP | Ubuntu 24.04 LTS, SSH key only, Trusted Launch |
| | Identity `id-qbs-prod` | Attached to the VM; `AZURE_CLIENT_ID` in both service units |
| | SQL server (Entra-only, admin = your account) + database `studio` Basic | Firewall: the VM's public IP only; "Allow Azure services" off |
| | Storage account `Standard_LRS`, shared key off, public access off | Containers `photos`, `keys`, `releases`; queue `processing`; CORS for the apex; soft delete on |
| | Key Vault (RBAC, purge protection) | Key `data-protection` |
| | Application Insights | `/api/health` availability test |

Roles for `id-qbs-prod`, scoped to the production resources: Storage Blob Data Contributor,
Storage Queue Data Contributor, Key Vault Crypto User, Cognitive Services OpenAI User, Azure Maps
Data Reader, Contributor on the Communication Services resource. In the database:
`CREATE USER [id-qbs-prod] FROM EXTERNAL PROVIDER` with `db_ddladmin`, `db_datareader`, and
`db_datawriter`, which is enough for `--migrate` and runtime.

## The host

- **Setup script** (`deploy/linux/setup-host.sh`, idempotent): `dotnet-runtime-10.0` and
  `aspnetcore-runtime-10.0`, `libraw-bin`, `caddy`, `azcopy`, Azure Monitor Agent, unattended
  security upgrades, 2 GB swap, user `qbs` with no shell login, directories
  `/opt/studio/releases/<version>`, `/opt/studio/current` symlink, `/var/www/studio/{marketing,admin,client}`.
- **Services**: `qbs-api.service`, `qbs-worker.service`, and Caddy under systemd. Both studio
  units read one environment file; the worker unit differs only in its working directory and DLL.

```ini
# /etc/systemd/system/qbs-api.service
[Unit]
After=network-online.target
Wants=network-online.target

[Service]
User=qbs
EnvironmentFile=/etc/studio/production.env
WorkingDirectory=/opt/studio/current/api
ExecStart=/usr/bin/dotnet QuinntyneBrownStudio.Api.dll
Restart=always
RestartSec=5

[Install]
WantedBy=multi-user.target
```

```ini
# /etc/studio/production.env  (root:qbs, mode 640)
ASPNETCORE_ENVIRONMENT=Production
DOTNET_ENVIRONMENT=Production
ASPNETCORE_URLS=http://127.0.0.1:7444
PublicOrigin=https://quinntynebrown.studio
AZURE_CLIENT_ID=<id-qbs-prod client id>
ConnectionStrings__Studio=Server=tcp:<server>.database.windows.net,1433;Database=studio;Authentication=Active Directory Managed Identity;User Id=<id-qbs-prod client id>;Encrypt=True
Azure__BlobEndpoint=https://<storage>.blob.core.windows.net/
Azure__QueueEndpoint=https://<storage>.queue.core.windows.net/processing
Azure__EmailEndpoint=https://<acs>.canada.communication.azure.com
Azure__EmailSender=studio@quinntynebrown.studio
Azure__MapsClientId=<maps account client id>
Azure__AiEndpoint=https://<aoai>.openai.azure.com/
Azure__AiDeployment=photo-vision
Azure__AiModelVersion=<qualified version>
Retention__AdministratorEmail=<studio address>
DataProtection__BlobUri=https://<storage>.blob.core.windows.net/keys/keyring.xml
DataProtection__KeyUri=https://<vault>.vault.azure.net/keys/data-protection
APPLICATIONINSIGHTS_CONNECTION_STRING=<from App Insights>
Raw__Executable=/usr/bin/dcraw_emu
```

  No password appears anywhere in that file: the database, storage, vault, email, maps, and
  OpenAI calls all authenticate as the VM's managed identity.
- **Publish**: the existing framework-dependent `dotnet publish` output runs unchanged with the
  installed runtime; no Linux-specific build.
- **Caddy**: the domain strategy's Caddyfile with `/var/www/studio/<app>` roots. Proxy on the same
  host, so `Gateway__TrustForwardedHeaders` stays off. HSTS after the first good HTTPS deploy.
- **Access**: SSH on 22 from the studio's IP only, key authentication, no passwords. Update the NSG
  rule when the studio's address changes. No Bastion.

## DNS, TLS, and email

At Namecheap: keep the registration, enable registrar lock, auto-renew, and two-factor
authentication; set the name servers to the four Azure DNS reports. Sign the zone
(`az network dns dnssec-config create`) and enter the DS values at Namecheap.

| Record | Name | Value |
| --- | --- | --- |
| A (alias) | `@` | The public IP resource |
| CNAME | `www`, `clients` | `quinntynebrown.studio` |
| CNAME | `design` | Static Web App default hostname |
| TXT | `@` | ACS verification, and `v=spf1 include:spf.protection.outlook.com -all` (ACS rejects `~all`) |
| CNAME ×2 | `selector1-azurecomm-prod-net._domainkey`, `selector2-…` | DKIM targets ACS lists |
| TXT | `_dmarc` | `v=DMARC1; p=quarantine; rua=mailto:studio@quinntynebrown.studio` |
| CAA | `@` | `0 issue "letsencrypt.org"` |

## Backups and monitoring

Azure SQL Basic keeps 7 days of point-in-time restore automatically. Keys and photos are in Blob
with soft delete; the Key Vault key has purge protection. A monthly `az sql db export` to the
`backups` container gives an off-database copy. Alerts to the studio address: `/api/health`
availability failing, VM heartbeat missing 10 minutes, CPU credits remaining under 20 % (the
signal to size up), database storage over 80 % of 2 GB, budget at 80 % and 100 %.

## CI/CD

The repository is public, so no self-hosted runner on the host. `verify.yml` already builds the
backend and static artifacts. Add `infra.yml` (manual, `what-if` then deploy) and `deploy.yml`
(release tag or manual, `production` environment with a required reviewer): `azure/login@v2`
with OIDC as `id-qbs-github`, upload `release-<version>.zip` to the `releases` container, then
`az vm run-command invoke` runs `/opt/studio/bin/apply-release.sh <version>`, which downloads the
release with the VM identity, runs `--migrate`, swaps the `current` symlink, copies the three
`browser` folders to `/var/www/studio`, restarts the two units, and fails if `/api/health` does
not answer. The previous release directory stays for rollback.

## Staging

Deferred. When wanted: a second Basic database (+ CAD 7.5), a second identity, storage account,
and Key Vault, a second pair of systemd units on `7445`, and a Caddy site for
`staging.quinntynebrown.studio` with `basic_auth` and `X-Robots-Tag: noindex`.

## Go-live, in order

1. Namecheap lock, auto-renew, 2FA; create the Azure DNS zone; switch name servers; sign; DS record.
2. Merge the connection-validator change with its test.
3. Deploy `rg-qbs-shared` and `rg-qbs-prod`; create the database user for `id-qbs-prod`;
   deploy the OpenAI model; note identifiers under G-ENV and G-AI.
4. ACS custom domain: TXT, SPF, DKIM, DMARC; verify; link; send one recovery email to a studio inbox.
5. Run the host setup script; install the release; `--migrate`; `--provision-admin`.
6. Start Caddy; check `https://quinntynebrown.studio/api/health`, the three apps, and the
   `www` and `clients` redirects; walk the invitation flow end to end.
7. Static Web App: deploy the catalog, add `design.quinntynebrown.studio`.
8. Switch HSTS on. Record OD-12 in `docs/specs/decisions.md` and update the runbook.
