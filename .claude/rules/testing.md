# Testing Rules

No test project exists yet (see [AGENTS.md](../../AGENTS.md)). These rules apply from the first
test project onward.

- **.NET**: use xUnit. Name test projects `<ProjectUnderTest>.Tests` and mirror the
  `src` structure so a test's location maps clearly to what it covers.
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
