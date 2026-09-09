# Implementation Plan: Welcome Page

**Branch**: `001-welcome-page` | **Date**: 2026-09-09 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `/specs/001-welcome-page/spec.md`

**Note**: This template is filled in by the `/speckit-plan` command; its definition describes the execution workflow.

## Summary

Add a static welcome page to the existing `spa` React/Vite frontend, replacing the current Vite
starter content in `App.tsx`. The page displays the Diaspora logo, a short description of the
product's purpose, and clearly presented (but not yet functional) sign-in and sign-up actions,
per the updated spec. No backend changes are required — the page is fully static, plain CSS is
used for a modern, simple, responsive layout, and Vitest + React Testing Library are introduced
for the first time to cover it.

## Technical Context

**Language/Version**: TypeScript ~5.x (per `spa/package.json`), React 19, built with Vite 8.
Backend (.NET 9 / ASP.NET Core, `Client.Api`) exists but is untouched by this feature.

**Primary Dependencies**: Existing `spa` toolchain only (`react`, `react-dom`, `vite`,
`@vitejs/plugin-react`). New dev dependencies: `vitest`, `@testing-library/react`,
`@testing-library/jest-dom`, `jsdom` (test tooling only, per [testing.md](../../.claude/rules/testing.md)).

**Storage**: N/A — no persisted or fetched data.

**Testing**: Vitest + React Testing Library (first introduction for the `spa` project), run via
`npm test`.

**Target Platform**: Web browser, desktop and mobile viewport widths.

**Project Type**: Web application — this feature is frontend-only within the existing
`src/Web/Client.Api/spa` project; the backend project is unaffected.

**Performance Goals**: No specific new performance target beyond a normally responsive static
page load (see spec SC-003 for the technology-agnostic version of this goal).

**Constraints**: No client-side routing library introduced; sign-in/sign-up controls MUST NOT
have navigation logic wired up in this feature (deferred; see spec FR-005/FR-006). Layout MUST
remain usable at common desktop and mobile widths without horizontal scrolling.

**Scale/Scope**: Single page component (plus supporting styles and image asset), no new routes,
no new backend surface.

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Check | Result |
|---|---|---|
| I. Learning-Oriented Engineering | Introduces first frontend test setup (Vitest/RTL) as a deliberate practice of the testing convention, not just to ship fast. | PASS |
| II. Incremental Delivery | Single vertical slice: one page, independently deployable/demonstrable; app remains working (existing controller/build untouched). | PASS |
| III. Simplicity First | No router, no CSS framework, no state library added; only test tooling added because Quality by Default requires it. | PASS |
| IV. Modular Architecture | Frontend has no module boundaries yet (single starter app) — adding one page component does not require inventing module structure prematurely, per [react-typescript.md](../../.claude/rules/react-typescript.md). | PASS |
| V. Quality by Default | Vitest + RTL introduced; component tests added for the new page; `npm run lint` and `npm run build` must stay clean. | PASS (addressed in this plan) |
| VI. Specification-Driven Development | Spec updated during planning (FR-005/006, related scenarios and success criteria) to match the agreed "no navigation logic yet" scope before implementation proceeds. | PASS |
| VII. Agent-Assisted Development | No new architectural decision, module boundary, or external dependency beyond dev-only test tooling (routine, not requiring separate sign-off). | PASS |
| VIII. Local-First and Reproducible | Runs entirely via `npm run dev` / `npm test`; no cloud or shared dependency. | PASS |
| IX. Document Important Decisions | No significant architectural/technology decision beyond adopting the already-planned Vitest/RTL convention; no ADR needed. | PASS |

No violations — Complexity Tracking table is not needed.

## Project Structure

### Documentation (this feature)

```text
specs/001-welcome-page/
├── plan.md              # This file (/speckit-plan command output)
├── research.md          # Phase 0 output (/speckit-plan command)
├── data-model.md        # Phase 1 output (/speckit-plan command) — N/A, no entities
├── quickstart.md        # Phase 1 output (/speckit-plan command)
└── tasks.md             # Phase 2 output (/speckit-tasks command - NOT created by /speckit-plan)
```

No `contracts/` directory is produced: this feature exposes no API, CLI, or other external
interface (see [research.md](research.md#1-feature-surface-frontend-only)).

### Source Code (repository root)

```text
src/Web/
├── Diaspora.sln
└── Client.Api/                       # ASP.NET Core Web API project — unaffected by this feature
    └── spa/                          # React + TypeScript frontend (Vite)
        ├── src/
        │   ├── App.tsx               # Replaced with the welcome page composition
        │   ├── App.css               # Extended with welcome page styles
        │   ├── main.tsx              # Unchanged
        │   └── assets/
        │       └── ...               # New logo/hero image asset(s) added here
        ├── public/                   # Unchanged (favicon.svg, icons.svg)
        ├── vitest.config.ts          # New: Vitest configuration (or merged into vite.config.ts)
        └── src/
            └── App.test.tsx          # New: component tests for the welcome page
```

**Structure Decision**: This feature stays entirely inside the existing single frontend project at
`src/Web/Client.Api/spa`. Per [react-typescript.md](../../.claude/rules/react-typescript.md), the
starter's flat `src/` layout is kept as-is (no feature-folder restructuring) since this is still
the first real feature added to the SPA. The backend project (`Client.Api`) and its `Controllers/`
are untouched — this feature has no server-side surface.

## Complexity Tracking

*No entries — no Constitution Check violations to justify.*
