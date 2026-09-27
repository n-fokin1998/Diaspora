# Testing Rules

- **.NET**: use xUnit, split across two kinds of test project:
  - **`<Module>.Tests`** (e.g. `Identity.Tests`), one per module once it has enough to test,
    referencing that module's `Domain`/`Application`/`Infrastructure` projects directly. Mirror
    the module's own layer folders inside it (`Domain/Users/UserTests.cs`,
    `Application/Authentication/Register/RegisterCommandHandlerTests.cs`,
    `Infrastructure/Persistence/Repositories/UserRepositoryTests.cs`, ...) so a test's location
    maps to what it covers. This is where a module's unit tests (`Domain`, command/query
    handlers and validators with mocked `Application` abstractions via Moq) and its
    `Infrastructure` integration tests (repositories, services) against a real Testcontainers
    PostgreSQL instance live.
  - **`Diaspora.Tests`**, one solution-wide project referencing `Client.Api`, for tests that only
    make sense above a single module — today, controller/API-level tests driven through
    `WebApplicationFactory<Program>` plus a real Testcontainers PostgreSQL instance (see
    `AuthApiFactory`). Do not duplicate a module's own unit/integration coverage here; only add a
    test here when it exercises the HTTP layer or crosses module boundaries.
  - Start a module without its own `<Module>.Tests` project (put a handful of early tests in
    `Diaspora.Tests` instead) until the module actually has enough test surface to justify the
    extra project, per Simplicity First.
- **Frontend**: use Vitest with React Testing Library (native fit for the existing Vite setup).
  Colocate a unit's test file next to what it covers (`LoginForm.tsx` → `loginForm.test.tsx`,
  `model.tsx` → `session.test.tsx`) rather than a separate top-level `__tests__` tree.
- Favor a small pyramid: many fast unit tests for `Domain`/`Application` logic with no database
  or network involved; fewer integration tests that exercise a module's `Infrastructure` (EF
  Core, Kafka) against real dependencies; a handful of end-to-end tests, if any, through the API.
- For anything touching PostgreSQL, use Testcontainers to run a real database instance rather
  than mocking `DbContext` or using the EF Core in-memory provider — this matches the
  constitution's Quality by Default principle and catches issues the in-memory provider hides
  (constraints, SQL translation, transactions).
- Mock true external boundaries that cannot reasonably run locally (e.g. a third-party HTTP API),
  and mock a module's own `Application`-layer abstractions (an `I<Entity>Repository`,
  `IUnitOfWork`, `IPasswordHasher`, a token service) when unit-testing a command/query handler or
  validator in isolation via Moq — this is the seam the handler is actually meant to be tested
  against, per the CQRS/Mediator pattern in [architecture.md](architecture.md). The same
  abstraction's concrete `Infrastructure` implementation still gets its own integration test
  against a real dependency (see `UserRepositoryTests`). Do not mock a class's own internal
  collaborators just to fake isolation (e.g. a `Domain` entity has no collaborators to mock).
- Name tests `MethodUnderTest_Scenario_ExpectedResult` (e.g.
  `CreateEvent_WithPastStartDate_ReturnsValidationError`) so a failing test name alone explains
  the regression.
- A change is not done until its new/updated automated tests pass and the full existing suite
  still passes (per the constitution's Quality by Default principle).

## For an educational project: keep test coverage minimal but representative

This is a learning project, and every test an agent writes and runs costs real tokens — that is
a genuine project constraint, not just a suggestion. Do not try to enumerate every edge case:
- Cover each structural category the pyramid above calls for (a `Domain` unit test, a handler
  test, a validator test, an `Infrastructure` integration test, an API-level test) with a small
  number of cases — typically one happy-path case and one or two of the most important
  failure/edge cases per unit, not an exhaustive matrix of every input combination.
- Prefer breadth across the categories over depth within one of them: it is more valuable to have
  a thin test at each layer of a new use case than an exhaustively-cased test at only one layer.
- This does not relax the requirement that new/changed behavior has some automated test, or that
  the existing suite keeps passing — it only bounds how many cases that test coverage needs.
