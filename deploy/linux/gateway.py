"""The reverse-proxy configuration. One definition, applied by host preparation and by every release.

Routes belong to the code that serves them. Host preparation is not repeated on release, so a
release that publishes a new address — the blog did — would otherwise reach a gateway that still
falls back to the marketing shell for it. Every release rewrites this file from the reviewed
definition below, and the release health check then proves the addresses answer from the API.
"""
import re
import subprocess
import urllib.parse
from pathlib import Path

CADDYFILE = Path("/etc/caddy/Caddyfile")

# Addresses the API serves. Everything else is a static application build, and anything the
# gateway does not name here silently becomes the marketing shell.
BACKEND_PATHS = "/api/* /blog /blog/* /robots.txt"


def origin_of(config):
    origin = urllib.parse.urlsplit(config["origin"])
    if origin.scheme != "https" or origin.path or origin.query or origin.fragment or not re.fullmatch(r"[a-z0-9.-]+", origin.netloc):
        raise ValueError("Expected an HTTPS DNS origin")
    return origin


def hostname(value, origin):
    if not re.fullmatch(r"[a-z0-9][a-z0-9.-]*", value) or value == origin.netloc:
        raise ValueError("Expected a DNS hostname other than the origin")
    return value


def caddyfile(config):
    origin = origin_of(config)
    # Names that must reach the studio without serving it. One origin serves the applications,
    # so every other name answers with a permanent redirect and nothing else.
    redirects = [hostname(name, origin) for name in config.get("redirects", [])]
    clientRedirect = hostname(config["clientRedirect"], origin) if config.get("clientRedirect") else ""
    sites = []
    if redirects:
        sites.append(", ".join(redirects) + " {\n    redir " + config["origin"] + "{uri} permanent\n}")
    if clientRedirect:
        sites.append(clientRedirect + " {\n    redir " + config["origin"] + "/client/ permanent\n}")
    sites.append('''HOSTNAME {
    encode gzip
    header X-Content-Type-Options nosniff
    INDEX
    @backend path BACKEND
    handle @backend {
        reverse_proxy 127.0.0.1:7444
    }
    redir /admin /admin/ 308
    redir /client /client/ 308
    handle_path /admin/* {
        root * /opt/studio/current/admin
        try_files {path} /index.html
        file_server
    }
    handle_path /client/* {
        root * /opt/studio/current/client
        try_files {path} /index.html
        file_server
    }
    handle {
        root * /opt/studio/current/marketing
        try_files {path} /index.html
        file_server
    }
}
'''.replace("HOSTNAME", origin.netloc).replace("BACKEND", BACKEND_PATHS)
   .replace("INDEX", '    header X-Robots-Tag "noindex, nofollow"\n' if config.get("noIndex") else ""))
    return "\n".join(sites)


def apply(config):
    """Write, validate, and load the gateway configuration. Never leaves an unvalidated file loaded."""
    CADDYFILE.write_text(caddyfile(config))
    subprocess.run(["caddy", "validate", "--config", str(CADDYFILE)], check=True)
    subprocess.run(["systemctl", "restart", "caddy"], check=True)
