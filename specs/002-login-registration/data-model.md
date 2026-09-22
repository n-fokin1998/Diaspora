# Phase 1 Data Model: Login and Registration

Derived from spec.md's Key Entities, refined with the decisions recorded in research.md.

## User (`Identity.Domain.Users.User`)

The one persisted entity this feature introduces, owned exclusively by the `Identity` module (see
[architecture.md](../../.claude/rules/architecture.md) — no other module references it or the
`IdentityDbContext` directly).

| Field | Type | Notes |
|---|---|---|
| `Id` | `Guid` | Primary key, generated on creation. |
| `Email` | `string` | As entered by the user (preserves display casing); validated as a well-formed email address before the entity is created. |
| `NormalizedEmail` | `string` | `Email` trimmed and upper-invariant; not user-facing. Backs the unique index used for FR-003's case/whitespace-insensitive uniqueness check. |
| `PasswordHash` | `byte[]` | PBKDF2-HMAC-SHA256 derived key (research.md #2). Never the plain-text password. |
| `PasswordSalt` | `byte[]` | Random 128-bit value generated per user at registration. |
| `PasswordHashIterations` | `int` | Iteration count used when the hash was computed, so it can be raised later without invalidating existing accounts. |
| `FirstName` | `string` | Trimmed, non-empty, bounded length. |
| `LastName` | `string` | Trimmed, non-empty, bounded length. |
| `DateOfBirth` | `DateOnly` | Must not be in the future; must reflect an age at or above the minimum (spec.md Assumptions: 13 years). |
| `Location` | `string` | Free text (spec.md FR-007); non-empty after trimming, bounded length. No structural/geocoding validation. |
| `CreatedAtUtc` | `DateTime` (UTC) | Set once, at creation. |

**Invariants enforced by the entity itself** (defense in depth, in addition to Application-layer
validation that produces the field-specific messages FR-013 requires):
- `Email`, `FirstName`, `LastName`, `Location` are never null/empty/whitespace-only.
- `DateOfBirth` is never in the future.
- `PasswordHash`/`PasswordSalt` are never empty once set; the entity has no way to hold a
  plain-text password (there is no such property).

**Construction**: a single static factory, `User.Register(email, passwordHash, passwordSalt,
passwordHashIterations, firstName, lastName, dateOfBirth, location, createdAtUtc)`, so an invalid
`User` can never exist mid-construction; the Application-layer command handler computes the
hash/salt (via the password-hashing abstraction) and passes the result in. `NormalizedEmail` is not
a caller-supplied argument — the factory computes it internally from `email` (trimmed,
upper-invariant) rather than trusting the caller to have normalized it consistently.

**Relationships**: none yet — this feature introduces no other entity that references `User`.

## Session (conceptual, not persisted)

Per research.md #4/#5, "session" (spec.md's second Key Entity) is not a database row. It is the
signed JWT itself:

| Claim | Meaning |
|---|---|
| `sub` | The authenticated `User.Id`. |
| `email` | The user's email, for convenient client-side display without an extra call. |
| `jti` | Unique token id (unused today; reserved for a future revocation mechanism). |
| `iat` / `nbf` | Issued-at / not-before, both "now" at issuance. |
| `exp` | 60 minutes after issuance (research.md #3). |

No `Sessions` table exists. "Ending a session" (FR-012) is the client discarding this token
(research.md #4); there is nothing server-side to update.

## Validation summary (traces to spec.md Functional Requirements)

| Field | Rule | Spec source |
|---|---|---|
| Email | Well-formed email address format | FR-002 |
| Email | Unique, case/whitespace-insensitive | FR-003 |
| Password | Minimum 8 characters, mix of letters and numbers | FR-004, spec Assumptions |
| Password confirmation | Must match `Password` | FR-004 |
| First name / Last name | Non-empty after trim, bounded max length | FR-005 |
| Date of birth | Real calendar date, not in the future, age ≥ 13 | FR-006, spec Assumptions |
| Location | Non-empty after trim, bounded max length, free text | FR-007 |

Validation is implemented as plain, project-owned checks (research.md #1 — no third-party
validation library) in a dedicated `IValidator<TRequest>` per command (`RegisterCommandValidator`,
`LoginCommandValidator`), run by a shared MediatR pipeline behavior (`ValidationBehavior`) before
the command handler executes — the handlers themselves contain no validation logic. A failed
validation short-circuits the pipeline with a field-name → message map that the API layer turns
into a `ValidationProblemDetails` response (see [contracts/auth-api.md](contracts/auth-api.md)).
