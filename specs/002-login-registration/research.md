# Phase 0 Research: Login and Registration

Research for open technical questions raised by [spec.md](spec.md) and by the additional
direction given for this planning pass (use a JWT auth scheme; follow general security best
practice for password handling; land on an empty Home screen after register/login).

## 1. Scope of "no third-party libraries"

**Decision**: The spec's "no third-party libs" constraint (spec.md Assumptions) applies to
libraries that would implement the *auth/identity/validation logic itself* on the project's
behalf — a hosted identity provider, an identity/user-management package (e.g.
`Microsoft.AspNetCore.Identity`), a general-purpose validation library (e.g. FluentValidation),
or a third-party password-hashing package (e.g. BCrypt.Net). It does **not** rule out the
project's already-adopted foundational stack (ASP.NET Core, EF Core/Npgsql, MediatR, React) or
the standard ASP.NET Core building blocks for *transporting and validating* a JWT
(`Microsoft.AspNetCore.Authentication.JwtBearer`, `System.IdentityModel.Tokens.Jwt`) — these are
Microsoft-maintained framework primitives, not an auth *service* or identity *system*, and hand-
rolling JWT parsing/signature verification would itself be a security risk the constitution's
Quality by Default principle argues against.

**Rationale**: This keeps the explicit ask ("Use JWT auth scheme") satisfiable while preserving
the spirit of the original constraint — the business logic of *who is registered*, *how a
password is verified*, and *how field validation is enforced* remains project-owned code, which
is also where the learning value (Principle I) actually is.

**Alternatives considered**:
- Hand-roll JWT encoding/signing/verification with no library at all — rejected: reimplementing
  base64url-safe JSON signing and constant-time signature verification correctly is exactly the
  kind of cryptographic code best not hand-rolled; the risk/learning trade-off does not favor it,
  and `JwtBearer` is already the idiomatic ASP.NET Core mechanism assumed by
  [aspnetcore-webapi.md](../../.claude/rules/aspnetcore-webapi.md).
- Use `Microsoft.AspNetCore.Identity` for user/password management — rejected: it is an
  identity/user-management framework, exactly what FR-014 excludes, and it would also impose its
  own `IdentityDbContext`/schema shape that conflicts with the module owning a plain `User`
  entity.

## 2. Password hashing and salting

**Decision**: PBKDF2-HMAC-SHA256 via the BCL (`System.Security.Cryptography.Rfc2898DeriveBytes.Pbkdf2`),
with a per-user random 128-bit salt (`RandomNumberGenerator`), 600,000 iterations (above OWASP's
2023 minimum recommendation of 210,000 for PBKDF2-HMAC-SHA256), and a 256-bit derived key.
Iteration count is stored alongside the hash/salt so it can be raised later without invalidating
existing hashes. Verification compares in constant time
(`CryptographicOperations.FixedTimeEquals`). To avoid a timing side-channel that would reveal
whether an email is registered, a login attempt against an unknown email still runs a dummy
PBKDF2 derivation (same 600,000 iterations) before returning the generic invalid-credentials
response.

**Rationale**: PBKDF2 is available in the BCL with no external package, is OWASP-endorsed, and
directly demonstrates the "hashing + per-user salt + work factor" concepts the user asked to see
applied. Storing the iteration count makes the scheme upgradable (a standard practice) without
extra infrastructure. 600,000 iterations was chosen over the 210,000 OWASP minimum for extra
margin, given PBKDF2-HMAC-SHA256's relatively low per-iteration cost.

**Alternatives considered**:
- Argon2id (currently OWASP's *first* recommendation) — rejected for now: no BCL implementation
  exists, so it would require a third-party package, which conflicts with the explicit
  no-third-party-library constraint for this area.
- A fixed/global salt or no salt — rejected: defeats the purpose of salting (precomputed
  rainbow-table attacks across all users), directly against "general security guidelines."

## 3. JWT issuance and validation

**Decision**: Symmetric HMAC-SHA256 (HS256) signing with a secret read from configuration. In any
real environment (staging, production) the secret MUST come from user-secrets/environment
configuration and MUST NOT be committed. For local development only, a non-production placeholder
secret is committed in `appsettings.Development.json` so the app runs out-of-the-box after a
fresh clone with no manual setup step — this is an accepted convention for a placeholder value
with no real-world sensitivity, not an exception for a real secret. Claims: `sub` (user id), `email`, `jti` (unique token id, for future revocation support), `iat`,
`nbf`, `exp`. Access token lifetime: 60 minutes. Validated via
`Microsoft.AspNetCore.Authentication.JwtBearer`, with issuer/audience/lifetime validation enabled
and a small clock-skew allowance (30 seconds) instead of the library's 5-minute default.

**Rationale**: HS256 with one shared secret is sufficient for a single backend that both issues
and validates its own tokens (no external relying parties yet) — asymmetric signing (RS256) would
add key-management complexity with no present benefit, which Simplicity First argues against.

**Alternatives considered**:
- Asymmetric RS256/ES256 — rejected for now: only pays off once a second, independent service
  needs to verify tokens without holding the signing secret; no such need exists yet.
- Long-lived access tokens with no expiry check — rejected: directly contradicts "general
  security guidelines" for token-based auth (bounded token lifetime limits the damage of a leaked
  token).

## 4. Session/logout semantics for a stateless JWT

**Decision**: No server-side session store or token blacklist in this iteration. "Login"
establishes an authenticated state by handing the client a signed, time-bounded JWT; "logout" is
a client-side action that discards the token and returns the user to an unauthenticated state.
The 60-minute expiry (Decision 3) bounds how long a token remains usable after logout.

**Rationale**: Matches spec.md's Assumptions (no persistent/"remember me" login, no
password-reset flow in scope) and the constitution's Simplicity First principle — a revocation
list is real, justified complexity only once a concrete need (e.g., "kill a compromised session
immediately") is articulated; today's requirement (FR-012: user can log out, ending their
session) is satisfied by the client no longer holding or sending a usable token.

**Alternatives considered**:
- Server-side token/session blacklist keyed by `jti` — rejected for now as premature; the module
  has no persistence need for it yet and it would add a write path with no current consumer.

## 5. Where the JWT lives on the client

**Decision**: The access token is kept in memory only (a small React auth store/context in the
`entities/session` slice), never written to `localStorage`/`sessionStorage`/cookies. A refresh of
the page clears it, which is acceptable because "remember me"/persistent login is explicitly out
of scope (spec.md Assumptions). Outbound requests attach it via an `Authorization: Bearer <token>`
header through the existing centralized `axios` client.

**Rationale**: In-memory storage is not readable by a same-origin script the way
`localStorage`/`sessionStorage` is, and avoids CSRF concerns that a cookie-based token would
otherwise require mitigating (e.g., `SameSite`, anti-CSRF tokens) — the simplest option that still
meets "general security guidelines" for where a bearer token is held client-side.

**Alternatives considered**:
- `localStorage`/`sessionStorage` — rejected: persistently readable by any script running on the
  page, the most common vector for JWT theft via XSS.
- `httpOnly` secure cookie — more XSS-resistant, but requires `SameSite`/CSRF handling and
  `AllowCredentials` CORS changes across the still cross-origin dev setup (SPA on `:5173`, API on
  a different port); deferred as unjustified complexity until a concrete need (e.g., persistent
  login) arises.

## 6. Frontend routing to a Home screen

**Decision**: Add `react-router-dom` and introduce a minimal route table in the `app` FSD layer:
`/` (existing welcome page), `/register`, `/login`, `/home` (new, empty placeholder). A route
guard component redirects an unauthenticated visitor away from `/home` back to `/login`, and
successful register/login navigates to `/home`.

**Rationale**: The existing SPA has no routing at all (single static `App.tsx`); multiple screens
(welcome, register, login, home) require it. `react-router-dom` is the de facto standard for
React SPA routing and is unrelated to the auth/validation logic the "no third-party libs"
constraint targets.

**Alternatives considered**:
- Hand-rolled routing (conditional rendering keyed off local state) — rejected: reinvents
  browser history/URL handling for no benefit once more than one or two screens exist, and would
  make the upcoming Home screen and future features harder to extend.

## 7. Data storage conventions

**Decision**: Follow [efcore-postgresql.md](../../.claude/rules/efcore-postgresql.md) as already
written: one `IdentityDbContext` owned by the Identity module, `IEntityTypeConfiguration<User>`
for mapping, snake_case naming convention, migrations committed to `Identity.Infrastructure`. A
`normalized_email` column (uppercase/trimmed) carries a unique index used for case/whitespace-
insensitive lookups and the uniqueness check in FR-003, rather than a database extension
(`citext`) that would add an operational dependency not currently justified.

**Rationale**: Reuses the already-documented, already-referenced (`Npgsql.EntityFrameworkCore.PostgreSQL`)
persistence conventions rather than introducing a new pattern for this one module.

**Alternatives considered**:
- PostgreSQL `citext` column type — rejected: requires enabling a database extension purely to
  save one normalization step; a plain unique index on a normalized column achieves the same
  constraint with zero new operational surface.
