# Factory Maintenance API v1
A REST API for managing factory machines and tracking their maintenance status.

## Tech Stack
- .NET 10
- ASP.NET Core Minimal APIs
- OpenAPI

## How to run it
- requires .NET 10 SDK
- to start the project run `dotnet run`
- the requests can be sent in `FactoryMaintenance.http`

## An important note
data is stored in memory and resets on every restart.

## What I learned
I learned how to structure endpoints as well as handle input validation, use DTOs and return HTTP status codes.

## What's next
- database with EF Core
- declarative validation rules
- automated tests
- authentication
- Docker

## Endpoints
- GET - `/machines` - lists every machine
- GET - `/machines/{id}` - returns the machine with the given id
- GET - `/machines?status=STATUS` - filters machines by status
- POST - `/machines` - creates a new machine
- PATCH - `/machines/{id}/status` - changes the status of the given machine

## Different endpoints return different Results<> instances

1. POST - returns either
- `Created<Machine>` with status 201, creating a new machine,
- `BadRequest<string>` with status 400 and corresponding error message for a missing status or empty name or location

2. PATCH - returns one of three
- `NoContent` with status 204, accepting the change
- `BadRequest<string>` with status 400, missing status
- `NotFound` with status 404, unknown id

3. GET with `{id}` - either returning 200 with `Ok<Machine>` or `NotFound` with status code of 404.

## Example request
- Request
```http
POST http://localhost:5259/machines
Content-Type: application/json

{
  "name": "Name",
  "location": "Hall C",
  "status": "Running"
}
```

- Response
```http
HTTP/1.1 201 Created
Content-Type: application/json; charset=utf-8
Location: /machines/3

{
  "id": 3,
  "name": "Name",
  "location": "Hall C",
  "status": "Running"
}
```
