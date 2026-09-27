---

description: "Task list template for feature implementation"
---

# Tasks: Login and Registration

**Input**: Design documents from `/specs/002-login-registration/`

**Prerequisites**: [plan.md](plan.md), [spec.md](spec.md), [research.md](research.md), [data-model.md](data-model.md), [contracts/auth-api.md](contracts/auth-api.md), [quickstart.md](quickstart.md)

**Tests**: Included. The constitution's Quality by Default principle requires automated tests for
every behavior-changing change, and [testing.md](../../.claude/rules/testing.md) and plan.md's
Constitution Check already commit this feature to unit tests (Domain/Application), a small number
of Testcontainers-backed integration tests, and Vitest/RTL tests for the new frontend code.

**Organization**: Tasks are grouped by user story (spec.md priorities P1/P2/P3) to enable
independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (US1, US2, US3)
- Every task names its exact file path(s)

## Path Conventions

Existing repository layout (see plan.md Project Structure) — no new top-level folder:
- Backend: `src/Client.Api/`, `src/Modules/Identity/Identity.{Domain,Application,Infrastructure}/`
- Backend tests: `src/Diaspora.Tests/`, mirroring `src/` (per testing.md)
- Frontend: `src/Client.Api/spa/src/`, following Feature-Sliced Design layers

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Bring in the packages this feature needs before any code is written.

- [X] T001 Add the `Microsoft.AspNetCore.Authentication.JwtBearer` package reference to `src/Client.Api/Client.Api.csproj`
- [X] T002 [P] Add the `System.IdentityModel.Tokens.Jwt` package reference to `src/Modules/Identity/Identity.Infrastructure/Identity.Infrastructure.csproj`
- [X] T003 [P] Add `Testcontainers.PostgreSql` and `Microsoft.AspNetCore.Mvc.Testing` package references, plus a `ProjectReference` to `src/Client.Api/Client.Api.csproj`, in `src/Diaspora.Tests/Diaspora.Tests.csproj` (needed for the Testcontainers-backed persistence test and the two endpoint-level tests below)
- [X] T004 [P] Add the `react-router-dom` dependency to `src/Client.Api/spa/package.json` and run `npm install` in `src/Client.Api/spa`
- [X] T005 Add a `Jwt` configuration section (`Issuer`, `Audience`, `AccessTokenMinutes`) to `src/Client.Api/appsettings.Development.json`, and set the actual signing key (`Jwt:Key`) via `dotnet user-secrets set` for `src/Client.Api` per research.md #3 (never commit the key)

**Checkpoint**: Packages restored, `dotnet build src/Diaspora.sln` succeeds, `npm install` succeeds.

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: The one shared entity, the hashing/JWT abstractions, persistence, DI wiring, and
frontend session/routing scaffolding that every user story below depends on.

**⚠️ CRITICAL**: No user story task can start until this phase is complete.

- [X] T006 Implement the `User` entity (`Id`, `Email`, `NormalizedEmail`, `PasswordHash`, `PasswordSalt`, `PasswordHashIterations`, `FirstName`, `LastName`, `DateOfBirth`, `Location`, `CreatedAtUtc`) with a static `Register(...)` factory enforcing the invariants in data-model.md, in `src/Modules/Identity/Identity.Domain/Users/User.cs`
- [X] T007 [P] Unit tests for `User.Register` invariants (rejects empty/whitespace name/location, future date of birth, accepts valid input) in `src/Diaspora.Tests/Modules/Identity/Domain/Users/UserTests.cs`
- [X] T008 Define `IIdentityDbContext` with a `DbSet<User> Users` in `src/Modules/Identity/Identity.Application/Common/Abstractions/IIdentityDbContext.cs` (depends on T006)
- [X] T009 [P] Define the `IPasswordHasher` abstraction (`Hash`, `Verify`) in `src/Modules/Identity/Identity.Application/Common/Abstractions/IPasswordHasher.cs`
- [X] T010 [P] Define the `IJwtTokenService` abstraction (issue an access token + expiry for a `User`) in `src/Modules/Identity/Identity.Application/Common/Abstractions/IJwtTokenService.cs`
- [X] T011 Implement `PasswordHasher` using PBKDF2-HMAC-SHA256, a random 128-bit salt per call, and 210,000 iterations, with constant-time verification, per research.md #2, in `src/Modules/Identity/Identity.Infrastructure/Authentication/PasswordHasher.cs` (depends on T009)
- [X] T012 [P] Unit tests for `PasswordHasher` (hash/verify round-trip succeeds, wrong password is rejected, two hashes of the same password use different salts) in `src/Diaspora.Tests/Modules/Identity/Infrastructure/Authentication/PasswordHasherTests.cs`
- [X] T013 Implement `JwtTokenService` issuing an HS256 token with `sub`/`email`/`jti`/`iat`/`nbf`/`exp` claims and a 60-minute lifetime, per research.md #3, in `src/Modules/Identity/Identity.Infrastructure/Authentication/JwtTokenService.cs` (depends on T010)
- [X] T014 [P] Unit tests for `JwtTokenService` (issued token contains the expected claims and expiry) in `src/Diaspora.Tests/Modules/Identity/Infrastructure/Authentication/JwtTokenServiceTests.cs`
- [X] T015 Implement `IdentityDbContext : IIdentityDbContext` with the `Users` `DbSet` in `src/Modules/Identity/Identity.Infrastructure/Persistence/IdentityDbContext.cs` (depends on T006, T008)
- [X] T016 [P] Add `UserConfiguration : IEntityTypeConfiguration<User>` (snake_case columns per efcore-postgresql.md, unique index on `NormalizedEmail`) in `src/Modules/Identity/Identity.Infrastructure/Persistence/Configurations/UserConfiguration.cs`
- [X] T017 Generate the initial EF Core migration for the `users` table in `src/Modules/Identity/Identity.Infrastructure/Persistence/Migrations` (depends on T015, T016)
- [X] T018 [P] Testcontainers-backed integration test: a `User` persists and round-trips through `IdentityDbContext`, and the `NormalizedEmail` unique index rejects a case/whitespace-variant duplicate, in `src/Diaspora.Tests/Modules/Identity/Infrastructure/Persistence/IdentityDbContextTests.cs` (depends on T017)
- [X] T019 Fix `src/Modules/Identity/Identity.Application/DependencyInjection.cs` to expose `AddIdentityApplication(this IServiceCollection, IConfiguration)` (currently misnamed `AddIdentityInfrastructure`) registering MediatR for this assembly
- [X] T020 Update `src/Modules/Identity/Identity.Infrastructure/DependencyInjection.cs`'s `AddIdentityInfrastructure` to register `IdentityDbContext` (Npgsql, connection string from configuration), `IPasswordHasher` → `PasswordHasher`, `IJwtTokenService` → `JwtTokenService`, and bind the `Jwt` options from configuration (depends on T011, T013, T015)
- [X] T021 Wire `AddIdentityApplication`/`AddIdentityInfrastructure`, and JWT bearer authentication (`AddAuthentication().AddJwtBearer(...)` with issuer/audience/lifetime validation and a 30-second clock skew per research.md #3) plus `AddAuthorization`/`UseAuthentication`/`UseAuthorization`, into `src/Client.Api/Program.cs` (depends on T019, T020)
- [X] T022 [P] Add the app-level route table (`/`, `/register`, `/login`, `/home`) with an auth-aware route guard for `/home` in `src/Client.Api/spa/src/app/` (depends on T004)
- [X] T023 [P] Add the in-memory session/auth store (current user + access token, `login`/`logout` actions, nothing persisted to `localStorage`/cookies per research.md #5) in `src/Client.Api/spa/src/entities/session/`
- [X] T024 Update the shared axios client to attach `Authorization: Bearer <token>` from the session store in `src/Client.Api/spa/src/shared/api/` (depends on T023)
- [X] T025 [P] Add the empty Home page (placeholder content only, per spec.md scope) in `src/Client.Api/spa/src/pages/home/` (depends on T022)

**Checkpoint**: `dotnet build src/Diaspora.sln` succeeds, `dotnet test src/Diaspora.sln` passes for
everything added so far, the SPA builds, and navigating directly to `/home` unauthenticated
redirects to `/login`. User story implementation can now begin.

### Refresh-token infrastructure (added 2026-09-22 — research.md #8-#11, data-model.md `RefreshToken`)

**Purpose**: Shared infrastructure for persisting the session across a page refresh (spec.md
FR-012/FR-015/FR-016), needed before either Register (US1) or Login (US2) can be extended to
issue a refresh token, or the new Refresh/Logout endpoints can exist.

- [X] T055 [P] Implement the `RefreshToken` entity (`Id`, `UserId`, `TokenHash`, `CreatedAtUtc`, `ExpiresAtUtc`, `RevokedAtUtc`, `ReplacedByTokenHash`, computed `IsActive`) with a static `Issue(userId, tokenHash, createdAtUtc, expiresAtUtc)` factory and a `Revoke(revokedAtUtc, replacedByTokenHash = null)` method per data-model.md, in `src/Modules/Identity/Identity.Domain/RefreshTokens/RefreshToken.cs`
- [X] T056 [P] Unit tests for `RefreshToken` (`Issue` produces an active token; `Revoke` without a replacement marks it inactive; `Revoke` with a replacement sets `ReplacedByTokenHash`; `IsActive` is false once `ExpiresAtUtc` has passed) in `src/Diaspora.Tests/Modules/Identity/Domain/RefreshTokens/RefreshTokenTests.cs` (depends on T055)
- [X] T057 [P] Add `RefreshTokenDays` (default 14) to `JwtOptions` in `src/Modules/Identity/Identity.Infrastructure/Authentication/JwtOptions.cs`, and add `Jwt:RefreshTokenDays` to `src/Client.Api/appsettings.Development.json`
- [X] T058 [P] Define `IRefreshTokenService` (`Issue()` → a raw token + its hash + expiry; `Hash(rawToken)` for looking up a presented token) in `src/Modules/Identity/Identity.Application/Common/Abstractions/IRefreshTokenService.cs`
- [X] T059 [P] Define `IRefreshTokenRepository` (`FindByTokenHashAsync`, `AddRefreshToken`, `RevokeAllActiveForUserAsync`) in `src/Modules/Identity/Identity.Application/Common/Abstractions/IRefreshTokenRepository.cs`
- [X] T060 Implement `RefreshTokenService`: a 256-bit `RandomNumberGenerator` token, base64url-encoded for the cookie, hashed with SHA-256 for storage, expiry from `JwtOptions.RefreshTokenDays`, per research.md #9, in `src/Modules/Identity/Identity.Infrastructure/Authentication/RefreshTokenService.cs` (depends on T057, T058)
- [X] T061 [P] Unit tests for `RefreshTokenService` (issued raw tokens are unique/high-entropy; `Hash` is deterministic for the same input; issued expiry matches `RefreshTokenDays`) in `src/Diaspora.Tests/Modules/Identity/Infrastructure/Authentication/RefreshTokenServiceTests.cs` (depends on T060)
- [X] T062 Add a `RefreshTokens` `DbSet<RefreshToken>` to `IdentityDbContext` in `src/Modules/Identity/Identity.Infrastructure/Persistence/IdentityDbContext.cs` (depends on T055)
- [X] T063 [P] Add `RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>` (snake_case `refresh_tokens` table, foreign key to `users`, index on `token_hash`) per efcore-postgresql.md, in `src/Modules/Identity/Identity.Infrastructure/Persistence/Configurations/RefreshTokenConfiguration.cs`
- [X] T064 Implement `RefreshTokenRepository : IRefreshTokenRepository` in `src/Modules/Identity/Identity.Infrastructure/Persistence/Repositories/RefreshTokenRepository.cs` (depends on T059, T062)
- [X] T065 Generate the `AddRefreshTokens` EF Core migration for the `refresh_tokens` table in `src/Modules/Identity/Identity.Infrastructure/Persistence/Migrations` (depends on T062, T063)
- [X] T066 [P] Testcontainers-backed integration test: a `RefreshToken` persists and round-trips through `IdentityDbContext`, and a rotated token can be found by hash with `ReplacedByTokenHash` set, in `src/Diaspora.Tests/Modules/Identity/Infrastructure/Persistence/RefreshTokenRepositoryTests.cs` (depends on T065)
- [X] T067 Register `IRefreshTokenService` → `RefreshTokenService` and `IRefreshTokenRepository` → `RefreshTokenRepository`, and bind `Jwt:RefreshTokenDays` into the existing `JwtOptions` binding, in `src/Modules/Identity/Identity.Infrastructure/DependencyInjection.cs` (depends on T060, T064)
- [X] T068 Add `.AllowCredentials()` to the `"Spa"` CORS policy in `src/Client.Api/Program.cs` per research.md #10
- [X] T069 [P] Set `withCredentials: true` on the shared axios client in `src/Client.Api/spa/src/shared/api/` per research.md #10

**Checkpoint**: Refresh-token infrastructure is in place — Register/Login can now be extended to
issue one, and the new Refresh/Logout endpoints can be built on top of it.

---

## Phase 3: User Story 1 - Register for a new account (Priority: P1) 🎯 MVP

**Goal**: A new visitor can submit the registration form with valid values for every field and
end up with a created, persisted account and an authenticated session on the Home screen.

**Independent Test**: Submit `POST /api/auth/register` (or the registration form) with valid
values for every field and confirm a new account is created, a valid access token is returned,
and the SPA lands on `/home`.

### Tests for User Story 1

> Write these first; confirm they fail before the implementation tasks below make them pass.

- [X] T026 [P] [US1] Unit tests for `RegisterCommandHandler` covering: valid registration succeeds; duplicate email (including case/whitespace variants) is rejected; mismatched password confirmation is rejected; weak password, empty/too-long name or location, and invalid date of birth are each rejected with a field-specific error, in `src/Diaspora.Tests/Modules/Identity/Application/Authentication/Register/RegisterCommandHandlerTests.cs`
- [X] T027 [P] [US1] Endpoint tests (via `Microsoft.AspNetCore.Mvc.Testing`, against a Testcontainers-backed database) for `POST /api/auth/register` covering the 201/400/409 responses in contracts/auth-api.md, in `src/Diaspora.Tests/Client.Api/Controllers/Auth/RegisterControllerTests.cs`
- [X] T028 [P] [US1] Frontend tests for the registration form: valid submit stores the session and navigates to `/home`; a duplicate-email or password-mismatch response renders next to the relevant field, in `src/Client.Api/spa/src/features/register-user/registerForm.test.tsx`

### Implementation for User Story 1

- [X] T029 [US1] Implement `RegisterCommand` (`Email`, `Password`, `ConfirmPassword`, `FirstName`, `LastName`, `DateOfBirth`, `Location`) in `src/Modules/Identity/Identity.Application/Authentication/Register/RegisterCommand.cs`
- [X] T030 [US1] Implement `RegisterResult` (`UserId`, `Email`, `FirstName`, `LastName`, `AccessToken`, `ExpiresAtUtc`, plus a way to signal field validation errors or an email-conflict outcome) in `src/Modules/Identity/Identity.Application/Authentication/Register/RegisterResult.cs`
- [X] T031 [US1] Implement `RegisterCommandHandler`: validate every field per data-model.md's Validation Summary (FR-002/004/005/006/007), reject on normalized-email uniqueness conflict (FR-003), hash the password via `IPasswordHasher`, persist the `User` via `IIdentityDbContext`, issue a token via `IJwtTokenService`, and return `RegisterResult` in `src/Modules/Identity/Identity.Application/Authentication/Register/RegisterCommandHandler.cs` (depends on T029, T030)
- [X] T032 [US1] Define `RegisterRequest` and the shared `AuthResponse`/`UserSummary` response DTOs per contracts/auth-api.md in `src/Client.Api/Controllers/Auth/RegisterRequest.cs` and `src/Client.Api/Controllers/Auth/AuthResponse.cs`
- [X] T033 [US1] Implement `RegisterController` (`POST /api/auth/register`, `[AllowAnonymous]`): map `RegisterRequest` → `RegisterCommand`, dispatch via MediatR, and map the result to `201 Created` + `AuthResponse`, `400` + `ValidationProblemDetails`, or `409 Conflict` per contracts/auth-api.md, in `src/Client.Api/Controllers/Auth/RegisterController.cs` (depends on T031, T032)
- [X] T034 [US1] Implement the `register-user` feature (form fields, client-side pre-submit checks, a submit hook that calls `POST /api/auth/register` and maps `ValidationProblemDetails`/`409` onto per-field messages) in `src/Client.Api/spa/src/features/register-user/`
- [X] T035 [US1] Implement the Register page composing the `register-user` feature; on success, store the returned user/token in the session store and navigate to `/home` in `src/Client.Api/spa/src/pages/register/`
- [X] T036 [US1] Wire `/register` to the Register page in the app route table in `src/Client.Api/spa/src/app/` (depends on T022, T035)

**Checkpoint**: User Story 1 is fully functional and independently testable — a new visitor can
register and reach an authenticated Home screen.

### Refresh-token extension for User Story 1 (added 2026-09-22 — spec.md FR-012)

- [X] T070 [US1] Extend `RegisterCommand`/`RegisterResult` to carry a raw refresh token + its expiry, and extend `RegisterCommandHandler` to also issue and persist a `RefreshToken` via `IRefreshTokenService`/`IRefreshTokenRepository` alongside the access token, in `src/Modules/Identity/Identity.Application/Authentication/Register/` (depends on T060, T064)
- [X] T071 [US1] Update `RegisterController` to set the `refreshToken` cookie (`HttpOnly`/`Secure`/`SameSite=Strict`/`Path=/api/auth`) on `201 Created` per contracts/auth-api.md, in `src/Client.Api/Controllers/Auth/RegisterController.cs` (depends on T070, T068)
- [X] T072 [P] [US1] Extend the registration endpoint tests to assert the `refreshToken` cookie is set with the expected attributes on success, in `src/Diaspora.Tests/Client.Api/Controllers/Auth/RegisterControllerTests.cs` (depends on T071)

**Checkpoint**: Registering now also starts a session that survives a page refresh, not just an
in-memory-only one.

---

## Phase 4: User Story 2 - Log in to an existing account (Priority: P2)

**Goal**: A user with an existing account can log in with their email and password, reach the
Home screen, and later log out.

**Independent Test**: Given an account already exists (from User Story 1 or seeded directly),
submit `POST /api/auth/login` (or the login form) with the correct credentials and confirm an
access token is returned and the SPA lands on `/home`; confirm wrong credentials are rejected
with one generic message.

### Tests for User Story 2

- [X] T037 [P] [US2] Unit tests for `LoginCommandHandler` covering: correct credentials succeed and return a token; a wrong password and an unregistered email both fail with the same generic invalid-credentials outcome (FR-011), in `src/Diaspora.Tests/Modules/Identity/Application/Authentication/Login/LoginCommandHandlerTests.cs`
- [X] T038 [P] [US2] Endpoint tests for `POST /api/auth/login` covering the 200/400/401 responses in contracts/auth-api.md, in `src/Diaspora.Tests/Client.Api/Controllers/Auth/LoginControllerTests.cs`
- [X] T039 [P] [US2] Frontend tests for the login form and logout: valid submit stores the session and navigates to `/home`; wrong credentials show one generic error (not field-specific); logging out from Home clears the session and returns to `/login`, in `src/Client.Api/spa/src/features/login-user/loginForm.test.tsx`

### Implementation for User Story 2

- [X] T040 [US2] Implement `LoginCommand` (`Email`, `Password`) in `src/Modules/Identity/Identity.Application/Authentication/Login/LoginCommand.cs`
- [X] T041 [US2] Implement `LoginResult` (`UserId`, `Email`, `FirstName`, `LastName`, `AccessToken`, `ExpiresAtUtc`, plus a way to signal invalid credentials) in `src/Modules/Identity/Identity.Application/Authentication/Login/LoginResult.cs`
- [X] T042 [US2] Implement `LoginCommandHandler`: look up the user by normalized email, verify the password via `IPasswordHasher` in constant time, return one generic invalid-credentials failure without revealing which field was wrong (FR-011) on any mismatch, otherwise issue a token via `IJwtTokenService`, in `src/Modules/Identity/Identity.Application/Authentication/Login/LoginCommandHandler.cs` (depends on T040, T041)
- [X] T043 [US2] Define `LoginRequest` in `src/Client.Api/Controllers/Auth/LoginRequest.cs`
- [X] T044 [US2] Implement `LoginController` (`POST /api/auth/login`, `[AllowAnonymous]`): map `LoginRequest` → `LoginCommand`, dispatch via MediatR, and map the result to `200 OK` + `AuthResponse`, `400` + `ValidationProblemDetails`, or `401 Unauthorized` per contracts/auth-api.md, in `src/Client.Api/Controllers/Auth/LoginController.cs` (depends on T042, T043, T032)
- [X] T045 [US2] Implement the `login-user` feature (form fields, a submit hook that calls `POST /api/auth/login` and renders the single generic error on failure) in `src/Client.Api/spa/src/features/login-user/`
- [X] T046 [US2] Implement the Login page composing the `login-user` feature; on success, store the returned user/token in the session store and navigate to `/home` in `src/Client.Api/spa/src/pages/login/`
- [X] T047 [US2] Wire `/login` to the Login page in the app route table, and add a logout control on the Home page that clears the session store and returns to `/login`, in `src/Client.Api/spa/src/app/` and `src/Client.Api/spa/src/pages/home/` (depends on T022, T025, T046)

**Checkpoint**: User Stories 1 and 2 both work independently — register-then-use and
login-then-use are both complete, end-to-end flows.

### Session persistence across refresh (added 2026-09-22 — spec.md User Story 2 acceptance scenarios 4/5, FR-012, FR-015, FR-016; research.md #9-#11)

**Goal**: A logged-in user (via register or login) stays authenticated across a page refresh or
browser restart until the refresh token expires (14 days of inactivity) or they explicitly log
out; a reused, already-rotated refresh token ends all of that user's sessions; logout is now a
server-side revocation.

**Independent Test**: Log in, refresh the browser tab, and confirm the user is still on `/home`
authenticated with no re-login. Log out, then attempt `POST /api/auth/refresh` with the
now-revoked cookie and confirm `401`. Capture two successive refresh-token cookie values (before
and after one rotation) and confirm presenting the older one is rejected and also revokes the
newer one (quickstart.md §5 steps 8-11).

- [X] T073 [US2] Extend `LoginCommand`/`LoginResult` and `LoginCommandHandler` to also issue and persist a refresh token, mirroring T070, in `src/Modules/Identity/Identity.Application/Authentication/Login/` (depends on T060, T064)
- [X] T074 [US2] Update `LoginController` to set the `refreshToken` cookie on `200 OK`, mirroring T071, in `src/Client.Api/Controllers/Auth/LoginController.cs` (depends on T073, T068)
- [X] T075 [P] [US2] Extend the login endpoint tests to assert the `refreshToken` cookie is set on success, in `src/Diaspora.Tests/Client.Api/Controllers/Auth/LoginControllerTests.cs` (depends on T074)
- [X] T076 [P] [US2] Implement `RefreshCommand`/`RefreshResult` (input: the raw refresh token from the cookie; output: a new access token + new refresh token + user summary, or a failure) in `src/Modules/Identity/Identity.Application/Authentication/Refresh/`
- [X] T077 [US2] Implement `RefreshCommandHandler`: hash the presented token and look it up; unknown or expired → fail; already revoked (reuse) → revoke every other active token for that user (FR-015) and fail; otherwise rotate — issue+persist a new `RefreshToken`, revoke the presented one with `ReplacedByTokenHash` set — and issue a new access token, per research.md #9, in `src/Modules/Identity/Identity.Application/Authentication/Refresh/RefreshCommandHandler.cs` (depends on T076, T060, T064)
- [X] T078 [P] [US2] Unit tests for `RefreshCommandHandler` covering: an active token rotates successfully; an unknown/expired token fails; a reused (already-revoked) token fails and revokes every other active token for that user, in `src/Diaspora.Tests/Modules/Identity/Application/Authentication/Refresh/RefreshCommandHandlerTests.cs` (depends on T077)
- [X] T079 [US2] Implement `RefreshController` (`POST /api/auth/refresh`, `[AllowAnonymous]`, no request body — reads `Request.Cookies["refreshToken"]`): dispatch via MediatR, map success to `200 OK` + `AuthResponse` + a rotated cookie, map failure to `401` + a cleared cookie, per contracts/auth-api.md, in `src/Client.Api/Controllers/Auth/RefreshController.cs` (depends on T077)
- [X] T080 [P] [US2] Endpoint tests for `POST /api/auth/refresh` covering the `200`/`401` responses and cookie rotation/clearing, in `src/Diaspora.Tests/Client.Api/Controllers/Auth/RefreshControllerTests.cs` (depends on T079)
- [X] T081 [P] [US2] Implement `LogoutCommand`/`LogoutResult` (input: the raw refresh token from the cookie; always succeeds) in `src/Modules/Identity/Identity.Application/Authentication/Logout/`
- [X] T082 [US2] Implement `LogoutCommandHandler`: if the presented token hashes to a known row, revoke it; a missing/unknown/already-revoked token is a no-op, not an error (FR-016), in `src/Modules/Identity/Identity.Application/Authentication/Logout/LogoutCommandHandler.cs` (depends on T081, T064)
- [X] T083 [US2] Implement `LogoutController` (`POST /api/auth/logout`, `[AllowAnonymous]`): dispatch via MediatR, clear the `refreshToken` cookie, always return `204 No Content` per contracts/auth-api.md, in `src/Client.Api/Controllers/Auth/LogoutController.cs` (depends on T082)
- [X] T084 [P] [US2] Endpoint tests for `POST /api/auth/logout` covering idempotent `204` with and without a valid cookie, and that a subsequent `POST /api/auth/refresh` using the now-revoked cookie fails, in `src/Diaspora.Tests/Client.Api/Controllers/Auth/LogoutControllerTests.cs` (depends on T083, T079)
- [X] T085 [US2] Add a `status: 'resolving' | 'authenticated' | 'anonymous'` field to the session store, plus a mount-time effect that calls `POST /api/auth/refresh` (credentials included) and resolves into `authenticated`/`anonymous` before anything auth-dependent renders, in `src/Client.Api/spa/src/entities/session/` (depends on T069, T079)
- [X] T086 [US2] Update `RequireAuth` to render a brief loading state while `status === 'resolving'` instead of redirecting immediately, in `src/Client.Api/spa/src/app/` (depends on T085)
- [X] T087 [US2] Add an axios response interceptor that, on a `401` from an authenticated call, attempts one silent `POST /api/auth/refresh` and retries the original request before surfacing the failure, in `src/Client.Api/spa/src/shared/api/` (depends on T069, T079)
- [X] T088 [US2] Update the Home page's logout control to call `POST /api/auth/logout` before clearing the session store and navigating to `/login`, in `src/Client.Api/spa/src/pages/home/` (depends on T083)
- [X] T089 [P] [US2] Frontend tests: reloading while authenticated keeps the user on `/home` (mocking a successful silent refresh); an expired/absent refresh-token cookie on mount redirects to `/login`; logging out calls `POST /api/auth/logout` before clearing state, in `src/Client.Api/spa/src/entities/session/session.test.tsx` (depends on T085, T086, T088)

**Checkpoint**: User Story 2 now also covers session persistence across a page refresh and
server-side logout, satisfying spec.md's updated acceptance scenarios 4/5 and FR-015/FR-016.

---

## Phase 5: User Story 3 - Understand and correct invalid input (Priority: P3)

**Goal**: Every class of invalid input on the registration or login form produces a specific,
field-relevant message, so nothing fails silently or only with a generic error.

**Independent Test**: Submit the registration form with each invalid-input class one at a time
(malformed email, weak password, empty required field, invalid date of birth) and confirm each
produces a distinct, field-relevant message; submit login with an unregistered email and confirm
the single generic message from User Story 2 still applies (a login failure is intentionally not
field-specific, per FR-011).

### Tests for User Story 3

- [X] T048 [P] [US3] Tests for the shared field-validation messages (one assertion per rule in data-model.md's Validation Summary: malformed email, short/weak password, empty first/last name, empty/too-long location, future or under-minimum-age date of birth) in `src/Client.Api/spa/src/shared/lib/validation.test.ts`

### Implementation for User Story 3

- [X] T049 [P] [US3] Extract the field-validation rules shared by both forms (email format, password strength, required-field/length checks, date-of-birth bounds) into a small reusable module, replacing any duplicated inline checks in `register-user`/`login-user`, in `src/Client.Api/spa/src/shared/lib/validation.ts`
- [X] T050 [US3] Ensure every backend `ValidationProblemDetails` field key from `POST /api/auth/register` (data-model.md's Validation Summary) is mapped to the matching form field in the UI, rather than shown as a single combined message, in `src/Client.Api/spa/src/features/register-user/` (depends on T034, T049)

**Checkpoint**: All three user stories are independently functional; registration and login give
specific, actionable feedback on every invalid input.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Final validation across all three stories.

- [X] T051 [P] Run `dotnet format src/Diaspora.sln` and resolve any warnings in the files this feature touched
- [X] T052 [P] Run `npm run lint` in `src/Client.Api/spa` and resolve any warnings in the files this feature touched
- [X] T053 Run `dotnet test src/Diaspora.sln` and `npm run test` in `src/Client.Api/spa`, and confirm the full suite passes
- [X] T054 Walk through every scenario in quickstart.md §5 against the running app (`dotnet run --project src/Client.Api` + `npm run dev`) and confirm each spec.md acceptance scenario holds
- [X] T090 [P] Re-run `dotnet format src/Diaspora.sln` and `npm run lint` (in `src/Client.Api/spa`) and resolve any new warnings from the refresh-token work
- [X] T091 Re-run `dotnet test src/Diaspora.sln` and `npm run test` (in `src/Client.Api/spa`) and confirm the full suite, including the new refresh-token tests, passes
- [X] T092 Walk through quickstart.md §5 steps 8-11 (session persists across refresh, refresh-token rotation, server-side logout revocation, reused-token rejection) against the running app and confirm spec.md SC-006, FR-015, and FR-016 all hold

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — start immediately.
- **Foundational (Phase 2)**: Depends on Setup — BLOCKS all user stories. Its refresh-token
  infrastructure subsection (T055-T069, added 2026-09-22) BLOCKS the refresh-token extensions in
  Phase 3 and Phase 4 below, but not the original T026-T047 tasks, which do not depend on it.
- **User Story 1 (Phase 3)**: Depends only on Foundational. Its refresh-token extension
  (T070-T072) additionally depends on the refresh-token infrastructure subsection of Phase 2.
- **User Story 2 (Phase 4)**: Depends only on Foundational (independently testable given a seeded
  account; does not require User Story 1's code, only that some account exists). Its session-
  persistence subsection (T073-T089) additionally depends on the refresh-token infrastructure
  subsection of Phase 2, and T085-T089 (frontend rehydration) depend on T079 (`RefreshController`)
  existing.
- **User Story 3 (Phase 5)**: Depends on Foundational, and touches files User Story 1 (and
  optionally User Story 2) already created — implement after Phase 3 (and ideally Phase 4) for a
  clean diff, even though its own validation logic is additive/refactoring in nature.
- **Polish (Phase 6)**: Depends on every user story phase you choose to complete, including the
  refresh-token extensions if you complete them.

### Within Each User Story

- Tests are written first and must fail before the implementation tasks in that story.
- Commands/Results before their handler; handler before the controller; controller before the
  frontend feature; feature before the page; page before route wiring.

### Parallel Opportunities

- All Setup tasks marked `[P]` (T002–T004) can run together.
- Within Foundational: T007, T009+T010, T012, T014, T016, T018, T022+T023, T025 are each
  independent of the other `[P]`-marked tasks at that point (see per-task dependency notes).
- Once Foundational is complete, User Story 1 and User Story 2 can be implemented in parallel by
  different contributors (they touch disjoint files except the shared `AuthResponse`/DTO file
  from T032, which US1 creates and US2 only reads).
- Within User Story 1: T026, T027, T028 run in parallel; within User Story 2: T037, T038, T039
  run in parallel.
- Within the refresh-token infrastructure subsection of Phase 2: T055, T057, T058, T059 can start
  together; T061 and T066 run in parallel with each other once their respective dependencies
  (T060; T065) land; T069 is independent of the backend tasks in that subsection.
- T072 (US1) and T075 (US2) are independent of each other. Within US2's session-persistence
  subsection, T076/T081 (the two new command/result shapes) can start together, and T078/T080/
  T084/T089 each run in parallel with the other `[P]`-marked test tasks once their own
  implementation dependency is met.

---

## Parallel Example: User Story 1

```bash
# Tests, launched together:
Task: "Unit tests for RegisterCommandHandler in src/Diaspora.Tests/Modules/Identity/Application/Authentication/Register/RegisterCommandHandlerTests.cs"
Task: "Endpoint tests for POST /api/auth/register in src/Diaspora.Tests/Client.Api/Controllers/Auth/RegisterControllerTests.cs"
Task: "Frontend tests for the registration form in src/Client.Api/spa/src/features/register-user/registerForm.test.tsx"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup.
2. Complete Phase 2: Foundational (blocks everything else).
3. Complete Phase 3: User Story 1 (Register).
4. **Stop and validate**: a new visitor can register and land on Home.
5. Demo if ready — this alone is a viable, observable increment.

### Incremental Delivery

1. Setup + Foundational → foundation ready.
2. User Story 1 (Register) → validate independently → demo (MVP).
3. User Story 2 (Login/Logout) → validate independently → demo.
4. User Story 3 (Field-specific validation feedback) → validate independently → demo.
5. Refresh-token infrastructure (Phase 2 addition, T055-T069) → User Story 1's refresh-token
   extension (T070-T072) → User Story 2's session-persistence subsection (T073-T089) → validate
   independently (quickstart.md §5 steps 8-11) → demo.
6. Polish → run the full quickstart.md walkthrough once more end-to-end, including steps 8-11.

## Notes

- `[P]` tasks touch different files with no unmet dependency.
- Every task names its file path(s); none require guessing repository structure.
- Commit after each task or logical group, per this repository's normal workflow.
- Verify each story's tests fail before implementing that story, and pass once it's done.
