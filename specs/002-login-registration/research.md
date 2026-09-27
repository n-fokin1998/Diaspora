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

## 4. Session/logout semantics for a stateless JWT (superseded — see #9/#10)

**Status**: Superseded on 2026-09-22. Persisting the session across a page refresh (spec.md's
scope addition) requires *some* server-side record to survive the refresh, which is exactly what
this decision's "no server-side session store" ruled out; #9 introduces the `RefreshToken` table
and #10 makes logout a server-side revocation again. Kept here as the record of what was
originally decided and why.

**Original decision**: No server-side session store or token blacklist in this iteration. "Login"
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

## 5. Where the JWT lives on the client (superseded — see #8)

**Status**: Superseded on 2026-09-22 by Decision #8 below. Persisting the session across a page
refresh became a concrete, explicit requirement (spec.md's 2026-09-22 scope addition), which the
in-memory-only design below cannot satisfy by construction — a page refresh clears all JS memory.
Kept here, per the constitution's Principle IX, as the record of what was originally decided and
why, since #8 needs it as the thing it replaces.

**Original decision**: The access token is kept in memory only (a small React auth store/context in the
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

## 8. Hybrid access/refresh token storage on the client (supersedes #5)

**Decision**: The access token keeps living only in memory (React state in `entities/session`),
exactly as #5 originally decided — that part was never the problem. What's added is a **refresh
token**, delivered as an `httpOnly`, `Secure`, `SameSite=Strict` cookie scoped to
`Path=/api/auth`, set by the server on register/login/refresh and cleared by the server on
logout. On every app load (including a page refresh), the SPA calls `POST /api/auth/refresh`
with credentials included; the browser attaches the cookie automatically (JavaScript never reads
or writes it), and the response's fresh access token is placed into memory before any
authenticated route renders.

**Rationale**: This is the widely-documented "hybrid" pattern for JWTs in a browser SPA — an
in-memory access token (short-lived, invisible to `localStorage`/`sessionStorage` inspection,
gone the instant the tab is closed) plus an `httpOnly` refresh token (never readable by page
JavaScript at all, so an XSS payload cannot exfiltrate the one credential that actually survives
a refresh) — and is exactly what the user asked for by name. It satisfies the new
persist-across-refresh requirement (spec.md FR-012) while keeping the access token's exposure
window at 60 minutes (research.md #3) and confining the only long-lived secret to a location
script cannot touch.

**Alternatives considered**:
- Keep #5's in-memory-only design, no refresh mechanism — rejected: cannot satisfy
  persist-across-refresh at all; a page refresh clears all JS memory by definition.
- Put the access token itself in an `httpOnly` cookie, sent automatically on every request —
  rejected: couples a short-lived, frequently-reissued token to cookie semantics and would
  require CSRF defenses on every single authenticated endpoint, not just the two auth endpoints
  that actually need a cookie (see #10).
- Refresh token in `localStorage`/`sessionStorage` — rejected: identical XSS exposure to the
  anti-pattern #5 already rejected for the access token; defeats the entire point of moving to a
  hybrid scheme.

## 9. Refresh token format, storage, and rotation

**Decision**: The refresh token is a 256-bit cryptographically random opaque value
(`RandomNumberGenerator`, base64url-encoded for the cookie) — not a JWT; it carries no claims and
is meaningless without a database lookup. The server persists only a SHA-256 hash of the value
(never the raw token) in a new `refresh_tokens` table: `UserId`, `TokenHash`, `CreatedAtUtc`,
`ExpiresAtUtc`, `RevokedAtUtc` (nullable), `ReplacedByTokenHash` (nullable — the hash of the
token that superseded this one, for rotation-chain / reuse detection).

Refresh tokens **rotate on every use**: `POST /api/auth/refresh` issues a brand-new refresh
token, marks the presented one revoked with `ReplacedByTokenHash` pointing at the new one's hash,
and sets the new token as the cookie. Lifetime: 14 days from each token's own issuance (not a
single fixed session start — continuous use keeps the session alive indefinitely; 14 days of
inactivity ends it), configured via a new `Jwt:RefreshTokenDays` setting alongside the existing
`Jwt:*` keys (research.md #3).

If an **already-revoked** token is presented again — the signature of a stolen, replayed token,
since the legitimate client would only ever hold the latest one — the handler treats it as
suspected compromise: it revokes every other active refresh token for that user (per spec.md's
new FR-015) and returns `401`, forcing re-login everywhere rather than just for that one token.

**Rationale**: Hashing before storage mirrors the password-hashing precedent (research.md #2) —
a database read does not hand out a directly usable credential. Rotation with reuse detection is
the standard OAuth2/OWASP-recommended mitigation for refresh-token theft and is proportionate
complexity here (one extra `RevokedAtUtc` check plus a bulk revoke query), not a general-purpose
session-management subsystem.

**Alternatives considered**:
- No rotation (one long-lived refresh token reused until expiry) — rejected: a token stolen once
  stays valid and undetectable for up to 14 days; rotation turns theft into a detectable event
  the next time the legitimate client's now-stale copy is also used.
- Store the raw refresh token value — rejected: same reasoning as storing plaintext passwords; a
  hash is just as effective for a high-entropy random value and costs nothing extra to verify.
- A JWT-format refresh token — rejected: a refresh token's only job is "look up its row so it can
  be revoked/rotated"; a self-describing JWT adds parsing/claims machinery for no benefit over a
  bare random id (Simplicity First).

## 10. Backend delivery: new use cases, cookie attributes, and CORS

**Decision**: Two new Application use cases, mirroring the existing `Authentication/Register` and
`Authentication/Login` vertical slices (architecture.md): `Authentication/Refresh` and
`Authentication/Logout`, dispatched via MediatR, each taking the raw refresh-token cookie value
as its only input (no request body). `RegisterCommandHandler`/`LoginCommandHandler` are extended
to also issue and persist a refresh token via two new `Common/Abstractions`:
`IRefreshTokenService` (generate a raw token + its hash; hash a presented raw token for lookup —
implemented in `Identity.Infrastructure/Authentication/RefreshTokenService.cs`) and
`IRefreshTokenRepository` (find-by-hash, add, bulk-revoke-active-for-user — implemented in
`Identity.Infrastructure/Persistence/Repositories/RefreshTokenRepository.cs`). `RegisterResult`/
`LoginResult` gain the raw refresh token and its expiry as result fields carried only to the
controller — never serialized into the JSON `AuthResponse` body (see
[contracts/auth-api.md](contracts/auth-api.md)); the controller writes it directly to the
response cookie.

Two new controllers in `Client.Api/Controllers/Auth/`, matching the existing
one-controller-per-action file convention: `RefreshController` (`POST /api/auth/refresh`,
`[AllowAnonymous]` — the access token may already be expired when this is called) and
`LogoutController` (`POST /api/auth/logout`, also `[AllowAnonymous]` — if the cookie is missing
or already invalid, logout still succeeds idempotently). Both read
`Request.Cookies["refreshToken"]` and, on success, write it back via
`Response.Cookies.Append("refreshToken", token, cookieOptions)` (Refresh) or
`Response.Cookies.Delete("refreshToken", ...)` (Logout).

Cookie options: `HttpOnly = true`, `Secure = true`, `SameSite = SameSiteMode.Strict`,
`Path = "/api/auth"`, `Expires` set to the refresh token's own `ExpiresAtUtc`. `Secure` needs no
local-dev exception (unlike the JWT signing secret in research.md #3): Chrome/Firefox treat
`localhost` as a secure context regardless of scheme, and this repository's
`Client.Api/Properties/launchSettings.json` already runs the API on plain
`http://localhost:5220` by default — a `Secure` cookie still gets set and sent there.

`Program.cs`'s existing `"Spa"` CORS policy (`WithOrigins("http://localhost:5173")`) gains
`.AllowCredentials()` — compatible with a specific-origin policy (ASP.NET Core only forbids
combining this with `AllowAnyOrigin`, which this policy was never using). The SPA's `axios`
client must set `withCredentials: true` so the browser attaches the cookie on these two
cross-origin calls. No new secret is introduced for any of this — the refresh token's security
comes from its own randomness and hashed storage (#9), not from the shared JWT signing key.

**Rationale**: Reuses every existing pattern in the module (vertical-slice use cases,
single-purpose abstractions, one-controller-per-action, the `JwtOptions`-style config-binding
`??`-with-throw pattern, `internal` Infrastructure implementations) rather than inventing a new
shape for this feature. Scoping the cookie's `Path` to `/api/auth` means it is never attached to
ordinary API calls (which keep using the unchanged `Authorization: Bearer` header for the access
token) — only the two endpoints that actually need it ever see it, which is what shrinks the
CSRF-relevant surface down to those two and lets `SameSite=Strict` fully close it (both origins
are `localhost`, hence same-site with each other, so `Strict` does not break the legitimate
same-site flow).

**Alternatives considered**:
- Session-wide `Path=/` — rejected: needlessly attaches the refresh-token cookie to every API
  request when only two endpoints ever read it, growing the blast radius of any future endpoint
  that logs or proxies request cookies/headers.
- Add double-submit anti-CSRF tokens on top — rejected for now: `SameSite=Strict` plus `Path`
  scoping already removes the realistic CSRF vector for this same-site topology; revisit only if
  the SPA and API are ever served from genuinely different sites (per Simplicity First).
- Return the refresh token in the `AuthResponse` JSON body and let the SPA decide where to store
  it — rejected: reintroduces the "client decides, client can get it wrong" flexibility that
  makes JWT-storage mistakes possible in the first place; an `HttpOnly` cookie is inaccessible to
  JS by server-side construction, not by SPA-code discipline.

## 11. Session persistence and rehydration on the frontend

**Decision**: `entities/session`'s `SessionProvider` gains a mount-time effect that calls the new
`POST /api/auth/refresh` (via the `shared/api` client, now configured with
`withCredentials: true`) before rendering anything that depends on auth state. Session state
gains an explicit `status: 'resolving' | 'authenticated' | 'anonymous'` (replacing today's
implicit "session is `null` ⇒ anonymous"), so `RequireAuth` can show a brief loading state
instead of redirecting to `/login` and then bouncing back once the silent refresh resolves. An
`axios` response interceptor also triggers one silent `/api/auth/refresh` attempt after any other
authenticated call gets a `401`, before surfacing the failure — so a mid-session access-token
expiry (up to the existing 60-minute lifetime, research.md #3) doesn't log the user out while
their refresh token is still valid. Logout calls the new `POST /api/auth/logout` (revoking
server-side, #10) before clearing in-memory session state and navigating to `/login`.

**Rationale**: This is the concrete client-side mechanism that fulfills "persist logged in
session between page refresh" — the access token still cannot survive a refresh by design (#5,
#8), so something has to re-derive it, and the `httpOnly` cookie is exactly what the browser
preserves and resends automatically. A distinct `resolving` state avoids the flash-of-login-page
bug that's common to this pattern if "anonymous" and "haven't checked yet" are conflated.

**Alternatives considered**:
- Rely only on the `401`-triggered interceptor refresh, with no mount-time check — rejected:
  `RequireAuth` would redirect to `/login` immediately on every page load (before any API call
  ever fires to trigger the interceptor), defeating the purpose entirely.
- Poll `/api/auth/refresh` on a timer instead of reacting to an actual `401` — rejected:
  unnecessary server load and complexity for no benefit over reacting to the real signal
  (Simplicity First).
