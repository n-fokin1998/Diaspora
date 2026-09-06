# Architecture Rules

Concrete conventions for structuring the system as a modular monolith. See the
[constitution](../../.specify/memory/constitution.md) for the *why* (Principle IV, Modular
Architecture); this file defines the *how*.

## Modules

- A **module** is organized around a business capability (e.g. `Events`, `Users`, `Venues`), not
  around a technical layer. Do not create solution-wide `Controllers/`, `Services/`,
  `Repositories/` folders that span every feature.
- Within a module, keep a pragmatic internal separation:
  - **Api** — controllers/endpoints and request/response DTOs.
  - **Application** — use cases/services orchestrating business logic.
  - **Domain** — entities, value objects, business rules. No framework dependencies here.
  - **Infrastructure** — EF Core `DbContext`/configurations, Kafka producers/consumers, external
    HTTP clients, OpenTelemetry instrumentation specific to the module.
- This split is guidance, not ceremony: for a small module, a handful of files across these
  concerns is fine. Do not create empty layers "for consistency."

## Boundaries

- Modules communicate only through explicit contracts: public interfaces, DTOs, or events —
  never by referencing another module's EF Core entities, `DbContext`, or internal service
  classes directly.
- A module's `Domain` and `Application` code must not reference infrastructure libraries
  directly (EF Core, Npgsql, Kafka client, `HttpClient`). Depend on an interface defined in
  `Application`/`Domain` and implement it in `Infrastructure` (dependency inversion).
- Avoid a generic `Common`/`Shared`/`Utils` project as a dumping ground. Shared code is
  acceptable only for genuinely cross-cutting, stable concerns (e.g. a `Result<T>` type); if a
  "shared" file starts accumulating unrelated helpers, split it back into the modules that need
  it.
- In-process communication between modules goes through the consuming module's own interface
  (constructor-injected), not a shared mutable state or a shared database table owned by another
  module.

## Evolving toward microservices

- Keep each module's persistence and contracts self-contained so it could be extracted into its
  own deployable later without rewriting its internals — this is enforced by the boundary rules
  above, not by a separate abstraction layer.
- Do not introduce distributed-systems machinery (separate deployables, a message broker between
  modules, per-module databases) until a concrete, articulated need exists. Record that decision
  as an ADR per the constitution when it happens.
