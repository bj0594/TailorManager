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

## Documentation

- [Specification](Specification.md): requirements, flow, interface, test list and decisions
- [API contract](api.md): endpoints, requests, responses and status codes
