---
paths:
  - "**/*.cs"
---

# C# / .NET Rules

Conventions for C# and .NET 9 code, on top of the repository-wide setup (nullable reference
types and implicit usings are already enabled in project files — keep them enabled in new
projects).

- Use **file-scoped namespaces** (`namespace Foo.Bar;`), not block-scoped.
- One public type per file; the file name matches the type name.
- Prefer `record`/`record class` for immutable data carriers (DTOs, events, value objects), and
  primary constructors for simple classes/services, to reduce boilerplate.
- Treat nullable reference type warnings as real: handle `null` explicitly instead of silencing
  with `!`. Only use `!` when nullability has already been proven by preceding logic.
- Default to immutability: `readonly` fields, `init`-only or `record` properties. Expose mutable
  state only where the type's purpose requires it.
- Async all the way: any method performing I/O is `async Task`/`async Task<T>`. Never block on
  async code with `.Result`, `.Wait()`, or `GetAwaiter().GetResult()`. Accept and forward a
  `CancellationToken` on public async methods that do I/O.
- Use constructor injection for dependencies; do not use a service locator pattern or static
  mutable singletons for stateful services.
- Validate method/constructor arguments at the boundary of a class (fail fast with a clear
  exception message) rather than letting invalid state propagate.
- Use exceptions for exceptional conditions, not for expected control flow (e.g. "not found" in a
  lookup is a return value/`Result`, not necessarily an exception).
- Code must be clean under `dotnet format` (see [AGENTS.md](../../AGENTS.md) for the command)
  before a change is considered done.
