---
paths:
  - "**/*.cs"
---

# OpenTelemetry Rules

OpenTelemetry is part of the planned stack (see [AGENTS.md](../../AGENTS.md)) for observability
across the backend.

- Instrument through the standard `OpenTelemetry.Extensions.Hosting` setup plus the official
  ASP.NET Core, `HttpClient`, and Npgsql instrumentation packages, instead of hand-rolled
  tracing/logging code.
- Wire all three signals — traces, metrics, and logs — through OpenTelemetry so there is one
  observability pipeline, rather than a separate logging framework configured independently.
- Create one `ActivitySource` and, if needed, one `Meter` per module, named after the module
  (e.g. `Diaspora.Events`), instead of a single global tracer — this keeps spans and metrics
  attributable to the module that produced them, consistent with the modular architecture.
- Use structured logging with the trace/span id attached (the default when logs flow through the
  OpenTelemetry logging provider), not plain string interpolation into logs.
- Export via OTLP to a local collector (added to
  `infrastructure/docker/docker-compose.yml` when this is introduced) in development. Depend only
  on the vendor-neutral OpenTelemetry API/SDK in application code; keep any vendor-specific
  backend configuration in the exporter/collector setup, not in business code.
