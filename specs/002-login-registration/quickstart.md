# Quickstart: Login and Registration

Manual/local validation steps for this feature. Assumes the repository is already cloned and
.NET 9 SDK / Node.js are installed.

## 1. Start local infrastructure

```bash
docker compose -f infrastructure/docker/docker-compose.yml up -d
```

Starts PostgreSQL (and pgAdmin) as documented in [AGENTS.md](../../AGENTS.md).

## 2. Apply the Identity module's database migration

From `src/Modules/Identity/Identity.Infrastructure`:

```bash
dotnet ef database update --project src/Modules/Identity/Identity.Infrastructure --startup-project src/Client.Api
```

Creates the `users` and `refresh_tokens` tables described in [data-model.md](data-model.md).

## 3. Run the backend

```bash
dotnet run --project src/Client.Api
```

Confirm the endpoints in [contracts/auth-api.md](contracts/auth-api.md) are reachable, e.g.:

```bash
curl -X POST http://localhost:5220/api/auth/register -H "Content-Type: application/json" -d "{\"email\":\"jane@example.com\",\"password\":\"Str0ngPass1\",\"confirmPassword\":\"Str0ngPass1\",\"firstName\":\"Jane\",\"lastName\":\"Doe\",\"dateOfBirth\":\"1998-04-12\",\"location\":\"Berlin\"}"
```

Expect `201 Created` with an `AuthResponse` body on first run, `409 Conflict` if run again with
the same email.

## 4. Run the frontend

From `src/Client.Api/spa`:

```bash
npm install
npm run dev
```

## 5. Validate the end-to-end scenarios (spec.md Acceptance Scenarios)

Open the SPA (default `http://localhost:5173`) and confirm:

1. **Register (User Story 1)**: From the welcome page, choose Sign Up, fill in all fields with
   valid values, submit — land on the (empty) Home screen, authenticated.
2. **Duplicate email (User Story 1)**: Repeat step 1 with the same email — see a clear "email
   already in use" error; no navigation away from the form.
3. **Password mismatch (User Story 1)**: Enter a password and a different confirmation — see a
   clear mismatch error before any request succeeds.
4. **Login (User Story 2)**: From the welcome page, choose Sign In with the credentials from step
   1 — land on the Home screen, authenticated.
5. **Wrong password (User Story 2)**: Sign in with the right email and a wrong password — see a
   single generic invalid-credentials message (never one that reveals which field was wrong).
6. **Logout (User Story 2)**: From the Home screen, log out — confirm the app returns to an
   unauthenticated state (e.g., navigating back to Home redirects to Sign In).
7. **Field-level feedback (User Story 3)**: On the registration form, try a malformed email, a
   too-short password, an empty first/last name, and a future date of birth, one at a time —
   confirm each produces a specific, field-relevant message rather than a generic failure.
8. **Session persists across refresh** (new, 2026-09-22 scope addition — spec.md SC-006): While
   authenticated on the Home screen, refresh the browser tab (F5). Confirm you remain
   authenticated with no visible re-login step. Open your browser's DevTools → Application →
   Cookies for `http://localhost:5220` and confirm a `refreshToken` cookie is present with
   `HttpOnly` checked and no value visible to a `document.cookie` read from the DevTools console.
9. **Refresh token rotates**: Note the `refreshToken` cookie's value, then trigger a refresh
   (reload the page, or wait for the SPA's silent refresh). Confirm the cookie's value has
   changed — the old value is no longer accepted (see step 10).
10. **Logout revokes the session server-side** (spec.md FR-016): Log out, then attempt to call
    `POST /api/auth/refresh` directly with the refresh-token cookie captured before logout (e.g.
    via `curl -b cookies.txt ...` using a cookie jar saved before logging out). Confirm it now
    returns `401 Unauthorized` — the server, not just the SPA, has ended the session.
11. **Reused refresh token is rejected** (spec.md FR-015): Save two copies of the refresh-token
    cookie before and after a rotation (step 9). Present the older, already-rotated value to
    `POST /api/auth/refresh`. Confirm it returns `401 Unauthorized`, and that the newer
    (legitimately current) refresh token is now also rejected — reuse of a superseded token
    revokes the whole session, not just the reused token.

## 6. Automated checks

```bash
dotnet test src/Diaspora.sln
```

```bash
npm run lint
npm run test
```

(from `src/Client.Api/spa` for the two frontend commands) — see
[testing.md](../../.claude/rules/testing.md) for the testing conventions these checks follow.
