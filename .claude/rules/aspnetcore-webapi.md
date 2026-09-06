---
paths:
  - "**/*.cs"
---

# ASP.NET Core Web API Rules

- Use **controllers** (as already established by `Client.Api`), not Minimal API endpoints, for
  consistency across the codebase. If a strong reason to switch arises, migrate a whole module
  at once, not endpoint by endpoint.
- Controllers are thin: they parse the request, call into the module's Application layer, and
  map the result to an HTTP response. No business logic, no direct EF Core/database access in a
  controller.
- Never expose EF Core entities directly as request/response bodies. Define explicit request and
  response DTOs per endpoint (or shared per module where genuinely identical).
- Validate incoming requests before invoking business logic (data annotations or a validation
  library) and return `ProblemDetails` (via `Results.Problem`/`ControllerBase.Problem`) for error
  responses, not ad hoc error shapes.
- Use conventional REST resource routing (`/api/<resource>`, plural nouns) and standard HTTP
  status codes (`200/201/204` for success, `400` for validation errors, `404` for missing
  resources, `409` for conflicts).
- Put cross-cutting concerns (global error handling, authentication, request logging) in
  middleware or filters shared across controllers, not repeated in each action method.
- Keep OpenAPI generation (`Microsoft.AspNetCore.OpenApi`, already referenced) enabled for local
  development; do not hand-write API documentation that duplicates it.
