# quinntynebrown.studio — domain and deployment strategy

A brief plan for putting the three applications on the `quinntynebrown.studio` domain. It builds on
the [Windows and LocalDB runbook](README.md) and [OD-10](../docs/specs/decisions.md#od-10--localdb-persistence-and-windows-hosting):
one Windows host runs the API and worker, and a reverse proxy in front of it serves the three
Angular builds and forwards `/api/`. Nothing here needs a code change; where a future change would
be needed, it says so.

## The recommendation in one table

| Address | Serves | How |
| --- | --- | --- |
| `https://quinntynebrown.studio/` | Marketing site | Apex domain; static `frontend/dist/marketing` |
| `https://quinntynebrown.studio/admin/` | Studio administration | Same host, path; static `frontend/dist/admin` |
| `https://quinntynebrown.studio/client/` | Client collections | Same host, path; static `frontend/dist/client` |
| `https://quinntynebrown.studio/api/` | API | Reverse proxy to the loopback API; never a separate hostname |
| `https://www.quinntynebrown.studio` | Nothing | Permanent redirect to the apex |
| `https://clients.quinntynebrown.studio` | Nothing | Optional permanent redirect to `/client/`, a memorable address for people who lose the email |
| `https://design.quinntynebrown.studio` | Design system catalog | Custom domain on the existing Azure Static Web Apps deployment |
| `https://staging.quinntynebrown.studio` | A second copy of all of the above | Separate database, separate `PublicOrigin`, not indexed |

**One origin, three paths.** The public site sits at the apex and the two signed-in applications sit
under paths on the same host. Subdomains are used only for things that are genuinely separate
origins (the catalog, staging) or as redirects.

## Why paths rather than subdomains for admin and client

The code already decides most of this:

- The sign-in cookie is `__Host-qbs` and the antiforgery cookie is `__Host-qbs-xsrf`. The `__Host-`
  prefix means the browser refuses any `Domain` attribute: the cookie belongs to exactly one host and
  cannot be shared with `admin.` or `clients.` subdomains. That is a deliberate security property,
  not a limitation to work around.
- The API has no CORS policy. Every application reaches it at `/api/` on its own origin, through
  the proxy. Putting the API on `api.quinntynebrown.studio` would require CORS, cross-site cookie
  rules, and a weaker `SameSite` setting.
- The admin and client builds are produced with base hrefs `/admin/` and `/client/`
  (`npm run build:apps`), and every invitation and password-reset email links to
  `{PublicOrigin}/client/accept-invitation?token=…`. One `PublicOrigin` value describes the whole
  deployment.
- The development gateway, the smoke run, the acceptance suite, and the demonstration recordings all
  exercise exactly this shape, so the production layout is the one that has been tested.

The cost of paths is small. Clients see `quinntynebrown.studio/client/…` in their invitation email
and their browser; the optional `clients.` redirect gives them something shorter to type. An
administrator who is signed in at `/admin/` shares the one host cookie with `/client/`, but each
application checks the account's role on every route and refuses the other's workspace, which the
acceptance suite covers.

If the client site should ever become a real host of its own (`clients.quinntynebrown.studio`
serving pages, not redirecting), four things change: the client build's base href becomes `/`, the
invitation link builder in `IdentityAccounts` takes a client origin instead of appending `/client/`,
that host proxies `/api/` to the same API, and the worker's data-protection keys stay shared so
tokens issued on one host are readable on the other. Cookie isolation comes free because
`__Host-` cookies are already per host. Defer this until there is a reason.

## DNS

| Record | Name | Value |
| --- | --- | --- |
| A (and AAAA if the host has IPv6) | `@` | The Windows host's public address |
| CNAME | `www` | `quinntynebrown.studio` |
| CNAME | `clients` | `quinntynebrown.studio` (optional; the proxy redirects it) |
| CNAME | `design` | The Static Web Apps hostname it gives you; add the TXT it asks for to validate |
| A | `staging` | The staging host, or the same host on a second proxy site |
| TXT, CNAME × 2, TXT | `@`, `selector1`/`selector2`, `_dmarc` | The verification, SPF, DKIM, and DMARC records Azure Communication Services Email lists when the domain is added; DMARC starts at `p=quarantine` |
| CAA | `@` | Restrict certificate issuance to the CA the proxy uses (Let's Encrypt, or Cloudflare's if proxied) |

Buy the domain with auto-renew, registrar lock, and DNSSEC enabled, and keep the registrar account
behind a hardware key: the whole studio's identity hangs off this name. Host the zone at a provider
with an API (Cloudflare is the usual choice) so certificate renewal and staging changes can be
scripted.

## TLS and the reverse proxy

The API stays bound to `http://127.0.0.1:7444` and is never reachable from outside. The proxy
terminates TLS, serves the three static builds with an SPA fallback each, forwards `/api/` to the
loopback API with `X-Forwarded-Proto: https`, and issues the redirects. Because the proxy runs on
the same host, the API's default trust of loopback proxies applies and `Gateway:TrustForwardedHeaders`
stays off.

Caddy is the least configuration on Windows: automatic Let's Encrypt certificates, HTTP/2, and
static files in about twenty lines. IIS with URL Rewrite and ARR, or nginx for Windows, do the same
job with more moving parts. A Caddyfile for the layout above:

```caddyfile
www.quinntynebrown.studio, clients.quinntynebrown.studio {
    @clients host clients.quinntynebrown.studio
    redir @clients https://quinntynebrown.studio/client/ permanent
    redir https://quinntynebrown.studio{uri} permanent
}

quinntynebrown.studio {
    encode gzip
    header Strict-Transport-Security "max-age=31536000; includeSubDomains"

    handle /api/* {
        reverse_proxy 127.0.0.1:7444
    }
    handle_path /admin/* {
        root * C:/studio/dist/admin/browser
        try_files {path} /index.html
        file_server
    }
    handle_path /client/* {
        root * C:/studio/dist/client/browser
        try_files {path} /index.html
        file_server
    }
    handle {
        root * C:/studio/dist/marketing/browser
        try_files {path} /index.html
        file_server
    }
}
```

Copy `frontend/dist/*/browser` to the paths named there on each release. Turn HSTS on only after the
first successful HTTPS deployment, and add `preload` only once every subdomain in the table serves
HTTPS.

## Environment values for production

```powershell
$env:PublicOrigin = 'https://quinntynebrown.studio'
$env:ASPNETCORE_URLS = 'http://127.0.0.1:7444'
$env:ConnectionStrings__Studio = 'Server=(localdb)\MSSQLLocalDB;Database=QbsProduction;Integrated Security=true;Encrypt=true;TrustServerCertificate=true'
$env:Azure__EmailSender = 'studio@quinntynebrown.studio'
```

Staging is the same host or a second one with `PublicOrigin = https://staging.quinntynebrown.studio`,
database `QbsStaging`, its own data-protection directory, the proxy's basic authentication in
front of it, and `X-Robots-Tag: noindex`. Invitation emails from staging then carry staging links,
so send them only to studio addresses.

## Go-live, in order

1. Register the domain; enable DNSSEC, lock, auto-renew. Point the zone at the DNS provider.
2. Verify the domain for Azure Communication Services Email and publish SPF, DKIM, and DMARC. Send
   one recovery email to a studio address and check it lands in the inbox, not spam.
3. Publish the API and worker, migrate `QbsProduction`, and provision the administrator, per the
   runbook, with `PublicOrigin` set to the apex.
4. Install the proxy with the configuration above, obtain the certificate, and check
   `https://quinntynebrown.studio/api/health`, the three applications, and the `www` redirect.
5. Walk the invitation path end to end on the real domain: invite a studio address, follow the link,
   set a password, open the assigned gallery. This is the one flow that depends on the domain being
   right.
6. Add the `design` custom domain to Static Web Apps, then `staging`.
7. Switch HSTS on. Take the first backup and record it, as the runbook's backup section requires.

## What this does not do

It does not change OD-10: one Windows host, LocalDB, integrated authentication. It does not put the
API, the admin, or the client on their own hostnames, for the reasons above. It does not decide
hosting for the Windows machine itself (a machine in the studio behind a static IP, or a Windows VM
at a provider both work); whichever it is, only ports 80 and 443 face the internet. The
[Azure deployment plan](azure-deployment-plan.md) works this layout out on a small Azure Linux VM with
Azure SQL Basic and Azure DNS, the domain registered at Namecheap; that plan supersedes OD-10. When the domain
is bought and this layout is adopted, record it as the next decision in
[docs/specs/decisions.md](../docs/specs/decisions.md).
