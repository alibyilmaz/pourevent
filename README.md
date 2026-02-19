# PourEvents API

Beer pour event tracking API built with .NET 8 and Clean Architecture.

## Build & Run

```bash
dotnet build
dotnet run --project src/Pours.Api
```

API runs at `http://localhost:10000`. Swagger UI available at root (`/`).

## Configuration

| Variable | Description |
|---|---|
| `ConnectionStrings__DefaultConnection` | PostgreSQL connection string |
| `Authentication__ApiKey` | API key for authentication |

## Run Tests

```bash
dotnet test
```

## API Endpoints

| Method | Endpoint | Auth | Description |
|---|---|---|---|
| GET | `/health` | No | Health check |
| POST | `/v1/pours` | X-API-Key | Ingest pour event (idempotent) |
| GET | `/v1/taps/{deviceId}/summary` | X-API-Key | Get pour summary |

### POST /v1/pours

```json
{
  "eventId": "uuid",
  "deviceId": "string",
  "locationId": "string",
  "productId": "string",
  "startedAt": "ISO8601",
  "endedAt": "ISO8601",
  "volumeMl": 330
}
```

### GET /v1/taps/{deviceId}/summary

Query params: `from`, `to` (ISO8601 timestamps)

## Docker

```bash
docker build -t pourevents .
docker run -p 10000:10000 -e ConnectionStrings__DefaultConnection="..." -e Authentication__ApiKey="..." pourevents
```
