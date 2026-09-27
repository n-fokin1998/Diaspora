# Diaspora

Find your people in a new city.

Diaspora is a social platform for people who've just landed in a new country or city and want to
build a life there. Users create activities — anything from a hiking trip to a board-game night —
and others browse a feed of what's happening nearby and join in. The goal is simple: turn "I don't
know anyone here" into a calendar full of things to do with people you've met.

## Table of Contents

- [Status](#status)
- [Features](#features)
- [Tech Stack](#tech-stack)
- [Architecture](#architecture)
- [Getting Started](#getting-started)
- [Testing](#testing)
- [Project Structure](#project-structure)
- [Documentation](#documentation)
- [Roadmap](#roadmap)
- [License](#license)

## Status

Diaspora is under active early-stage development. The identity foundation (registration, login,
sessions) and the public welcome page are implemented; the activity feed itself — the core of the
product — is still ahead. See [Roadmap](#roadmap).

## Features

- **Welcome page** — a public landing page that explains what Diaspora is and routes visitors to
  sign in or sign up.
- **Account registration** — email, password, first/last name, date of birth, and location, with
  server-side validation.
- **Authentication** — email/password login backed by JWT access tokens and a rotating refresh
  token, so a session survives a page refresh or browser restart within its validity window.
- **Logout** — revokes the active refresh token and ends the session.

## Tech Stack

| Layer | Technology |
|---|---|
| Backend | .NET 9, ASP.NET Core Web API (controllers), MediatR (CQRS-style command/query dispatch) |
| Data access | Entity Framework Core, Npgsql, one schema per module |
| Auth | JWT bearer tokens with a refresh-token flow |
| Frontend | React 19 + TypeScript, Vite, React Router, Feature-Sliced Design |
| Database | PostgreSQL |
| Testing | xUnit, Moq, Testcontainers (backend) · Vitest, React Testing Library (frontend) |
| Infrastructure | Docker / Docker Compose |

## Architecture

Diaspora is a **modular monolith built to microservices conventions**: it deploys as a single
backend today, but each business capability is organized as a self-contained module — its own
`Domain`, `Application`, and `Infrastructure` layers, its own database schema — wired to the rest
of the system only through explicit contracts. That keeps the door open to extracting a module
into its own service later without rewriting its internals, without paying distributed-systems
costs before there's a real need to.

Within a module:
- **`Domain`** holds entities and business rules, with no framework dependencies.
- **`Application`** holds use cases as MediatR commands/queries, plus the abstractions
  (repositories, a unit of work, external services) that `Infrastructure` implements. A shared
  `ValidationBehavior` pipeline runs request validation before a handler executes.
- **`Infrastructure`** holds the module's EF Core `DbContext`, its own PostgreSQL schema, and the
  concrete implementations of `Application`'s abstractions.

The HTTP transport layer — controllers, request/response DTOs, routing — lives outside every
module, in the `Client.Api` host, which dispatches into a module's `Application` layer via
MediatR. The frontend is a separate Vite/React single-page app, structured with Feature-Sliced
Design (`app` → `pages` → `widgets` → `features` → `entities` → `shared`).

`Identity` is the first module and the reference example for this layout; new business
capabilities (activities, the feed, venues, ...) are added as their own modules following the same
shape as they're built.

The concrete, per-technology conventions this project follows are recorded under
[`.claude/rules/`](.claude/rules/), and the durable engineering principles behind them in the
[project constitution](.specify/memory/constitution.md).

## Getting Started

### Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Node.js](https://nodejs.org/) 20+ and npm
- [Docker](https://www.docker.com/) (for PostgreSQL, and to run the automated tests, which spin up
  a real database via Testcontainers)

### 1. Start local infrastructure

```bash
docker compose -f infrastructure/docker/docker-compose.yml up -d db pgadmin
```

This starts PostgreSQL (`localhost:5432`) and pgAdmin (`localhost:5050`).

### 2. Configure the backend

The API needs a connection string and JWT signing configuration. In development, set these with
`dotnet user-secrets` from `src/Client.Api`:

```bash
dotnet user-secrets set "ConnectionStrings:DiasporaDb" "Host=localhost;Database=Diaspora;Username=admin;Password=secret" --project src/Client.Api
dotnet user-secrets set "Jwt:Key" "<a long random development-only signing key>" --project src/Client.Api
dotnet user-secrets set "Jwt:Issuer" "Diaspora" --project src/Client.Api
dotnet user-secrets set "Jwt:Audience" "Diaspora.Client" --project src/Client.Api
```

Apply the database schema (install the EF Core CLI once, if you don't have it):

```bash
dotnet tool install --global dotnet-ef
dotnet ef database update --project src/Modules/Identity/Identity.Infrastructure --startup-project src/Client.Api
```

### 3. Run the backend

```bash
dotnet run --project src/Client.Api
```

### 4. Run the frontend

```bash
cd src/Client.Api/spa
npm install
npm run dev
```

The SPA runs at `http://localhost:5173` and talks to the API at `http://localhost:5220` (CORS is
already configured for this origin).

## Testing

```bash
dotnet test src/Diaspora.sln
```

```bash
cd src/Client.Api/spa
npm run test
```

Backend integration tests spin up a disposable PostgreSQL container via Testcontainers, so Docker
must be running.

## Project Structure

```
src/
  Diaspora.sln                  # .NET solution
  Client.Api/                    # ASP.NET Core Web API host: transport layer only
    Controllers/                   # thin controllers per module, dispatch via MediatR
    spa/                            # React + TypeScript frontend (Vite), Feature-Sliced Design
  Diaspora.Core/                 # small, stable cross-cutting kernel shared by modules
  Diaspora.Contracts/            # DTOs/events shared across module (later, service) boundaries
  Diaspora.Tests/                # xUnit tests spanning Client.Api / multiple modules
  Modules/
    Identity/                      # registration, login, sessions — reference module layout
      Identity.Domain/
      Identity.Application/
      Identity.Infrastructure/
      Identity.Tests/
infrastructure/
  docker/
    docker-compose.yml           # local Postgres + pgAdmin (+ containerized API/SPA)
```

## Documentation

- **Feature docs**: [Diaspora on Notion](https://app.notion.com/p/Diaspora-3d2a910dbf8880e6ae1ec62610aebbad?source=copy_link)
- **Feature specifications**: [`specs/`](specs/)
- **Engineering conventions**: [`.claude/rules/`](.claude/rules/)
- **Project constitution**: [`.specify/memory/constitution.md`](.specify/memory/constitution.md)
- **Agent instructions**: [`AGENTS.md`](AGENTS.md)

## Roadmap

- Activities: creating, browsing, and joining events (the core product loop)
- An activity feed, with filtering by location and interest
- Additional business modules (e.g. venues, notifications) following the `Identity` module's
  layout
- Asynchronous messaging between modules via Kafka, once a concrete cross-module use case exists
- Observability via OpenTelemetry
- Containerized deployment to Kubernetes

## License

MIT — see [LICENSE](LICENSE).
