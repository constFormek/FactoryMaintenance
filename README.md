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

## Notes
- Data is stored in memory and resets on every restart.
- IDs are assigned sequentially and never reused.
- Access to the machines list is synchronised with lock, so concurrent requests are safe.

## What I learned
I learned how to structure endpoints as well as handle input validation, use DTOs and return HTTP status codes.

## What's next
- database with EF Core
- declarative validation rules
- automated tests
- authentication
- Docker

## Endpoints
| Method | Route                    | Description                         |
| ------ | ------------------------ | ----------------------------------- |
| GET    | `/machines`              | lists every machine                 |
| GET    | `/machines/{id}`         | gives back machine of the given id  |
| GET    | `/machines?status=STATUS`| filters machines by status          |
| POST   | `/machines`              | creates a new machine               |
| PATCH  | `/machines/{id}/status`  | changes the status of the machine   |
| DELETE | `/machines/{id}`         | deletes a machine of the given id   |

## Responses
| Endpoint | Status | Result Type | When |
| -------- | ------ | ----------- | ---- |
| GET `/machines` | 200 | 'Ok<Machine[]> | Always (empty array if nothing matches)
| GET `/machines/{id}` | 200 | 'Ok<Machine> | Machine Found |
| | 404 | 'NotFound' | No machine with that id |
| POST `/machines` | 201 | 'Created<Machine>' | Machine created |
| | 400 | 'BadRequest<string>' | Missing status, name or location |
| PATCH `/machines/{id}` | 204 | 'NoContent' | Status changed |
| | 400 | 'BadRequest<string>' | Missing status |
| | 404 | 'NotFound' | No machine with that id |
| DELETE `/machines/{id}` | 204 | 'NoContent' | Machine deleted |
| | 404 | 'NotFound' | No machine with that id |

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
