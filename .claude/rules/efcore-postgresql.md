---
paths:
  - "**/*.cs"
---

# EF Core / PostgreSQL Rules

EF Core and PostgreSQL are part of the planned stack (see [AGENTS.md](../../AGENTS.md)); apply
these rules once a module actually adds persistence.

- One `DbContext` per module (bounded context), not a single application-wide context. A
  module's `DbContext` is internal to that module (see [architecture.md](architecture.md)) — no
  other module references it or its entities directly.
- Configure entities explicitly with `IEntityTypeConfiguration<T>` classes per entity, not a
  single growing `OnModelCreating` method and not scattered data-annotation attributes.
- Disable lazy-loading proxies. Load related data explicitly with `Include` for write scenarios,
  and with a `Select` projection straight to a DTO for read/query scenarios, to avoid N+1 queries
  and over-fetching.
- Use PostgreSQL-appropriate column types (`timestamptz` for instants, `text` for unbounded
  strings) and a consistent naming convention (snake_case for tables/columns is the PostgreSQL
  norm; configure it once via a naming-convention mechanism rather than annotating every
  property).
- Schema changes go through EF Core migrations (`dotnet ef migrations add <Name>`,
  `dotnet ef database update`), committed to the module's project. Never hand-edit the schema of
  a running database.
- Connection strings come from configuration (`appsettings.*.json`, environment variables, or
  user-secrets in development) — never hardcoded in source. The credentials in
  `infrastructure/docker/docker-compose.yml` are local-development-only and must never be reused
  for a non-local environment.
- In tests, run against a real PostgreSQL instance via Testcontainers rather than the EF Core
  in-memory provider, which does not enforce PostgreSQL constraints or SQL translation — see
  [testing.md](testing.md).
