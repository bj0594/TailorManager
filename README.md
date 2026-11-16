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

The API runs on the address it shows, like `http://localhost:5250`. Scalar (the API reference with the Authorize button) is at `/scalar/v1`, the OpenAPI document at `/openapi/v1.json`, and `TailorManager.Api/TailorManager.Api.http` has a request for each endpoint.

## Run with Docker Compose

From the repository root:

```powershell
docker compose up --build -d
```

| Service | Address |
|---|---|
| Website | `http://localhost:5100` |
| API | `http://localhost:8080` (health: `/health`) |
| pgAdmin | `http://localhost:5050` |
| PostgreSQL | `localhost:5432` |

`docker compose ps` shows the status, and the API is ready when it shows `(healthy)`. Stop everything with `docker compose down`. `docker compose down -v` also deletes the database.

To check that the container answers, run `bash scripts/smoke-test.sh` in Git Bash.

## Database

The data is stored in PostgreSQL with EF Core. The tables come from the migrations in `TailorManager.Infrastructure/Migrations/`.

With Docker Compose, the API runs the migrations when it starts, so `docker compose up --build -d` is all you need. The data is kept in the volume `db-data`, and `docker compose down -v` deletes it.

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

## Log in

1. Register: `POST /auth/register` with `{ "username": "<username>", "password": "<password>", "firstName": "<first name>", "lastName": "<last name>", "role": "FrontDesk" }`. The role is `FrontDesk` or `Workshop`.
2. Log in: `POST /auth/login` with the same body, and copy `token` from the response.
3. Send the token with each request: in Swagger, click Authorize and paste it. In the `.http` file, the requests below `# @name login` use `Authorization: Bearer {{login.response.body.$.token}}`.

Without a valid token, the endpoints that require login answer `401 Unauthorized`.

The key the tokens are signed with is in `TailorManager.Api/appsettings.Development.json`. Outside Development, set it as the environment variable `Jwt__Key` (64 random characters).

## Run the website

With Docker Compose, the website runs on `http://localhost:5100` (table above). Register on the start page, log in, and the menu shows the pages for your part (FrontDesk or Workshop).

To run it with `dotnet run` instead, start the API first (Docker Compose or `dotnet run`), and from the repository root:

```powershell
dotnet run --project TailorManager.Web
```

The website finds the API at `ApiBaseUrl` in `TailorManager.Web/appsettings.Development.json`. The login is kept while the tab is open, and is lost when the page is reloaded.

## Project structure

| Project | What is in it |
|---|---|
| `TailorManager.Core` | the types, the services and the rules, one folder per feature (`Customers/`, `Orders/`, `Users/`), and the repository interfaces with their in-memory fakes in `Repository/` |
| `TailorManager.Infrastructure` | EF Core: the `DbContext`, the repositories in `Repositories/`, and `Migrations/` |
| `TailorManager.Api` | the controllers, and login in `Auth/` |
| `TailorManager.Web` | the website: the API clients in `Api/`, and the pages and the menu in `Components/` |
| `TailorManager.Tests` | the tests for Core (`Domain/`), the API (`Api/`) and the repositories (`Repository/`) |
| `TailorManager.Web.Tests` | the tests for the website: `Clients/`, `Pages/`, `Layout/`, and the fakes in `Fakes/` |

## Documentation

- [Specification](Documentation/Specification.md): requirements, flow, interface, test list and decisions
- [API contract](Documentation/api.md): endpoints, requests, responses and status codes
