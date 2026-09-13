"""AC-L2-076-04, AC-L2-077-02: the gateway definition routes the server-rendered pages and keeps the no-index header."""
import importlib.util
from pathlib import Path
import unittest

SOURCE = Path(__file__).resolve().parents[3] / "deploy/linux/gateway.py"


class GatewayDefinitionTests(unittest.TestCase):
    def setUp(self):
        spec = importlib.util.spec_from_file_location("gateway", SOURCE)
        self.gateway = importlib.util.module_from_spec(spec)
        spec.loader.exec_module(self.gateway)

    def backend_matcher(self, config):
        line = next(line for line in self.gateway.caddyfile(config).splitlines() if "@backend path" in line)
        return line.split("@backend path", 1)[1].split()

    # Given the reviewed gateway definition, when a release applies it, then /about, /contact, their
    # trailing-slash forms and the site sitemap reach the API instead of the marketing shell.
    def test_AC_L2_076_04_backend_paths_route_about_contact_and_sitemap(self):
        paths = self.backend_matcher({"origin": "https://studio.example"})
        for path in ["/about", "/about/", "/contact", "/contact/", "/sitemap.xml", "/robots.txt", "/blog", "/blog/*", "/api/*"]:
            self.assertIn(path, paths)

    # Given a host configured as no-index, when the gateway answers any address, then it adds the
    # X-Robots-Tag header; an indexable host adds nothing.
    def test_AC_L2_077_02_no_index_hosts_add_x_robots_tag(self):
        indexed = self.gateway.caddyfile({"origin": "https://studio.example"})
        hidden = self.gateway.caddyfile({"origin": "https://staging.example", "noIndex": True})
        self.assertNotIn("X-Robots-Tag", indexed)
        self.assertIn('header X-Robots-Tag "noindex, nofollow"', hidden)
        self.assertLess(hidden.index("X-Robots-Tag"), hidden.index("@backend path"))
