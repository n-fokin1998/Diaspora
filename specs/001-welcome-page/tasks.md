---

description: "Task list template for feature implementation"
---

# Tasks: Welcome Page

**Input**: Design documents from `/specs/001-welcome-page/`

**Prerequisites**: [plan.md](plan.md), [spec.md](spec.md), [research.md](research.md), [data-model.md](data-model.md) (N/A — no entities), [quickstart.md](quickstart.md)

**Tests**: Included. [plan.md](plan.md) and [research.md](research.md#6-automated-testing-for-the-new-page) commit to introducing Vitest + React Testing Library for this feature per the constitution's Quality by Default principle, so each user story includes a test task.

**Organization**: Tasks are grouped by user story (from [spec.md](spec.md)) to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

This feature is frontend-only, inside the existing SPA project:
`src/Web/Client.Api/spa/`. All paths below are relative to the repository root unless noted.

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Introduce the test tooling this feature needs (none exists yet in `spa`).

- [X] T001 [P] Add `vitest`, `@testing-library/react`, `@testing-library/jest-dom`, and `jsdom` as dev dependencies, and add a `"test": "vitest run"` script, in `src/Web/Client.Api/spa/package.json`
- [X] T002 [P] Create `src/Web/Client.Api/spa/vitest.config.ts` configured with the `jsdom` environment, globals enabled, and `setupFiles` pointing at the new test setup file
- [X] T003 [P] Create `src/Web/Client.Api/spa/src/setupTests.ts` importing `@testing-library/jest-dom` matchers

**Checkpoint**: `npm test` runs (zero tests yet) with no configuration errors.

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Replace the Vite/React starter content with an empty welcome page shell that every
user story renders into.

**⚠️ CRITICAL**: No user story work can begin until this phase is complete.

- [X] T004 Remove the Vite/React starter demo content (counter, React/Vite logos, docs/social
      links) from `src/Web/Client.Api/spa/src/App.tsx` and replace it with a semantic welcome page
      container: a brand/description region and an actions region, both empty for now
- [X] T005 [P] Replace the starter styles in `src/Web/Client.Api/spa/src/App.css` with a base
      modern, simple layout (centered content column, spacing scale, a mobile-width media query
      breakpoint) for the two regions added in T004

**Checkpoint**: The app builds and renders an empty but structurally complete welcome page shell.
No user story is demoable yet — that starts in Phase 3.

---

## Phase 3: User Story 1 - First-time visitor learns what Diaspora offers (Priority: P1) 🎯 MVP

**Goal**: A visitor sees the Diaspora logo and a description of the product's purpose immediately
on landing, with no interaction required.

**Independent Test**: Load the welcome page with no prior session; confirm the logo and product
description are visible without needing sign-in/sign-up or any other feature to exist (per
[spec.md](spec.md#user-story-1---first-time-visitor-learns-what-diaspora-offers-priority-p1)).

### Tests for User Story 1

> **NOTE: Write this test FIRST, ensure it FAILS before implementation**

- [X] T006 [P] [US1] Write a failing test asserting the logo image and the product description
      text are present on render, in `src/Web/Client.Api/spa/src/App.test.tsx`

### Implementation for User Story 1

- [X] T007 [US1] Obtain a freely-licensed (CC0/royalty-free), topic-appropriate logo/hero image
      (people connecting / community in a new city) and add it to
      `src/Web/Client.Api/spa/src/assets/`. **Ask the user for explicit confirmation (filename,
      source, and size) before downloading it** — if declined, author an inline SVG icon of the
      same concept instead (see [research.md §4](research.md#4-logo--hero-imagery)).
      Resolution: user chose the inline-SVG fallback; authored `diaspora-logo.svg` (no download).
- [X] T008 [US1] Render the logo image and a short product description ("helping people find
      friends and activities in a new country or city") in the brand/description region of
      `src/Web/Client.Api/spa/src/App.tsx` (satisfies FR-001, FR-002); depends on T004 and T007
- [X] T009 [P] [US1] Style the brand/description region (logo sizing, description typography) in
      `src/Web/Client.Api/spa/src/App.css`

**Checkpoint**: User Story 1 is fully functional and independently testable/demoable — `npm test`
passes T006, and `npm run dev` shows the logo and description.

---

## Phase 4: User Story 2 - New visitor proceeds to sign up (Priority: P2)

**Goal**: A visitor can see a clearly labeled, selectable sign-up action.

**Independent Test**: Load the welcome page and confirm a distinct, selectable sign-up control is
visible and identifiable, independent of whether a real sign-up destination exists (per
[spec.md](spec.md#user-story-2---new-visitor-proceeds-to-sign-up-priority-p2)).

### Tests for User Story 2

> **NOTE: Write this test FIRST, ensure it FAILS before implementation**

- [X] T010 [P] [US2] Write a failing test asserting a labeled, focusable "Sign Up" button renders
      and has no click handler or navigation attribute wired to it, in
      `src/Web/Client.Api/spa/src/App.test.tsx`

### Implementation for User Story 2

- [X] T011 [US2] Add a non-functional `<button type="button">Sign Up</button>` control (no
      `onClick`, no navigation) to the actions region of
      `src/Web/Client.Api/spa/src/App.tsx` (satisfies FR-004, FR-006); depends on T004
- [X] T012 [P] [US2] Style the sign-up button as the primary call-to-action control in
      `src/Web/Client.Api/spa/src/App.css`

**Checkpoint**: User Stories 1 AND 2 both work independently.

---

## Phase 5: User Story 3 - Returning visitor proceeds to sign in (Priority: P3)

**Goal**: A visitor can see a clearly labeled, selectable sign-in action.

**Independent Test**: Load the welcome page and confirm a distinct, selectable sign-in control is
visible and identifiable, independent of whether a real sign-in destination exists (per
[spec.md](spec.md#user-story-3---returning-visitor-proceeds-to-sign-in-priority-p3)).

### Tests for User Story 3

> **NOTE: Write this test FIRST, ensure it FAILS before implementation**

- [X] T013 [P] [US3] Write a failing test asserting a labeled, focusable "Sign In" button renders
      and has no click handler or navigation attribute wired to it, in
      `src/Web/Client.Api/spa/src/App.test.tsx`

### Implementation for User Story 3

- [X] T014 [US3] Add a non-functional `<button type="button">Sign In</button>` control (no
      `onClick`, no navigation) to the actions region of
      `src/Web/Client.Api/spa/src/App.tsx` (satisfies FR-003, FR-005); depends on T004
- [X] T015 [P] [US3] Style the sign-in button as the secondary call-to-action control in
      `src/Web/Client.Api/spa/src/App.css`

**Checkpoint**: All three user stories are independently functional together on one welcome page.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Validate the page as a whole against the spec's edge cases and success criteria.

- [X] T016 [P] Verify and adjust the responsive layout in
      `src/Web/Client.Api/spa/src/App.css` at a common mobile width (~375px) and a common desktop
      width (~1280px): logo, description, and both actions must remain visible and usable with no
      horizontal scrolling (Edge Cases, FR-008, SC-004)
- [X] T017 Run `npm run lint` in `src/Web/Client.Api/spa` and fix any warnings
- [X] T018 Run `npm run build` in `src/Web/Client.Api/spa` and confirm the production build
      succeeds
- [X] T019 Walk through every scenario in [quickstart.md](quickstart.md) and confirm each one
      passes

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — can start immediately.
- **Foundational (Phase 2)**: Depends on Setup completion — BLOCKS all user stories.
- **User Stories (Phase 3-5)**: All depend on Foundational (Phase 2) completion.
  - US1, US2, and US3 touch the same two files (`App.tsx`, `App.css`) in different regions, so
    treat them as sequential (P1 → P2 → P3) rather than parallelized across stories, to avoid
    merge conflicts within a single contributor's working tree.
- **Polish (Phase 6)**: Depends on all three user stories being complete.

### Within Each User Story

- Test task written and failing before its implementation task.
- Implementation task before its style task is not required (style task only depends on the
  region existing from Phase 2), but functionally verify the button/content it targets exists.
- Story complete (test passing) before moving to the next priority.

### Parallel Opportunities

- T001, T002, T003 (Setup) can run in parallel — different files.
- T005 (Foundational style) can run in parallel with nothing else in Phase 2 (T004 must land
  first to give it real selectors, but the base layout rules can be drafted in parallel and
  reconciled).
- Within each user story phase, the test task ([P]) and the style task ([P]) touch different files
  than the implementation task and than each other, so they can be done in parallel; the
  implementation task should follow the test task (test-first).

---

## Parallel Example: User Story 1

```bash
# Test and asset-sourcing can proceed in parallel; implementation waits on both:
Task: "Write failing test for logo + description render in src/Web/Client.Api/spa/src/App.test.tsx"
Task: "Obtain topic-appropriate logo image (confirm with user before downloading) into src/Web/Client.Api/spa/src/assets/"

# Once T008 (implementation) lands, styling can proceed in parallel with the next story's test:
Task: "Style brand/description region in src/Web/Client.Api/spa/src/App.css"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational (blocks all stories)
3. Complete Phase 3: User Story 1
4. **STOP and VALIDATE**: run `npm test` and the User Story 1 quickstart scenario independently
5. Demo if ready — a visitor can already understand what Diaspora is for

### Incremental Delivery

1. Setup + Foundational → empty page shell
2. Add User Story 1 → test independently → demo (MVP: logo + description)
3. Add User Story 2 → test independently → demo (adds sign-up control)
4. Add User Story 3 → test independently → demo (adds sign-in control)
5. Polish → responsive check, lint, build, full quickstart pass

---

## Notes

- [P] tasks = different files, no ordering dependency
- [Story] label maps each task to its user story for traceability
- This feature has no backend, data model, or API contract work — see
  [research.md §1](research.md#1-feature-surface-frontend-only) and
  [data-model.md](data-model.md)
- Commit after each task or logical group
- Stop at any checkpoint to validate a story independently
- T007's download step requires explicit user confirmation before it happens, per this
  repository's operating rules — do not skip that confirmation even though it is written as a
  single task here
