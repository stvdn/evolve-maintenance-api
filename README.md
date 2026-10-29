# Evolve Maintenance API

A .NET 9 REST API for managing software maintenance requests and quality metrics.
Final academic project using Clean Architecture, PostgreSQL, JWT authentication,
role-based authorization, rate limiting, and Swagger documentation.

## Deploy on Dokploy

The API and database run together as one Docker Compose deployment. No separate
.NET installation or database service is required on the VPS.

1. In Dokploy, create a project named **Evolve Maintenance API**.
2. Add a **Compose** service and choose **Docker Compose**, not Docker Stack.
3. Choose the **GitHub** provider. Grant the Dokploy GitHub integration access to
   this private repository if it is not listed.
4. Select repository `stvdn/evolve-maintenance-api`, branch `main`, and Compose
   Path `./compose.dokploy.yml`. Keep the build context at the repository root.
5. In **Environment**, paste these entries, replacing both placeholders with
   different random values (letters and digits are simplest):

   ```env
   POSTGRES_PASSWORD=REPLACE_WITH_A_RANDOM_32_CHARACTER_PASSWORD
   JWT_SECRET=REPLACE_WITH_A_RANDOM_SECRET_OF_AT_LEAST_64_CHARACTERS
   ```

   Save them in Dokploy; do not commit actual secrets to GitHub. Keep the same
   database password on future deployments because the database volume persists.
6. Point an `A` record such as `maintenance.example.com` to your VPS IP.
7. In the service's **Domains** tab, add that host with:

   | Setting | Value |
   | --- | --- |
   | Service | `api` |
   | Container port | `8080` |
   | Path | `/` |
   | HTTPS | Enabled |
   | Certificate | Let's Encrypt |

8. Save and click **Deploy**. Domain changes require redeployment.
9. Open `https://maintenance.example.com/health` and
   `https://maintenance.example.com/api-docs`.

The compose file connects the API to Dokploy's existing proxy network and a
separate default network shared with PostgreSQL. Neither service publishes a
host port. The database uses a named volume to retain data across redeployments.

For a demonstration without a personal domain, use Dokploy's generated domain
if that option is available. Use the scheme offered by Dokploy for that domain.

Official guides: [Compose setup](https://docs.dokploy.com/docs/core/docker-compose/example)
and [domain setup](https://docs.dokploy.com/docs/core/docker-compose/domains).

## Demonstrate the API

Swagger UI is available at `/api-docs`; there is no frontend home page.
Call `POST /api/v1/auth/login`, copy the returned JWT, then use Swagger's
**Authorize** button for protected operations.

The project seeds academic demo accounts on a new database:

| Role | Email | Password |
| --- | --- | --- |
| Architect | `juan.perez@bancoecuador.com` | `Architect@123` |
| Developer | `carlos.lopez@bancoecuador.com` | `Developer@123` |
| Manager | `maria.garcia@bancoecuador.com` | `Manager@123` |

These accounts are for the final-project demonstration. Change their passwords
through `PATCH /api/v1/users/me` before storing any real data.

## Local development

```sh
dotnet run --project src/WebApi
```

Without a connection string the app uses an in-memory database. The existing
`docker-compose.yml` provides a local PostgreSQL setup; `compose.dokploy.yml`
is the VPS deployment configuration.

## Repository layout

- `src/Domain`: entities and repository interfaces
- `src/Application`: application services and DTOs
- `src/Infrastructure`: persistence and JWT security
- `src/WebApi`: HTTP endpoints and middleware
- `tests/UnitTests`: unit tests
- `Fase*.md`, `Fase*.drawio`, `Fase*.svg`: academic documentation and diagrams
- `openapi.yaml`: API contract

## Validation

The Docker image was built successfully locally using the included Dockerfile.
The deployment Compose file was checked with `docker compose config`.
Deployment to the VPS must still be performed in Dokploy.
