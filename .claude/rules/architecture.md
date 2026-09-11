# Architecture Rules

Concrete conventions for structuring the system as a modular monolith **built to microservices
conventions**: every module is coded as if it were its own service — self-contained domain,
application, and persistence, wired in only through explicit contracts — even though it runs
in-process inside one deployable today. See the
[constitution](../../.specify/memory/constitution.md) for the *why* (Principle IV, Modular
Architecture); this file defines the *how*. `src/Web/Modules/Identity` is the concrete reference
example for everything below — when in doubt, match its layout.

## Modules

- A **module** is organized around a business capability (e.g. `Identity`, `Events`, `Venues`),
  not around a technical layer. Do not create solution-wide `Controllers/`, `Services/`,
  `Repositories/` folders that span every feature.
- A module is a set of **separate projects**, one per layer, so that dependencies between layers
  are enforced by the compiler, not just by convention — mirroring how an extracted service would
  be structured internally:
  - **`<Module>.Domain`** — entities, value objects, business rules. No package references at
    all beyond the BCL; this project must not depend on ASP.NET Core, EF Core, or any other
    framework.
  - **`<Module>.Application`** — use cases orchestrating business logic (see CQRS/Mediator
    below), plus the abstractions (interfaces) that `Infrastructure` implements. References only
    `Domain` and abstraction packages (e.g. `Microsoft.Extensions.DependencyInjection.Abstractions`),
    never a concrete infrastructure package.
  - **`<Module>.Infrastructure`** — the module's own EF Core `DbContext`/configurations, and the
    concrete implementations of `Application`'s abstractions (e.g. a password hasher, a token
    service). References `Application` and `Domain`.
  - **Transport (API)** — controllers, request/response DTOs, and routing for the module's
    endpoints live *outside* the module, in `Client.Api` (see "Transport stays outside the
    module" below) — a module has no `Api`/`Controllers` project of its own while the system is a
    monolith.
- Each layer project (except `Domain`) exposes a `DependencyInjection` static class with an
  `Add<Module><Layer>(this IServiceCollection, IConfiguration)` extension method as its
  composition-root entry point (e.g. `Identity.Infrastructure.DependencyInjection.AddIdentityInfrastructure`).
  `Client.Api` calls these at startup; this is the explicit, minimal contract for wiring a module
  in, not direct construction of the module's internal types.
- Within `Application`, organize use cases as **vertical slices**: one folder per use case
  containing its command/query, handler, and result together (e.g.
  `Identity.Application/Authentication/Register/{RegisterCommand,RegisterCommandHandler,RegisterResult}.cs`),
  not folders split by technical role (`Commands/`, `Handlers/`, `Results/`) across all use cases.
- This split is guidance, not ceremony: for a small module, a handful of files across these
  projects is fine. Do not create empty layers or empty folders "for consistency."

## Transport stays outside the module

- Controllers live in `Client.Api/Controllers/<Module>/`, referencing the module's `Application`
  project (and its `Infrastructure` project only for the module's `DependencyInjection` call at
  startup). A controller MUST NOT be added inside a module's own projects.
- A controller depends on MediatR's sender abstraction and the module's `Application`-layer
  commands/queries and result DTOs — never on the module's `Domain` entities or `Infrastructure`
  types directly (see [aspnetcore-webapi.md](aspnetcore-webapi.md)).
- Keeping transport outside the module means extracting a module into its own service later is a
  matter of giving it its own host/transport project and pointing `Client.Api` at it over the
  network instead — the module's `Domain`/`Application`/`Infrastructure` code does not change.

## CQRS and Mediator

- Use a **lightweight, logic-level CQRS** style inside `Application`: explicit commands
  (state-changing use cases) and queries (read use cases), each with a single handler, dispatched
  through the **Mediator pattern** via the `MediatR` package already referenced in the solution.
  This is a code-organization split, not a data-level one — a module still has exactly one
  database/`DbContext` (see [efcore-postgresql.md](efcore-postgresql.md)); do not introduce
  separate read/write stores or event sourcing to go with it.
- Reach for a command/query + handler only where it earns its keep: decoupling a controller from
  a module's use case, or enabling a shared MediatR pipeline behavior (e.g. validation, logging)
  across many use cases. Do not wrap every trivial CRUD operation in a Command/Handler pair when
  a direct method call on an `Application` service is simpler and clearer — extra indirection
  without a concrete benefit violates the constitution's Simplicity First principle.
- Commands and queries are simple data carriers (no business logic); the handler holds the
  orchestration logic and depends on `Application`-defined abstractions, never on
  `Infrastructure` concrete types.

## Boundaries

- Modules communicate only through explicit contracts: public interfaces, DTOs, or events —
  never by referencing another module's EF Core entities, `DbContext`, or internal service
  classes directly.
- A module's `Domain` and `Application` code must not reference infrastructure libraries
  directly (EF Core, Npgsql, Kafka client, `HttpClient`). Depend on an interface defined in
  `Application`/`Domain` and implement it in `Infrastructure` (dependency inversion).
- `src/Web/Contracts/Diaspora.Contracts` holds DTOs/events genuinely shared *across* module (and,
  later, service) boundaries — the same kind of versioned contract [kafka.md](kafka.md) already
  describes for integration events. Keep it small: most cross-module communication should go
  through a module's own `Application`-layer interface, not through a shared contracts project.
- `src/Web/BuildingBlocks/Diaspora.Core` is the one acceptable shared/common project: a small,
  stable, cross-cutting kernel (e.g. a `Result<T>` type, shared MediatR pipeline behaviors) that
  many modules' `Domain`/`Application` layers may depend on. It is not a dumping ground — if it
  starts accumulating unrelated helpers, split them back into the modules that need them.
- In-process communication between modules goes through the consuming module's own interface
  (constructor-injected), not a shared mutable state or a shared database table owned by another
  module.

## Evolving toward microservices

- Keep each module's persistence and contracts self-contained so it could be extracted into its
  own deployable later without rewriting its internals — this is enforced by the module layout
  and boundary rules above, not by a separate abstraction layer.
- Do not introduce distributed-systems machinery (separate deployables, a message broker between
  modules, per-module databases) until a concrete, articulated need exists. Record that decision
  as an ADR per the constitution when it happens.
