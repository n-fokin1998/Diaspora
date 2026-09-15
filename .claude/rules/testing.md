# Testing Rules

- **.NET**: use xUnit. Today, all backend tests live in the single `src/Diaspora.Tests` project
  (mirror the `src` structure with folders inside it, e.g. `Modules/Identity/...`, so a test's
  location still maps clearly to what it covers) — this is a Simplicity First starting point
  while there isn't much to test yet. Split a module's tests out into its own
  `<ProjectUnderTest>.Tests` project only once that module's test surface grows enough to justify
  the extra project (e.g. it needs its own heavy test-only dependencies, or the single project
  becomes unwieldy).
- **Frontend**: use Vitest with React Testing Library (native fit for the existing Vite setup).
- Favor a small pyramid: many fast unit tests for `Domain`/`Application` logic with no database
  or network involved; fewer integration tests that exercise a module's `Infrastructure` (EF
  Core, Kafka) against real dependencies; a handful of end-to-end tests, if any, through the API.
- For anything touching PostgreSQL, use Testcontainers to run a real database instance rather
  than mocking `DbContext` or using the EF Core in-memory provider — this matches the
  constitution's Quality by Default principle and catches issues the in-memory provider hides
  (constraints, SQL translation, transactions).
- Mock only true external boundaries that cannot reasonably run locally (e.g. a third-party
  HTTP API). Do not mock your own module's collaborators just to isolate a unit — prefer testing
  through the real object graph within a module.
- Name tests `MethodUnderTest_Scenario_ExpectedResult` (e.g.
  `CreateEvent_WithPastStartDate_ReturnsValidationError`) so a failing test name alone explains
  the regression.
- A change is not done until its new/updated automated tests pass and the full existing suite
  still passes (per the constitution's Quality by Default principle).
