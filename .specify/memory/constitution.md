<!--
Sync Impact Report
- Version change: 1.1.0 → 1.2.0
- Modified principles:
  - VI. Specification-Driven Development — specifications and plans MUST also be consistent with
    the applicable per-technology conventions in .claude/rules/, not only with this constitution
  - VII. Agent-Assisted Development — agents MUST work from .claude/rules/ in addition to
    specifications and this constitution, explicitly including during Spec Kit's research,
    planning, and implementation phases
- Added sections: none
- Removed sections: none
- Deferred placeholders: none
- Templates requiring follow-up: none checked in this run (constitution-only change per
  command scope guard); re-validate plan/spec/tasks templates against these principles the next
  time they are touched.

Sync Impact Report (previous amendment)
- Version change: 1.0.0 → 1.1.0
- Modified principles:
  - IV. Modular Architecture — expanded to require that backend modules be designed and coded
    following microservices architecture patterns/conventions (self-contained domain,
    application, and persistence boundary per module) while remaining a single monolith
    deployable, with the HTTP/API transport layer kept outside each module's boundary
- Concrete conventions implementing that expanded principle — per-module project layout,
  lightweight CQRS/Mediator usage, and Feature-Sliced Design for the SPA — were added to
  AGENTS.md and .claude/rules/ in the same change.
-->

# Diaspora Constitution

## Core Principles

### I. Learning-Oriented Engineering
This project exists to practice modern software engineering and agentic development, not to
maximize shipped features. Every non-trivial technical decision MUST be justifiable by what it
teaches or how it improves engineering practice, in addition to what it delivers. When a faster,
less-instructive path and a more instructive, sound-engineering path both satisfy the
requirement, the project SHOULD prefer the latter.

**Rationale**: Feature velocity is not the goal of a personal learning project; optimizing for
it would defeat the project's purpose.

### II. Incremental Delivery
Development MUST proceed in small, vertical slices that each deliver observable, end-to-end
behavior. The system MUST remain in a working, deployable state after every merged change.
Partially wired features, long-lived feature branches, and "big bang" rewrites MUST NOT be
merged as complete.

**Rationale**: Small slices keep feedback fast and risk low, and avoid the common failure mode
of personal projects stalling on large, unfinished changes.

### III. Simplicity First
The simplest design that satisfies the current, known requirement MUST be chosen. Abstractions,
design patterns, and new dependencies MUST be introduced only in response to a concrete, present
need, never in anticipation of hypothetical future requirements. Adopting any new technology
MUST be tied to a stated engineering or learning purpose, not novelty for its own sake.

**Rationale**: Premature abstraction and speculative technology adoption are direct threats to a
long-lived solo project's maintainability and to the clarity of what was actually learned.

### IV. Modular Architecture
The system MUST be organized into modules with clear, well-named boundaries and explicit,
minimal dependencies between them. Cross-module communication MUST go through explicit
interfaces or contracts, never through shared internal state. The system SHOULD start and remain
a modular monolith by default; extracting a module into a separately deployable service is
permitted only when a specific, articulated scaling, ownership, or technical constraint justifies
it, and MUST be recorded as a decision (Principle IX).

Backend modules MUST be designed and coded following microservices architecture patterns and
conventions — a self-contained domain, application, and persistence boundary per module, wired
together only through explicit contracts — even though every module currently runs inside one
monolith deployable. The HTTP/API transport layer (controllers, request/response contracts,
routing) MUST be kept outside each module's own boundary, in the shared host, so that extracting
a module later is a transport-wiring change, not a rewrite of the module's internals.

**Rationale**: Clear boundaries make later evolution (e.g., toward event-driven or distributed
designs) an option rather than a rewrite, without paying distributed-systems cost up front.
Coding modules to microservice conventions from the start — while deferring the actual
distributed deployment — lets this project practice that discipline continuously rather than
retrofitting it under pressure once a split actually becomes necessary.

### V. Quality by Default
Every change that alters behavior MUST include automated tests appropriate to its risk and
complexity, and MUST pass all existing automated validation (tests, linting, type-checking,
build) before it is considered done. Tests SHOULD exercise real dependencies (e.g., via
containerized services) wherever practical; mocking SHOULD be reserved for true external
boundaries that cannot reasonably run locally.

**Rationale**: Automated validation is both a safety net for a project maintained by a single
person over a long time and a core engineering skill this project exists to practice.

### VI. Specification-Driven Development
Every non-trivial feature MUST have a specification that serves as the source of truth for its
intended behavior. Specifications and plans MUST be consistent with the concrete, per-technology
engineering conventions recorded under [.claude/rules/](../../.claude/rules/) for the
technologies the feature touches, not only with this constitution's principles — research and
design MUST take those conventions into account rather than rediscovering or re-litigating them
per feature. Implementation MUST conform to its specification. Any deviation discovered during
implementation MUST be reflected back into the specification before the work is considered
complete.

**Rationale**: Keeping specs and implementation aligned is what allows specs to be trusted as a
working reference for both the human maintainer and any AI agents involved. Grounding specs and
plans in the same concrete rules code must already follow keeps them realistic and prevents a
spec from prescribing something the codebase's own conventions would reject at review time.

### VII. Agent-Assisted Development
AI agents are first-class contributors to this project's workflow and MAY be used for
exploration, implementation, testing, and review. Agents MUST work from specifications, this
constitution, and the applicable conventions under [.claude/rules/](../../.claude/rules/) rather
than from ad hoc, undocumented instructions — including when using Spec Kit skills for a
feature's research, clarification, planning, task breakdown, and implementation. Any significant
architectural decision, module boundary change, or new external dependency proposed by an agent
MUST be reviewed and explicitly approved by the human maintainer before it is merged.

**Rationale**: Practicing agentic development is an explicit goal of this project, but
architecture and dependency choices have long-term consequences that require human judgment.
Spec Kit's phases are where an agent decides how a feature will be built; skipping the rules at
that stage would let a spec or plan drift from established conventions before a human ever
reviews the resulting code.

### VIII. Local-First and Reproducible
The system MUST be runnable, testable, and debuggable entirely on a local machine, without a
required dependency on shared or cloud environments. Local setup MUST be reproducible from
versioned configuration (e.g., containerized dependencies and scripted setup), not from manual,
undocumented steps.

**Rationale**: A local-first workflow keeps iteration fast for a solo contributor and forces
good practices (reproducible environments, infrastructure as configuration) early.

### IX. Document Important Decisions
Significant architectural or technology decisions, including the trade-offs considered and
lessons learned, MUST be recorded at the time they are made (e.g., as an Architecture Decision
Record). A decision that reverses or supersedes an earlier one MUST reference the decision it
replaces.

**Rationale**: In a long-lived personal project, the reasoning behind past decisions is easy to
forget; recorded decisions preserve the learning that this project is meant to produce.

## Technology Approach

This project uses its technology stack as a vehicle for learning, and is currently exploring
.NET, React/TypeScript, PostgreSQL, Kafka, Docker, Kubernetes, SignalR, OpenTelemetry, and
Testcontainers, among others. No technology is mandated by this constitution: its use in any
given module MUST still satisfy Simplicity First (Principle III) and Modular Architecture
(Principle IV), and adopting or replacing a technology MUST be recorded per Principle IX when it
constitutes a significant decision.

## Governance

This constitution takes precedence over other informal practices and instructions when they
conflict. Amendments MUST be made as an update to this file that states the rationale for the
change and updates the Sync Impact Report and version footer below; for this single-maintainer
project, merging that update constitutes ratification.

Versioning follows semantic versioning: MAJOR for backward-incompatible governance changes or
removal/redefinition of a principle, MINOR for adding a principle or materially expanding
guidance, PATCH for clarifications and wording fixes that do not change intent.

Specs, plans, and non-trivial changes SHOULD be checked against these principles and the
applicable conventions under [.claude/rules/](../../.claude/rules/) before merge.
Repeated or persistent violation of a principle MUST be resolved either by bringing the work
into compliance or by amending this constitution to reflect a deliberate, documented change in
practice — not by silent drift.

**Version**: 1.2.0 | **Ratified**: 2026-09-05 | **Last Amended**: 2026-09-11
