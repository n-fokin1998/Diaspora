---
paths:
  - "**/*.cs"
---

# Kafka Rules

Kafka is part of the planned stack (see [AGENTS.md](../../AGENTS.md)) for asynchronous
communication *between* modules or services — it is not a substitute for a direct in-process
call within a single module, and it should not be introduced until a concrete cross-module async
use case exists (per the constitution's Simplicity First principle).

- Define integration events as explicit, versioned contracts (plain DTOs) owned by the
  publishing module, never as a serialized internal domain entity.
- Name topics `<domain>.<event-name>.<version>` (e.g. `events.event-created.v1`); bump the
  version segment on a breaking payload change instead of mutating an existing version in place.
- Publish events after the originating database transaction has committed. Prefer the outbox
  pattern (write the event to an outbox table in the same transaction as the business change,
  then relay it to Kafka) over a dual-write that can publish an event for a change that later
  rolls back.
- Consumers must be idempotent — dedupe on an event id or business key — since Kafka delivery is
  at-least-once and the same event may be processed more than once.
- Keep the number of topics and consumer groups minimal: one topic per meaningful event type, one
  consumer group per independent subscriber, not one topic per producer.
