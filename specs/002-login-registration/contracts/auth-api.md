# API Contract: Auth endpoints

Transport lives in `Client.Api/Controllers/Auth/` (see
[architecture.md](../../../.claude/rules/architecture.md) — controllers stay outside the
`Identity` module). Both endpoints are unauthenticated (`[AllowAnonymous]`); every other endpoint
this feature does not introduce is unaffected.

Conventions follow [aspnetcore-webapi.md](../../../.claude/rules/aspnetcore-webapi.md): explicit
request/response DTOs, `ProblemDetails`/`ValidationProblemDetails` for errors, standard status
codes.

## `POST /api/auth/register`

Registers a new account and immediately authenticates it (FR-001, FR-008; "after register ...
navigate to the Home screen").

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

Authenticates an existing account (FR-010).

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

- `accessToken`: signed JWT per research.md #3; the SPA holds it in memory only (research.md #5)
  and sends it back as `Authorization: Bearer <accessToken>` on subsequent requests.
- `expiresAtUtc`: lets the client know when it should treat the token as stale without decoding it.

## Logout

There is no `POST /api/auth/logout` endpoint. Per research.md #4, logging out (FR-012) is a
client-side action — the SPA discards the in-memory access token and returns to an
unauthenticated state; there is no server-side session record to invalidate.
