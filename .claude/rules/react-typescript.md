---
paths:
  - "**/*.ts"
  - "**/*.tsx"
---

# React / TypeScript Rules

Conventions for the `spa` frontend (React + TypeScript on Vite).

- Function components and hooks only; no class components.
- No `any`. Give props, state, and API response shapes explicit `interface`/`type` definitions.
  Prefer inferring types from a single source of truth (e.g. a shared response type) over
  redefining the same shape in multiple places.
- Organize using **Feature-Sliced Design (FSD)**, instead of splitting everything into global
  `components/`, `hooks/`, `types/` folders. Layers, outermost to innermost: `app` (providers,
  global setup, routing — e.g. `app/AppRouter.tsx`, `app/RequireAuth.tsx`) → `pages` (route-level
  compositions, one folder per route, e.g. `pages/login/LoginPage.tsx`) → `widgets` (composite,
  reusable UI blocks) → `features` (a single user action, e.g. `features/login-user`,
  `features/register-user`) → `entities` (a business object and its own state/UI, e.g.
  `entities/session`) → `shared` (generic UI kit, the typed API client, utilities — no business
  logic). A module/slice may only import from its own layer or a layer strictly below it, never
  sideways or upward (e.g. a `feature` may import `entities`/`shared` but not another `feature`,
  and nothing may import from `app`). Only add a layer folder (e.g. `widgets`) once a real piece
  of UI needs it — don't scaffold it speculatively.
- Each slice (a folder under `features`/`entities`/etc.) exposes its public surface through a
  single `index.ts` barrel (e.g. `features/login-user/index.ts` re-exporting `LoginForm`) —
  other layers import from the slice's `index.ts`, not from its internal files directly.
- A `features`/`entities` slice that talks to the backend keeps its own `api.ts` (thin wrapper
  functions over the shared HTTP client, one per endpoint it needs) rather than reaching into
  another slice's `api.ts` or calling the shared client's methods directly from a component/hook.
- Centralize outbound HTTP calls behind the small typed API client in `shared/api` (wrapping the
  existing `axios` dependency, with the bearer token/interceptor logic in one place — see
  `shared/api/httpClient.ts`) instead of calling `axios.get`/`.post` directly from a slice's
  `api.ts` or components.
- Keep components focused on rendering; move non-trivial logic (data fetching, derived state,
  side effects) into a custom hook named after the use case (e.g. `features/login-user` exposes a
  `useLoginUser()` hook returning `{ submit, error, isSubmitting }`) so it can be reused and
  tested independently of JSX.
- Prefer colocated component state (`useState`/`useReducer`) over introducing a global state
  library. Cross-cutting state that many unrelated components need (e.g. the current session)
  goes through a dedicated `entities/<name>` slice exposing React Context plus a `useX()` hook
  (see `entities/session`) rather than a general-purpose state-management library.
- Code must pass `npm run lint` (see [AGENTS.md](../../AGENTS.md)) with no new warnings before a
  change is considered done.
