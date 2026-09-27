---
paths:
  - "**/*.cs"
---

# EF Core / PostgreSQL Rules

EF Core and PostgreSQL are in active use (see [AGENTS.md](../../AGENTS.md)) — `Identity` is the
reference example (`Identity.Infrastructure/Persistence/`); apply the same conventions to every
module that adds persistence.

- One `DbContext` per module (bounded context), not a single application-wide context. A
  module's `DbContext` (e.g. `IdentityDbContext`) is declared `internal` — no other module or the
  `Client.Api` host references it or its entities directly (see [architecture.md](architecture.md)).
  `Infrastructure` grants the module's own `<Module>.Tests` project and `Diaspora.Tests`
  `InternalsVisibleTo` so tests can exercise it directly.
- **One PostgreSQL schema per module**, named after the module in `snake_case` (e.g. `identity`).
  Every `IEntityTypeConfiguration<T>.Configure` call sets it explicitly:
  `builder.ToTable("<table>", "<module_schema>")`. Create the schema in the module's own initial
  migration via `migrationBuilder.EnsureSchema("<module_schema>")`. This keeps a module's tables
  visibly and physically separated in the shared database, one step short of a database per
  module if that is ever needed.
- Configure entities explicitly with `IEntityTypeConfiguration<T>` classes per entity, not a
  single growing `OnModelCreating` method and not scattered data-annotation attributes.
  `<Module>DbContext.OnModelCreating` applies every configuration in the assembly via
  `modelBuilder.ApplyConfigurationsFromAssembly(...)` rather than registering each one by hand.
- Map every property's column name explicitly with `.HasColumnName("snake_case_name")` inside its
  `IEntityTypeConfiguration<T>` (see `UserConfiguration`) rather than adding a global
  naming-convention package — one dependency fewer, per Simplicity First, while the test project
  covers only a handful of entities.
- Disable lazy-loading proxies. Load related data explicitly with `Include` for write scenarios,
  and with a `Select` projection straight to a DTO for read/query scenarios, to avoid N+1 queries
  and over-fetching.
- Use PostgreSQL-appropriate column types (`timestamptz` for instants via `.HasColumnType(...)`,
  `text`/default `varchar` via `.HasMaxLength(...)` for bounded strings).
- **Repository + Unit of Work**: `Application`'s `Common/Abstractions` folder declares one
  `I<Entity>Repository` interface per aggregate (query methods returning domain entities, plus
  synchronous `Add<Entity>`/mutation methods — no `SaveChanges` on the repository itself) and a
  single module-wide `IUnitOfWork` with just `SaveChangesAsync`. `Infrastructure` implements each
  repository against the module's `DbContext` (e.g. `UserRepository`, `RefreshTokenRepository`)
  and implements `IUnitOfWork` as a thin wrapper calling `DbContext.SaveChangesAsync`. A command
  handler injects the repositories it needs plus `IUnitOfWork`, makes all its changes, and calls
  `SaveChangesAsync` exactly once at the end of the use case — this is the module's one
  `DbContext`/one transaction per request, not a repository-per-call-commits pattern.
- Schema changes go through EF Core migrations (`dotnet ef migrations add <Name>`,
  `dotnet ef database update`), committed to the module's `Infrastructure` project under
  `Persistence/Migrations/`. Never hand-edit the schema of a running database.
- Connection strings come from configuration (`appsettings.*.json`, environment variables, or
  user-secrets in development) — never hardcoded in source. The credentials in
  `infrastructure/docker/docker-compose.yml` are local-development-only and must never be reused
  for a non-local environment.
- In tests, run against a real PostgreSQL instance via Testcontainers (`Testcontainers.PostgreSql`,
  `postgres:18`) rather than the EF Core in-memory provider, which does not enforce PostgreSQL
  constraints or SQL translation — see [testing.md](testing.md).
