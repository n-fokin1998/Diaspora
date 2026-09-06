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
- Organize by feature as the app grows beyond the current starter files: a feature owns its
  components, hooks, and types together, instead of splitting everything into global
  `components/`, `hooks/`, `types/` folders. Keep the starter layout until a second feature
  actually needs this structure — don't restructure speculatively.
- Centralize outbound HTTP calls behind a small typed API client module (wrapping the existing
  `axios` dependency) instead of calling `axios.get`/`.post` directly from components.
- Keep components focused on rendering; move non-trivial logic (data fetching, derived state,
  side effects) into custom hooks so it can be reused and tested independently of JSX.
- Prefer colocated component state (`useState`/`useReducer`) over introducing a global state
  library; only reach for one when multiple unrelated components genuinely need to share the
  same state.
- Code must pass `npm run lint` (see [AGENTS.md](../../AGENTS.md)) with no new warnings before a
  change is considered done.
