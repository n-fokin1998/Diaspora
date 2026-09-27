# Implementation Plan: Login and Registration

**Branch**: `002-login-registration` | **Date**: 2026-09-15 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `/specs/002-login-registration/spec.md`

**Note**: This template is filled in by the `/speckit-plan` command; its definition describes the execution workflow.

## Summary

Let a new visitor register (email, password, first/last name, date of birth, location) and let a
registered user log in, using an entirely project-owned JWT-based auth scheme — no third-party
auth/identity/validation library, password hashing done with the BCL's PBKDF2 implementation plus
a per-user random salt. The session survives a page refresh or browser restart via a hybrid
client-side token strategy: a short-lived JWT access token held only in memory, refreshed
silently using a longer-lived, rotating refresh token the server delivers as an `HttpOnly`,
`Secure`, `SameSite=Strict` cookie — never readable by page JavaScript. Successful registration
or login lands the user on the Home screen in the SPA; the session then persists there across
refreshes until the refresh token itself expires (14 days of inactivity) or the user explicitly
logs out (now a server-side revocation, not just a client-side token discard). See
[research.md](research.md) for the technology decisions this rests on (notably #8-#11, added
2026-09-22 to introduce the refresh token and supersede the original stateless/in-memory-only
design in #4/#5), and [data-model.md](data-model.md)/[contracts/auth-api.md](contracts/auth-api.md)
for the concrete shape.

## Technical Context

**Language/Version**: C# 13 / .NET 9 (backend: `Client.Api`, `Identity.*`); TypeScript 5 / React
19 on Vite (frontend: `Client.Api/spa`)

**Primary Dependencies**: MediatR, EF Core + `Npgsql.EntityFrameworkCore.PostgreSQL` (already
referenced by the `Identity` module); `Microsoft.AspNetCore.Authentication.JwtBearer` +
`System.IdentityModel.Tokens.Jwt` (JWT issuance/validation, research.md #1/#3); `axios`
(already referenced); `react-router-dom` (routing to the Home screen, research.md #6). No new
package is added for password hashing, input validation, or the refresh token (all hand-rolled
against BCL primitives — `RandomNumberGenerator`, `SHA256` — research.md #1/#2/#9) or for JWT/
refresh-token storage on the client (in-memory access token + `HttpOnly` cookie, research.md
#8).

**Storage**: PostgreSQL, via the `Identity` module's own `IdentityDbContext` — one `users` table
(see [data-model.md](data-model.md)) plus, new in this revision, one `refresh_tokens` table
(research.md #9, data-model.md's `RefreshToken` entity) storing only a hash of each refresh
token, never the raw value. Access-token JWTs themselves remain stateless (unchanged from
research.md #3); what is now persisted is the refresh token that lets the client silently obtain
a new one, which is what research.md #4's original "no server-side session store" decision
explicitly ruled out and research.md #9/#10 supersede.

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
SC-004). The raw refresh token is never stored server-side (only its SHA-256 hash) and never
appears in a JSON response body (only in the `HttpOnly` cookie, research.md #9/#10). The `"Spa"`
CORS policy MUST add `AllowCredentials()` (still restricted to the specific SPA origin, never
`AllowAnyOrigin`) for the refresh-token cookie to flow on the SPA's cross-origin calls, and the
SPA's `axios` client MUST set `withCredentials: true` on those calls (research.md #10).

**Scale/Scope**: Four API endpoints total (two existing, two new: refresh, logout), two database
tables (`users`, new `refresh_tokens`), three new/changed SPA screens (Register, Login, Home)
plus routing — still a single small vertical slice within the existing `Identity` module.

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

*Re-evaluated 2026-09-22 for the refresh-token addition; changes from the original pass are
called out explicitly below.*

- **I. Learning-Oriented Engineering**: PASS. Implementing PBKDF2 salting/hashing and JWT
  issuance/validation by hand (rather than pulling in an identity framework) is the explicit,
  concrete learning surface of this feature. **Addition**: refresh-token rotation and
  reuse-detection (research.md #9) is a second concrete instance of the same principle — a
  hand-rolled application of a standard OAuth2/OWASP pattern, not a library black-box.
- **II. Incremental Delivery**: PASS. Register → auto-login → Home and Login → Home are each a
  complete, observable, end-to-end slice; the Home screen being empty is an explicit, stated
  scope boundary, not a partially-wired feature. The refresh-token addition extends the same
  slices (session persistence is observable end-to-end: refresh the page, stay logged in) rather
  than opening a new, separately-shippable feature.
- **III. Simplicity First**: PASS, with one deliberate addition of scope over the original pass.
  The original design's "no refresh tokens, no server-side session store" was itself a Simplicity
  First call — but it was made when persisting a session across a page refresh was explicitly out
  of scope (spec.md's original Assumptions). That is no longer true (spec.md's 2026-09-22 scope
  addition), and a refresh token stored server-side is the minimum mechanism that can satisfy it
  at all — there is no simpler design that still meets the requirement. What stays deliberately
  simple within that: one `refresh_tokens` table (no separate sessions/devices model), rotation
  detection via one nullable `RevokedAtUtc`/`ReplacedByTokenHash` pair rather than a general
  audit log, and cookie `Path` scoping instead of a broader CSRF-token subsystem (research.md
  #10). The two new packages from the original pass
  (`Microsoft.AspNetCore.Authentication.JwtBearer`/`System.IdentityModel.Tokens.Jwt`,
  `react-router-dom`) are unchanged and still each tied to a concrete, present need; no new
  package is added for the refresh token itself (research.md #9 uses only BCL primitives).
- **IV. Modular Architecture**: PASS. `RefreshToken`, its abstractions
  (`IRefreshTokenService`/`IRefreshTokenRepository`), and the new `refresh_tokens` table stay
  inside `Identity.Domain`/`Identity.Application`/`Identity.Infrastructure`, exactly like `User`;
  the two new controllers (`RefreshController`, `LogoutController`) live in `Client.Api`,
  matching "transport stays outside the module."
- **V. Quality by Default**: PASS (planned, verified at implementation time). Unit tests for
  validation/hashing/JWT logic with no database, now including refresh-token issuance/rotation/
  reuse-detection logic; Testcontainers-backed integration tests for the EF Core mapping,
  extended to the new `refresh_tokens` table; Vitest/RTL tests for the new forms, routing, and
  the mount-time silent-refresh/rehydration flow (research.md #11) — see
  [quickstart.md](quickstart.md) §6 and [testing.md](../../.claude/rules/testing.md).
- **VI. Specification-Driven Development**: PASS. spec.md was amended in the same pass as this
  plan (2026-09-22: FR-012 updated, FR-015/FR-016 and SC-006 added, session persistence made an
  explicit Assumption) specifically so this plan continues to derive from, and stay traceable to,
  the specification rather than getting ahead of it.
- **VII. Agent-Assisted Development**: PASS. The refresh-token mechanism, its data-model addition,
  and the CORS/cookie configuration change are all recorded with rationale and rejected
  alternatives in [research.md](research.md) (#8-#11) for human review before implementation;
  nothing here requires a human approval gate beyond the normal PR review.
- **VIII. Local-First and Reproducible**: PASS. Still runs entirely against the existing
  Docker-Composed PostgreSQL and local `dotnet run`/`npm run dev`; the `Secure` cookie attribute
  needs no local-only exception since browsers treat `localhost` as a secure context regardless
  of scheme (research.md #10) — no new local-setup step is introduced.
- **IX. Document Important Decisions**: PASS. research.md #4 and #5 (the original stateless-JWT/
  in-memory-only decisions) are marked superseded in place, each pointing at the new decision
  (#9/#10 and #8, respectively) that replaces them, per this principle's explicit requirement
  that a superseding decision reference the one it replaces.

No violations requiring an entry in Complexity Tracking — the added complexity (a new table, two
new endpoints, cookie/CORS changes) is the direct, minimum consequence of a requirement the
project owner explicitly added, not speculative.

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
│   │       ├── RegisterController.cs      # POST /api/auth/register (existing — now also sets the refresh-token cookie)
│   │       ├── LoginController.cs         # POST /api/auth/login (existing — now also sets the refresh-token cookie)
│   │       ├── RefreshController.cs       # POST /api/auth/refresh (new — research.md #10)
│   │       └── LogoutController.cs        # POST /api/auth/logout (new — research.md #10)
│   ├── Program.cs                          # JwtBearer wiring (existing) + CORS "Spa" policy gains AllowCredentials() (new)
│   └── spa/
│       └── src/
│           ├── app/                        # route table (Register/Login/Home), auth-aware routing (existing)
│           ├── pages/
│           │   ├── register/               # registration page (existing)
│           │   ├── login/                  # login page (existing)
│           │   └── home/                   # empty Home page (existing)
│           ├── features/
│           │   ├── register-user/          # form + submit hook + validation messages (existing)
│           │   └── login-user/             # form + submit hook + validation messages (existing)
│           ├── entities/
│           │   └── session/                # existing in-memory session state + new: 'resolving' status,
│           │                                 mount-time silent refresh, 401-triggered refresh retry (research.md #11)
│           └── shared/
│               └── api/                    # existing axios client + new: withCredentials: true,
│                                             refresh/logout calls, 401 response interceptor (research.md #10/#11)
│
└── Modules/
    └── Identity/
        ├── Identity.Domain/
        │   ├── Users/
        │   │   └── User.cs                 # existing entity, unchanged by this revision
        │   └── RefreshTokens/
        │       └── RefreshToken.cs         # new — the entity in data-model.md (research.md #9)
        ├── Identity.Application/
        │   ├── Authentication/
        │   │   ├── Register/               # existing — handler extended to also issue a refresh token
        │   │   ├── Login/                  # existing — handler extended to also issue a refresh token
        │   │   ├── Refresh/                # new: RefreshCommand/Handler/Result (research.md #10)
        │   │   └── Logout/                 # new: LogoutCommand/Handler/Result (research.md #10)
        │   └── Common/
        │       ├── Abstractions/
        │       │   ├── IUserRepository.cs        # existing
        │       │   ├── IUnitOfWork.cs             # existing
        │       │   ├── IPasswordHasher.cs         # existing
        │       │   ├── IJwtTokenService.cs        # existing — access-token issuance, unchanged
        │       │   ├── IValidator<T>.cs           # existing
        │       │   ├── IRefreshTokenService.cs    # new — issue raw token + hash, hash a presented token
        │       │   └── IRefreshTokenRepository.cs # new — find-by-hash, add, bulk-revoke-active-for-user
        │       └── Behaviors/
        │           └── ValidationBehavior.cs  # existing
        └── Identity.Infrastructure/
            ├── Authentication/
            │   ├── PasswordHasher.cs         # existing — PBKDF2 implementation (research.md #2)
            │   ├── JwtTokenService.cs        # existing — JWT issuance (research.md #3)
            │   ├── JwtOptions.cs             # existing — gains RefreshTokenDays (default 14)
            │   └── RefreshTokenService.cs    # new — opaque token generation + SHA-256 hashing (research.md #9)
            └── Persistence/
                ├── IdentityDbContext.cs      # existing — gains a RefreshTokens DbSet
                ├── Repositories/
                │   ├── UserRepository.cs         # existing
                │   └── RefreshTokenRepository.cs # new — implements IRefreshTokenRepository
                ├── UnitOfWork.cs               # existing
                ├── Configurations/           # existing UserConfiguration + new RefreshTokenConfiguration
                └── Migrations/               # existing InitialCreate + new AddRefreshTokens migration

tests/ (src/Diaspora.Tests, mirroring the structure above per testing.md)
├── Modules/Identity/Domain/Users/
├── Modules/Identity/Domain/RefreshTokens/        # new
├── Modules/Identity/Application/Authentication/Register/
├── Modules/Identity/Application/Authentication/Login/
├── Modules/Identity/Application/Authentication/Refresh/  # new
├── Modules/Identity/Application/Authentication/Logout/   # new
└── Modules/Identity/Infrastructure/Persistence/
```

**Structure Decision**: Existing "Option 2: Web application" shape, already present in this
repository as `Client.Api` (backend host + SPA) and `Modules/Identity` (the one module this
feature touches). Every path above either already exists as an empty stub (per AGENTS.md's
instruction to verify before assuming) or is a new file/folder inside an already-established
project — no new project, module, or top-level folder is introduced.

## Complexity Tracking

*No entries — Constitution Check reported no violations requiring justification.*
