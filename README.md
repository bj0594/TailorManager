# TailorManager

TailorManager is an order management system for tailoring businesses that handle new garment orders and repairs.

Employees can register customers and orders, track workshop progress, schedule fittings, communicate with customers, register payments and see the complete history of each order.

## Run the tests

From the repo root:

```powershell
dotnet test
```

## Run the API

From the repository root:

```powershell
dotnet run --project TailorManager.Api
```

The API runs on the address it shows, like `http://localhost:5xxx`. The OpenAPI document is at `/openapi/scalar/v1`, and `TailorManager.Api/TailorManager.Api.http` has a request for each endpoint.

## Run with Docker Compose

From the repository root:

```powershell
docker compose up --build -d
```

| Service | Address |
|---|---|
| API | `http://localhost:8080` (health: `/health`) |
| pgAdmin | `http://localhost:5050` |
| PostgreSQL | `localhost:5432` |

`docker compose ps` shows the status, and the API is ready when it shows `(healthy)`. Stop everything with `docker compose down`. `docker compose down -v` also deletes the database.

To check that the container answers, run `bash scripts/smoke-test.sh` in Git Bash.

## Database

The data is stored in PostgreSQL with EF Core. The tables come from the migrations in `TailorManager.Infrastructure/Migrations/`.

With Docker Compose, the API runs the migrations when it starts, so `docker compose up --build -d` is all you need. The data is kept in the volume `db_data`, and `docker compose down -v` deletes it.

To run the API with `dotnet run` instead, start the database and set the connection string once, from the repository root:

~~~powershell
docker compose up -d db
dotnet user-secrets set "ConnectionStrings:Default" "Host=localhost;Port=5432;Database=tailor;Username=employee;Password=password" --project TailorManager.Api
dotnet ef database update --project TailorManager.Infrastructure --startup-project TailorManager.Api -- --environment Development
~~~

After a change to a model class, add a migration:

~~~powershell
dotnet ef migrations add <MigrationName> --project TailorManager.Infrastructure --startup-project TailorManager.Api
~~~

`dotnet test` also runs the repository contract tests against a PostgreSQL that Testcontainers starts, so Docker must be running.

## Documentation

- [Specification](Specification.md): requirements, flow, interface, test list and decisions
- [API contract](api.md): endpoints, requests, responses and status codes
