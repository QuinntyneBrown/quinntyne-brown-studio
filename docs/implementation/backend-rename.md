# Backend name

The backend solution and all seven projects use `QuinntyneBrownStudio`: Domain,
Application, Infrastructure, API, Worker, Qualification and AcceptanceTests. Project
folders, project references, namespaces, assembly names, launch profiles, migration
metadata and NuGet project identities follow the same prefix. CI, local startup,
publishing, qualification commands and maintained documentation reference the renamed
projects. The backend labels in the existing diagrams are rendered again.

This is a mechanical refactor with regression acceptance. No product behavior or
database schema is added, and no naming/architecture acceptance tests are introduced.
Existing files and the earlier uncommitted feature implementation are preserved.

Database names (including the existing LocalDB development database), cookie names,
encryption purposes, environment-variable contracts and the worker's opaque
`UserSecretsId` remain stable so stored data, protected jobs and existing configuration
remain accessible. Angular package names and selectors belong to the frontend and
are unchanged. Earlier verification JSON files describe their original runs, including
the assembly names at that time.

Verification is recorded in `.artifacts/backend-rename/`: locked restore, Release
build, the complete backend acceptance suite, published API/worker/qualification
artifacts, runtime checks, and detailed-design validation. The final results are
summarized in [backend-rename-verification.json](backend-rename-verification.json).
