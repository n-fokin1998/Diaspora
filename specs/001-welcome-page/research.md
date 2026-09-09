# Phase 0 Research: Welcome Page

## 1. Feature surface: frontend-only

**Decision**: This feature is implemented entirely in the existing `spa` React/Vite frontend. No
backend (`Client.Api`) code changes are required.

**Rationale**: The welcome page's content (logo, description) is static and does not depend on
server-side data. Per the updated spec, the sign-in/sign-up controls are visual-only in this
iteration (no real navigation), so no route or endpoint needs to exist yet on either side.

**Alternatives considered**: Serving welcome-page copy from a backend endpoint — rejected per the
constitution's Simplicity First principle; there is no concrete need for server-rendered or
API-delivered content for static, unauthenticated marketing copy.

## 2. No client-side routing library yet

**Decision**: Do not introduce a routing library (e.g., `react-router`) for this feature.

**Rationale**: Sign-in/sign-up actions are explicitly non-functional in this iteration (per plan
input: "without navigation logic. It will be implemented later"). Adding a router now would be
speculative infrastructure with no current consumer.

**Alternatives considered**: Installing `react-router` now and pre-wiring placeholder routes —
rejected per Simplicity First; the dependency should be introduced together with the feature that
actually needs navigation.

## 3. Sign-in / sign-up controls: visual-only elements

**Decision**: Render the sign-in and sign-up actions as `<button type="button">` elements styled
as primary/secondary call-to-action controls, with no `onClick` navigation logic attached.

**Rationale**: A native `<button>` is focusable and keyboard-operable out of the box (satisfying
FR-005/FR-006's "distinct, selectable element" requirement and accessibility expectations),
without implying a real destination the way a bare `<a>` without an `href` would (which is also
not keyboard-focusable). This keeps the placeholder honest about not yet being wired up, while
still being a legitimate, testable UI element.

**Alternatives considered**: `<a href="#">` placeholder links — rejected, `href="#"` triggers a
scroll-to-top on click and is commonly flagged as an anti-pattern for non-functional links.

## 4. Logo / hero imagery

**Decision**: Use a freely-licensed (CC0 / royalty-free) illustration or icon that visually
represents the product's theme (people connecting / community in a new city), sourced from the
internet as requested, in place of the current Vite/React starter images.

**Rationale**: Matches the explicit instruction to use topic-appropriate imagery from the internet
for the logo rather than authoring original artwork.

**Process note**: Per this repository's operating rules, downloading any file requires explicit
user confirmation (filename, source, and size) before the download happens. That confirmation will
be requested during implementation, immediately before fetching the chosen image(s), rather than
assumed here. If the user declines, the fallback is an inline, hand-authored SVG icon using the
same visual concept, requiring no download.

**Alternatives considered**: Hand-authored inline SVG from the start — kept as the documented
fallback rather than the primary approach, since the user explicitly asked for internet-sourced
images.

## 5. Styling approach

**Decision**: Plain CSS (extending the existing `App.css` / `index.css` pattern already in the
`spa` project), with a mobile-width media query for responsiveness. No CSS framework or
component library is added.

**Rationale**: Simplicity First — the project already styles with plain CSS; adding a framework
(Tailwind, CSS-in-JS, a component kit) for a single static page is unjustified.

**Alternatives considered**: Tailwind CDN (allowed for artifacts elsewhere but not applicable to
this project's own build) or a component library — rejected as an unnecessary dependency for the
current, known requirement.

## 6. Automated testing for the new page

**Decision**: Introduce Vitest + React Testing Library for the `spa` project, per
[testing.md](../../.claude/rules/testing.md), and add the first tests against the new welcome
page component.

**Rationale**: The constitution's Quality by Default principle requires automated tests for any
behavior-changing feature; this is the first `spa` feature to add real UI logic, so the test
tooling is introduced now, not speculatively.

**Alternatives considered**: Deferring test setup to a later feature — rejected; it would leave
this feature without the coverage Quality by Default requires.
