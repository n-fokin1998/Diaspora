---
paths:
  - "**/Dockerfile*"
  - "**/docker-compose*.yml"
  - "infrastructure/**"
---

# Docker Rules

- One Dockerfile per deployable app (e.g. the `Client.Api` backend; the frontend later if it is
  served as its own container). Use a multi-stage build: an SDK/build stage that restores and
  publishes/builds, and a minimal runtime stage (e.g. `aspnet` runtime image for .NET, a static
  file or Node runtime image for the frontend) that only contains the published output.
- Run the final container as a non-root user where the base image supports it; do not add
  `USER root` back in the runtime stage.
- Never bake secrets or connection strings into an image. Pass configuration via environment
  variables at container run time.
- Local development dependencies (PostgreSQL, and later Kafka) are defined in the single
  `infrastructure/docker/docker-compose.yml` — add new services there rather than creating
  parallel or per-service compose files.
- The credentials currently in `docker-compose.yml`/`servers.json`/`.pgpass` are local-only
  defaults for development; do not copy them into any non-local configuration.
- Keep a `.dockerignore` next to each Dockerfile excluding build output and dependency
  directories (`bin/`, `obj/`, `node_modules/`, `.vs/`) so images stay small and builds stay
  reproducible.
