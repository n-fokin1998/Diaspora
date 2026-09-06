# AGENTS.md

Instructions for AI coding agents working in this repository.

Diaspora is a personal pet project for learning modern software engineering and agentic
development. The repository is currently close to greenfield — most of what follows describes
either the current, minimal state or the intended direction, and the two are labeled explicitly
so agents don't assume more exists than actually does.

Also see the project constitution at [.specify/memory/constitution.md](.specify/memory/constitution.md)
for durable engineering principles, any specifications under `.specify/` for intended behavior
of specific features, and [.claude/rules/](.claude/rules/) for concrete per-technology
engineering conventions.

## Tech Stack

**Currently in use:**
- .NET 9 / ASP.NET Core Web API (backend)
- React + TypeScript, built with Vite (frontend)
- PostgreSQL, via Docker Compose (database)
- Docker / Docker Compose (local infrastructure)

**Planned / intended direction (not yet present in code):**
- Entity Framework Core (data access)
- Kafka (async messaging between modules/services)
- Kubernetes (deployment)
- OpenTelemetry (observability)

Do not assume EF Core, Kafka, Kubernetes, or OpenTelemetry are wired up until you have checked
the repository — introduce them only when a concrete task requires it, per the constitution's
Simplicity First principle.

## Architecture

The intended architecture is a **modular monolith**: a single deployable backend organized into
modules with clear boundaries and explicit interfaces between them, with a separate frontend
application. Splitting a module out into its own service is deferred until a specific,
articulated need justifies it (see the constitution's Modular Architecture principle).

Today, the backend is a single ASP.NET Core Web API project with no internal module boundaries
yet defined, and the frontend is a single Vite/React app. Do not invent module boundaries or
service splits that aren't already there — add structure only as real features require it.

## Repository Structure

```
src/
  Web/
    Diaspora.sln              # .NET solution
    Client.Api/                # ASP.NET Core Web API project
      spa/                      # React + TypeScript frontend (Vite)
infrastructure/
  docker/
    docker-compose.yml         # local Postgres + pgAdmin
.specify/                    # Spec Kit: constitution, specs, plans, tasks
```

This structure is intentionally minimal. Do not create additional top-level folders (e.g. for
future services, shared libraries, or Kubernetes manifests) speculatively — add them when a
task actually needs them.

## Development Commands

All commands below exist in the repository today and are run from the repository root unless
noted otherwise.

**Backend (.NET):**
```bash
dotnet build src/Web/Diaspora.sln
dotnet run --project src/Web/Client.Api
dotnet format src/Web/Diaspora.sln
```
There is no automated test project yet.

**Frontend (from `src/Web/Client.Api/spa`):**
```bash
npm install
npm run dev
npm run build
npm run lint
```

**Local infrastructure (Docker Compose):**
```bash
docker compose -f infrastructure/docker/docker-compose.yml up -d
docker compose -f infrastructure/docker/docker-compose.yml down
```
This currently starts PostgreSQL and pgAdmin only.

**Kubernetes:** no manifests or commands exist yet; this is planned direction only.

## Project Rules

Concrete, per-technology engineering conventions live under [.claude/rules/](.claude/rules/) as
small, focused files rather than one large document. Rules scoped to a file type via a `paths:`
frontmatter field load automatically when a matching file is opened; the rest load every
session:

- [architecture.md](.claude/rules/architecture.md) — module boundaries, layering, infra separation (always loaded)
- [testing.md](.claude/rules/testing.md) — testing conventions (always loaded)
- [csharp-dotnet.md](.claude/rules/csharp-dotnet.md) — C# / .NET conventions (`**/*.cs`)
- [aspnetcore-webapi.md](.claude/rules/aspnetcore-webapi.md) — Web API conventions (`**/*.cs`)
- [efcore-postgresql.md](.claude/rules/efcore-postgresql.md) — data access conventions (`**/*.cs`)
- [kafka.md](.claude/rules/kafka.md) — async messaging conventions (`**/*.cs`)
- [opentelemetry.md](.claude/rules/opentelemetry.md) — observability conventions (`**/*.cs`)
- [react-typescript.md](.claude/rules/react-typescript.md) — frontend conventions (`**/*.ts`, `**/*.tsx`)
- [docker.md](.claude/rules/docker.md) — container conventions (Dockerfiles, compose files, `infrastructure/**`)

Add new rule files here as the stack grows; keep each one scoped to a single technology or
concern, and add a `paths:` frontmatter field when it should only apply to a specific file type
or area.

## Agent Guidelines

- Inspect the actual repository state before acting — do not assume a file, module, package, or
  command exists because it's mentioned here as "planned"; verify first.
- Follow the [project constitution](.specify/memory/constitution.md), the rules under
  [.claude/rules/](.claude/rules/), and any relevant specification under `.specify/` as the
  source of truth for intended behavior.
- Keep changes small and focused on the task at hand; avoid opportunistic refactors.
- Avoid adding new dependencies, abstractions, or infrastructure (including items from the
  "planned" list above) unless the current task genuinely requires them.
- Validate changes before finishing: build/run the affected project(s) and run lint/format, using
  the commands in this file.
