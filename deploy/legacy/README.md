# Superseded deployment archive

These files preserve the original Azure SQL, Container Apps, and Linux-container design. **They are unsupported and must not be used to deploy the current backend.** OD-10 replaced that target with Windows and LocalDB on 2026-09-06.

The Dockerfiles assume their original locations under `deploy/`; commands and template parameters are historical evidence. CI no longer builds these images or invokes the template. Use the [Windows runbook](../README.md) and [accepted decision](../../docs/specs/decisions.md#od-10--localdb-persistence-and-windows-hosting).

Archiving these files removes no deployed resources, databases, or Docker volumes.
