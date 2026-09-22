# Feature Specification: Login and Registration

**Feature Branch**: `002-login-registration`

**Created**: 2026-09-15

**Status**: Draft

**Input**: User description: "Implement login and registration functionality. During register, user should be able to specify email, password, first name, last name, DOB, Location. Do not use any third-party libs for now. Email should be just stored in DB without integration with SMTP. Location also should be just string. Apply regular validation for these fields, that is used in general for such data according to best practices. Apply styling to follow modern web design trends, but not too complex."

## Clarifications

### Session 2026-09-22

- Q: For first/last name, should leading/trailing whitespace be rejected as invalid, or trimmed and accepted? → A: Trimmed and accepted — not treated as invalid.
- Q: Should a malformed (but non-empty) login email produce a field-specific 400 validation error, or the same generic invalid-credentials response as a wrong password or unknown email? → A: The same generic invalid-credentials response; login does not perform email-format validation before checking credentials.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Register for a new account (Priority: P1)

A new visitor creates an account by providing their email, a password, first name, last name,
date of birth, and location, so they can access the parts of the product that require an
identity.

**Why this priority**: Without registration, no one can create an account at all, so this is the
foundation every other identity-related capability depends on. It is independently shippable and
delivers value on its own (an account exists after this story, even before login is built).

**Independent Test**: Can be fully tested by submitting the registration form with valid values
for every field and confirming a new account is created and persisted, without needing login to
exist yet.

**Acceptance Scenarios**:

1. **Given** a visitor on the registration page, **When** they submit a valid email, matching
   password/confirmation, first name, last name, date of birth, and location, **Then** a new
   account is created and the visitor is informed registration succeeded.
2. **Given** a visitor filling out the registration form, **When** they submit an email address
   that is already registered, **Then** registration is rejected and they are told the email is
   already in use.
3. **Given** a visitor filling out the registration form, **When** they submit a password and
   confirmation that do not match, **Then** registration is rejected and they are told the
   passwords do not match.

---

### User Story 2 - Log in to an existing account (Priority: P2)

A returning user with a registered account enters their email and password to authenticate and
gain access to their account.

**Why this priority**: Registration alone has no ongoing value without a way to come back and use
the account; login is the second half of the minimum usable identity flow.

**Independent Test**: Can be fully tested, given an account already exists (created directly or
via User Story 1), by submitting correct credentials on the login page and confirming an
authenticated session starts.

**Acceptance Scenarios**:

1. **Given** a registered account, **When** the user submits the correct email and password,
   **Then** they are authenticated and gain access to the account.
2. **Given** a registered account, **When** the user submits an incorrect password for that
   email, **Then** login is rejected with a generic invalid-credentials message.
3. **Given** an authenticated user, **When** they choose to log out, **Then** their session ends
   and they are treated as unauthenticated on subsequent access.

---

### User Story 3 - Understand and correct invalid input (Priority: P3)

A visitor or user who makes a mistake filling out the registration or login form (a malformed
email, a too-weak password, a missing field, an impossible date of birth) is told specifically
what is wrong so they can correct it without guesswork.

**Why this priority**: This is a quality-of-experience layer on top of Stories 1 and 2 — the core
flows work without it, but clear validation feedback is what makes them pleasant and trustworthy
to use, and prevents bad data from ever reaching storage.

**Independent Test**: Can be fully tested by submitting the registration or login form with each
class of invalid input (bad email format, weak password, empty required field, invalid date of
birth) one at a time and confirming a specific, field-relevant error is shown each time.

**Acceptance Scenarios**:

1. **Given** the registration form, **When** the visitor enters an email address that is not in a
   valid format, **Then** they see an error identifying the email as invalid before/without the
   account being created.
2. **Given** the registration form, **When** the visitor enters a password that does not meet the
   minimum strength requirement, **Then** they see an error explaining the requirement.
3. **Given** the registration form, **When** the visitor leaves a required field empty or enters a
   date of birth that is in the future or implies an unreasonably young age, **Then** they see an
   error specific to that field.

---

### Edge Cases

- What happens when someone submits the registration form with a date of birth in the future, or
  one implying an age below the minimum allowed?
- What happens when someone tries to register with an email address that only differs from an
  existing account by letter case or surrounding whitespace?
- What happens when someone submits a location value that is extremely long or contains only
  whitespace?
- What happens when someone repeatedly submits incorrect login credentials for the same account?
- What happens when someone tries to log in with an email that has no registered account?
- What happens when someone submits a malformed (not validly formatted) email address on the login
  form? (Treated the same as any other incorrect credential — the generic invalid-credentials
  message, not a format-specific error, since login does not validate email format before checking
  credentials.)
- What happens when an already-authenticated user opens the registration or login page again?

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST allow a new visitor to register by providing email, password, password
  confirmation, first name, last name, date of birth, and location.
- **FR-002**: System MUST validate that the submitted email is in a valid email address format
  before accepting registration.
- **FR-003**: System MUST treat each email address as a unique account identifier and MUST reject
  registration when the submitted email is already registered (comparison ignoring letter case and
  surrounding whitespace).
- **FR-004**: System MUST validate that the password meets a minimum strength requirement (at
  least a minimum length and a mix of character types) before accepting registration, and MUST
  reject registration when the password and its confirmation do not match.
- **FR-005**: System MUST validate that first name and last name are non-empty (after trimming
  surrounding whitespace) and do not exceed a reasonable maximum length; leading and trailing
  whitespace is trimmed rather than treated as an invalid value.
- **FR-006**: System MUST validate that the date of birth is a real calendar date, is not in the
  future, and reflects an age at or above a minimum allowed age.
- **FR-007**: System MUST accept location as free-form text without validating it against any
  real-world location data or fixed format, while rejecting an empty/whitespace-only value and
  enforcing a reasonable maximum length.
- **FR-008**: System MUST persist every successfully registered account, storing the password only
  in a securely hashed form and never in plain, reversible text.
- **FR-009**: System MUST NOT send any email communication (e.g., verification, welcome, or
  notification messages) as part of registration or login; the stored email is used only to
  identify the account.
- **FR-010**: System MUST allow a user with a registered account to log in by submitting their
  email and password.
- **FR-011**: System MUST reject a login attempt when the email/password combination is incorrect,
  using a single generic message that does not reveal whether the email or the password was the
  incorrect part. This generic rejection applies uniformly to a missing, malformed, or unregistered
  email and to a correct email with the wrong password — login does not perform email-format
  validation separately from the credential check, so a malformed email is never distinguished from
  a wrong password in the response.
- **FR-012**: System MUST establish an authenticated session for a user upon successful login, and
  MUST allow the user to explicitly log out, ending that session.
- **FR-013**: System MUST present specific, field-level validation error messages on the
  registration and login forms whenever submitted input fails validation, rather than a single
  generic failure message. On the login form this applies to a missing email or password; a
  malformed email is not treated as a separate validation failure and instead falls under FR-011's
  generic invalid-credentials response.
- **FR-014**: Registration and login MUST be implemented entirely with the project's own logic,
  without depending on any third-party authentication, identity, or user-management library or
  hosted service.

### Key Entities

- **User Account**: A registered person's identity in the system. Attributes: email (unique,
  used to log in), securely hashed password, first name, last name, date of birth, location (free
  text), and the time the account was created.
- **Session**: An authenticated login instance tied to a single user account, created on
  successful login and ended on logout or expiration; used to recognize the user on subsequent
  access without asking them to log in again.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: A new user can go from opening the registration page to having a created account in
  under 2 minutes.
- **SC-002**: A returning user can go from opening the login page to being authenticated in under
  15 seconds when their credentials are correct.
- **SC-003**: 100% of registration or login submissions with invalid input (malformed email, weak
  password, mismatched confirmation, empty required field, invalid date of birth, wrong
  credentials) result in a specific, on-screen explanation of what to fix — none fail silently or
  with only a generic error.
- **SC-004**: At no point does any stored or logged data contain a user's password in plain,
  readable form.
- **SC-005**: The registration and login pages remain fully usable and visually coherent at both
  common desktop and mobile screen widths.

## Assumptions

- No third-party authentication, identity, or user-management libraries or hosted services are
  used, per explicit instruction; password hashing and session handling are implemented as
  project-owned logic.
- The stored email is account data only — no outbound email capability (verification, password
  reset, notifications) exists yet, so no such flow is in scope.
- Location is a free-text field only; no geocoding, autocomplete, or validation against
  real-world place data is performed.
- Minimum registration age is 13 years, a common baseline for online account creation; no upper
  age bound is enforced.
- Minimum password strength follows common best-practice guidance: at least 8 characters,
  including a mix of letters and numbers; no forced periodic password rotation is included.
- "Login" establishes a session-based authenticated state for the browser; "remember me" /
  long-lived persistent login and password-reset/forgot-password flows are out of scope for this
  feature.
- No social login/SSO is included; the only way to authenticate is the email/password pair
  created at registration.
- Visual design follows clean, contemporary web UI conventions (clear typography, generous
  spacing, a simple and consistent layout) without introducing a complex design system, animation
  framework, or component library.
