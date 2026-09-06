# Third-party notices

The [MIT License](LICENSE) covers this project's original code and documentation. It does not replace the licenses of dependencies, development tools, or third-party assets. Preserve the notices that accompany those materials when redistributing them.

## Sample photography

Photographs bundled in `docs/mocks/assets/photos/` are illustrative stock images from Unsplash. Their source identifiers and license reference are recorded in [ATTRIBUTION.md](docs/mocks/assets/photos/ATTRIBUTION.md). These photographs retain the [Unsplash License](https://unsplash.com/license) and are not project-authored images licensed under MIT.

## Dependencies and tools

Exact dependency versions are recorded in the backend `packages.lock.json` files and in the [frontend](frontend/package-lock.json), [acceptance-suite](e2e/package-lock.json), and [design-system](design-system/package-lock.json) npm lockfiles. Dependency license texts and notices ship with the respective packages; this document is a navigation aid, not a complete inventory of their transitive licenses.

MediatR is pinned to **12.5.0**, whose [tagged license](https://github.com/LuckyPennySoftware/MediatR/blob/v12.5.0/LICENSE) is Apache-2.0. The [dependency decision](docs/specs/decisions.md#od-09--implementation-and-deployment) records the selection. The project's MIT license does not change MediatR's terms.

RAW conversion uses the separately installed LibRaw `dcraw_emu` executable. Retain the license and notices supplied with the LibRaw distribution you install. Other external runtimes, database software, and hosted services remain subject to their own terms.

## Adding third-party material

Identify the original source, version, license, and any attribution or notice requirements in the contribution. Include the required license and notice files with vendored material, update relevant lockfiles for package changes, and add asset-specific attribution beside the asset. Link new bundled-material notices from this document so they can be found by redistributors.
