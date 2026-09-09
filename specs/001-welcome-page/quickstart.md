# Quickstart: Welcome Page

Validates the welcome page feature end-to-end in a local dev environment. No backend or database
is required for this feature (see [research.md](research.md) §1).

## Prerequisites

- Node.js (matching `spa/package.json` toolchain) and npm installed.
- Dependencies installed: `npm install` run from `src/Web/Client.Api/spa`.

## Run

```bash
npm run dev
```

(from `src/Web/Client.Api/spa`) then open the printed local URL (default
`http://localhost:5173`).

## Manual validation scenarios

Maps to the spec's acceptance scenarios — see [spec.md](spec.md#user-scenarios--testing).

1. **Logo and description visible (User Story 1)**
   - Load the site root with no prior session (fresh browser tab / private window).
   - Confirm the Diaspora logo and a description of the product's purpose (finding friends and
     activities in a new country/city) are visible without any interaction.

2. **Sign-up action present (User Story 2)**
   - On the welcome page, confirm a clearly labeled sign-up control is visible and focusable
     (e.g., tab to it with the keyboard).
   - Selecting it does not need to navigate anywhere in this iteration (deferred; see FR-006).

3. **Sign-in action present (User Story 3)**
   - On the welcome page, confirm a clearly labeled sign-in control is visible and focusable.
   - Selecting it does not need to navigate anywhere in this iteration (deferred; see FR-005).

4. **Responsive layout (Edge Cases)**
   - Resize the browser (or use devtools device emulation) to a common mobile width (e.g. 375px)
     and a common desktop width (e.g. 1280px).
   - Confirm the logo, description, and both actions remain visible and usable without horizontal
     scrolling at both widths.

## Automated checks

Run from `src/Web/Client.Api/spa`:

```bash
npm run lint
npm run build
npm test
```

`npm test` (Vitest) exercises the welcome page component tests introduced with this feature (see
[research.md](research.md#6-automated-testing-for-the-new-page)).

## Expected outcome

All manual scenarios above pass, `npm run lint` and `npm run build` succeed with no new warnings
or errors, and the Vitest suite passes.
