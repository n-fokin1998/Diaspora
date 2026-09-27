# API Contract: Auth endpoints

Transport lives in `Client.Api/Controllers/Auth/` (see
[architecture.md](../../../.claude/rules/architecture.md) — controllers stay outside the
`Identity` module). All four endpoints below are unauthenticated (`[AllowAnonymous]`) — `refresh`
and `logout` read a cookie rather than an `Authorization` header, so neither can require a valid
access token to invoke. Every other endpoint this feature does not introduce is unaffected.

Conventions follow [aspnetcore-webapi.md](../../../.claude/rules/aspnetcore-webapi.md): explicit
request/response DTOs, `ProblemDetails`/`ValidationProblemDetails` for errors, standard status
codes.

**Refresh-token cookie** (set by `register`, `login`, and `refresh`; cleared by `logout`; see
[research.md](../research.md) #8-#10 for the full rationale):

| Attribute | Value |
|---|---|
| Name | `refreshToken` |
| `HttpOnly` | `true` — never readable by page JavaScript |
| `Secure` | `true` |
| `SameSite` | `Strict` |
| `Path` | `/api/auth` — attached only to these four endpoints, never to ordinary API calls |
| `Expires` | The issued refresh token's own expiry (`Jwt:RefreshTokenDays`, default 14 days from issuance) |

The SPA's `axios` client must send `withCredentials: true` on calls to these endpoints for the
cookie to be attached cross-origin.

## `POST /api/auth/register`

Registers a new account and immediately authenticates it (FR-001, FR-008; "after register ...
navigate to the Home screen"). On `201`, also sets the refresh-token cookie described above.

**Request body**:

```json
{
  "email": "jane.doe@example.com",
  "password": "Str0ngPass",
  "confirmPassword": "Str0ngPass",
  "firstName": "Jane",
  "lastName": "Doe",
  "dateOfBirth": "1998-04-12",
  "location": "Berlin, Germany"
}
```

**Responses**:

| Status | When | Body |
|---|---|---|
| `201 Created` | Account created | `AuthResponse` (below) |
| `400 Bad Request` | One or more fields fail validation (FR-002, FR-004, FR-005, FR-006, FR-007) | `ValidationProblemDetails` with an entry per invalid field, e.g. `{"errors": {"email": ["Enter a valid email address."]}}` |
| `409 Conflict` | Email already registered (FR-003) | `ProblemDetails` with a message indicating the email is already in use |

## `POST /api/auth/login`

Authenticates an existing account (FR-010). On `200`, also sets the refresh-token cookie
described above.

**Request body**:

```json
{
  "email": "jane.doe@example.com",
  "password": "Str0ngPass"
}
```

**Responses**:

| Status | When | Body |
|---|---|---|
| `200 OK` | Credentials correct | `AuthResponse` (below) |
| `400 Bad Request` | Email or password missing/blank (FR-013) | `ValidationProblemDetails` |
| `401 Unauthorized` | Email/password combination incorrect — including a malformed or unregistered email (FR-011) | `ProblemDetails` with a single generic "invalid email or password" message — never identifies which field was wrong, and never distinguishes a malformed email from a wrong password |

Login does not validate email format: only presence (non-empty) is checked at `400`. A
non-empty-but-malformed email, or one with no matching account, both fall through to the `401`
generic-invalid-credentials path — this avoids ever revealing whether an email is registered or
merely mistyped.

## `POST /api/auth/refresh`

Exchanges the refresh-token cookie for a new access token, and rotates the refresh token itself
(research.md #9). Persists the session across a page refresh/browser restart (spec.md FR-012).
Takes no request body — the input is entirely the `refreshToken` cookie.

**Responses**:

| Status | When | Body |
|---|---|---|
| `200 OK` | Cookie present and its token is active (not expired, not already revoked) | `AuthResponse` (below); also rotates the refresh-token cookie to a new value |
| `401 Unauthorized` | Cookie missing, expired, or unknown | `ProblemDetails`, generic "session expired, please log in again" message |
| `401 Unauthorized` | Cookie's token is **already revoked** (reuse of a superseded token — FR-015) | Same generic message as above (never reveals that reuse specifically was detected); as a side effect, every other active refresh token for that user is also revoked, ending all of that user's sessions |

A `401` from this endpoint also clears the (now-useless) `refreshToken` cookie, so the client
does not keep resending it.

## `POST /api/auth/logout`

Ends the session server-side (FR-016): revokes the refresh token identified by the
`refreshToken` cookie and clears the cookie. Idempotent — logging out with a missing, already-
expired, or already-revoked cookie still succeeds, since the end state (no active session) is
the same either way.

**Responses**:

| Status | When | Body |
|---|---|---|
| `204 No Content` | Always (idempotent) | none |

## `AuthResponse` (shared response shape)

```json
{
  "accessToken": "<JWT>",
  "expiresAtUtc": "2026-09-15T13:00:00Z",
  "user": {
    "id": "b6e6c6d0-...",
    "email": "jane.doe@example.com",
    "firstName": "Jane",
    "lastName": "Doe"
  }
}
```

- `accessToken`: signed JWT per research.md #3; the SPA holds it in memory only (research.md
  #5/#8) and sends it back as `Authorization: Bearer <accessToken>` on subsequent requests.
- `expiresAtUtc`: lets the client know when it should treat the token as stale without decoding it.
- The refresh token itself is never present in this (or any) JSON response body — it only ever
  travels as the `HttpOnly` `refreshToken` cookie (research.md #10), so page JavaScript can never
  read it.

## Logout

See `POST /api/auth/logout` above. Per research.md #9/#10 (superseding the original research.md
#4, which had no persisted session to invalidate), logging out (FR-016) is now a server-side
action — the presented refresh token is revoked in the database — in addition to the SPA
discarding its in-memory access token and returning to an unauthenticated state.
