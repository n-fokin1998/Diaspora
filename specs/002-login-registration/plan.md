# Implementation Plan: Login and Registration

**Branch**: `002-login-registration` | **Date**: 2026-09-15 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `/specs/002-login-registration/spec.md`

**Note**: This template is filled in by the `/speckit-plan` command; its definition describes the execution workflow.

## Summary

Let a new visitor register (email, password, first/last name, date of birth, location) and let a
registered user log in, using an entirely project-owned JWT-based auth scheme — no third-party
auth/identity/validation library, password hashing done with the BCL's PBKDF2 implementation plus
a per-user random salt, and the JWT held only in memory on the client. Successful registration or
login lands the user on a new, currently-empty Home screen in the SPA. See
[research.md](research.md) for the technology decisions this rests on, and
[data-model.md](data-model.md)/[contracts/auth-api.md](contracts/auth-api.md) for the concrete
shape.

## Technical Context

**Language/Version**: C# 13 / .NET 9 (backend: `Client.Api`, `Identity.*`); TypeScript 5 / React
19 on Vite (frontend: `Client.Api/spa`)

**Primary Dependencies**: MediatR, EF Core + `Npgsql.EntityFrameworkCore.PostgreSQL` (already
referenced by the `Identity` module); `Microsoft.AspNetCore.Authentication.JwtBearer` +
`System.IdentityModel.Tokens.Jwt` (new — JWT issuance/validation, research.md #1/#3); `axios`
(already referenced); `react-router-dom` (new — routing to the Home screen, research.md #6). No
new package is added for password hashing or input validation (both hand-rolled, research.md
#1/#2) or for JWT storage on the client (in-memory, research.md #5).

**Storage**: PostgreSQL, via the `Identity` module's own `IdentityDbContext` (one new `users`
table — see [data-model.md](data-model.md)). No new session/token storage — JWTs are stateless
(research.md #4).

**Testing**: xUnit in `Diaspora.Tests` (domain/application unit tests for `User` and the
register/login handlers; Testcontainers-backed integration tests for the EF Core mapping) per
[testing.md](../../.claude/rules/testing.md); Vitest + React Testing Library for the new
register/login/home frontend code.

**Target Platform**: Web — ASP.NET Core Web API (Kestrel) backend, React SPA frontend, run
locally via `dotnet run`/`npm run dev` against a Dockerized PostgreSQL instance
([AGENTS.md](../../AGENTS.md) Development Commands).

**Project Type**: Web application (existing backend + frontend split in this repository).

**Performance Goals**: No dedicated performance target beyond ordinary interactive web-app
responsiveness (spec.md SC-001/SC-002: registration under 2 minutes end-to-end, login under 15
seconds for a user with correct credentials) — this feature does not introduce a
high-throughput/concurrency requirement.

**Constraints**: The JWT signing secret and any environment-specific config used outside local
development MUST come from configuration/user-secrets, never source (matches the existing
connection-string convention in [efcore-postgresql.md](../../.claude/rules/efcore-postgresql.md));
a non-production placeholder JWT secret is the one accepted exception, committed in
`appsettings.Development.json` for a zero-setup local `dotnet run` (research.md #3). Passwords are
never stored, logged, or transmitted in plain form beyond the initial HTTPS request body (spec.md
SC-004).

**Scale/Scope**: Two new API endpoints, one new database table, three new/changed SPA screens
(Register, Login, Home) plus routing — a single small vertical slice within the existing
`Identity` module.

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- **I. Learning-Oriented Engineering**: PASS. Implementing PBKDF2 salting/hashing and JWT
  issuance/validation by hand (rather than pulling in an identity framework) is the explicit,
  concrete learning surface of this feature.
- **II. Incremental Delivery**: PASS. Register → auto-login → Home and Login → Home are each a
  complete, observable, end-to-end slice; the Home screen being empty is an explicit, stated
  scope boundary, not a partially-wired feature.
- **III. Simplicity First**: PASS. No refresh tokens, no server-side session/revocation store, no
  roles/permissions, one `DbContext`, one entity. The two new packages
  (`Microsoft.AspNetCore.Authentication.JwtBearer`/`System.IdentityModel.Tokens.Jwt`,
  `react-router-dom`) are each tied to a concrete, present need (JWT validation middleware;
  multi-screen navigation) — see research.md #1 and #6 for why hand-rolling either was rejected.
- **IV. Modular Architecture**: PASS. `User`, its hashing/JWT abstractions, and the
  `IdentityDbContext` stay inside `Identity.Domain`/`Identity.Application`/`Identity.Infrastructure`;
  the two controllers and JWT-bearer middleware registration live in `Client.Api`, matching
  "transport stays outside the module."
- **V. Quality by Default**: PASS (planned, verified at implementation time). Unit tests for
  validation/hashing/JWT logic with no database; Testcontainers-backed integration tests for the
  EF Core mapping; Vitest/RTL tests for the new forms and routing — see
  [quickstart.md](quickstart.md) §6 and [testing.md](../../.claude/rules/testing.md).
- **VI. Specification-Driven Development**: PASS. This plan and its research/data-model/contracts
  are derived from and traceable back to spec.md's FRs and the applicable `.claude/rules/` files.
- **VII. Agent-Assisted Development**: PASS. No new module boundary or architecturally
  significant dependency is introduced without the rationale being recorded here for review
  (research.md); nothing here requires a human approval gate beyond the normal PR review.
- **VIII. Local-First and Reproducible**: PASS. Runs entirely against the existing
  Docker-Composed PostgreSQL and local `dotnet run`/`npm run dev`; no cloud dependency introduced.
- **IX. Document Important Decisions**: PASS. The auth-scheme, password-hashing, token-storage,
  and logout-semantics decisions — the significant technology choices this feature makes — are
  recorded with rationale and rejected alternatives in [research.md](research.md), which serves as
  this feature's decision record; no separate ADR log exists yet in this repository to add to.

No violations requiring an entry in Complexity Tracking.

## Project Structure

### Documentation (this feature)

```text
specs/002-login-registration/
├── plan.md              # This file (/speckit-plan command output)
├── research.md          # Phase 0 output (/speckit-plan command)
├── data-model.md         # Phase 1 output (/speckit-plan command)
├── quickstart.md         # Phase 1 output (/speckit-plan command)
├── contracts/
│   └── auth-api.md       # Phase 1 output (/speckit-plan command)
├── checklists/
│   └── requirements.md   # /speckit-specify quality checklist
└── tasks.md              # Phase 2 output (/speckit-tasks command - NOT created by /speckit-plan)
```

### Source Code (repository root)

This feature extends the existing modular-monolith layout (see
[architecture.md](../../.claude/rules/architecture.md)) — no new module, no new top-level
folder.

```text
src/
├── Client.Api/
│   ├── Controllers/
│   │   └── Auth/
│   │       ├── RegisterController.cs      # POST /api/auth/register (existing empty stub → implemented)
│   │       └── LoginController.cs         # POST /api/auth/login (new)
│   ├── Program.cs                          # + JwtBearer authentication/authorization wiring
│   └── spa/
│       └── src/
│           ├── app/                        # + route table (Register/Login/Home), auth-aware routing
│           ├── pages/
│           │   ├── register/               # new: registration page
│           │   ├── login/                  # new: login page
│           │   └── home/                   # new: empty Home page
│           ├── features/
│           │   ├── register-user/          # new: form + submit hook + validation messages
│           │   └── login-user/             # new: form + submit hook + validation messages
│           ├── entities/
│           │   └── session/                # new: in-memory auth/session state (current user + token)
│           └── shared/
│               └── api/                    # existing axios client, extended with an auth-aware instance
│
└── Modules/
    └── Identity/
        ├── Identity.Domain/
        │   └── Users/
        │       └── User.cs                 # existing empty stub → the entity in data-model.md
        ├── Identity.Application/
        │   ├── Authentication/
        │   │   ├── Register/               # RegisterCommand/Handler/Result + RegisterCommandValidator
        │   │   └── Login/                  # LoginCommand/Handler/Result + LoginCommandValidator
        │   └── Common/
        │       ├── Abstractions/
        │       │   ├── IUserRepository.cs    # EmailExistsAsync/FindByNormalizedEmailAsync/AddUser
        │       │   ├── IUnitOfWork.cs         # SaveChangesAsync
        │       │   ├── IPasswordHasher.cs     # Hash/Verify contract
        │       │   ├── IJwtTokenService.cs    # issue-token contract
        │       │   └── IValidator<T>.cs       # command validator contract
        │       └── Behaviors/
        │           └── ValidationBehavior.cs  # MediatR pipeline behavior running IValidator<T>
        └── Identity.Infrastructure/
            ├── Authentication/
            │   ├── PasswordHasher.cs         # PBKDF2 implementation (research.md #2)
            │   └── JwtTokenService.cs        # JWT issuance (research.md #3)
            └── Persistence/
                ├── IdentityDbContext.cs      # Users DbSet, consumed only inside Infrastructure
                ├── Repositories/
                │   └── UserRepository.cs      # implements IUserRepository
                ├── UnitOfWork.cs               # implements IUnitOfWork
                ├── Configurations/           # UserConfiguration (IEntityTypeConfiguration<User>)
                └── Migrations/               # initial Users table migration

tests/ (src/Diaspora.Tests, mirroring the structure above per testing.md)
├── Modules/Identity/Domain/Users/
├── Modules/Identity/Application/Authentication/Register/
├── Modules/Identity/Application/Authentication/Login/
└── Modules/Identity/Infrastructure/Persistence/
```

**Structure Decision**: Existing "Option 2: Web application" shape, already present in this
repository as `Client.Api` (backend host + SPA) and `Modules/Identity` (the one module this
feature touches). Every path above either already exists as an empty stub (per AGENTS.md's
instruction to verify before assuming) or is a new file/folder inside an already-established
project — no new project, module, or top-level folder is introduced.

## Complexity Tracking

*No entries — Constitution Check reported no violations requiring justification.*
