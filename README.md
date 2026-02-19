# PourEvents API

Production-quality ASP.NET Core Web API for beer pour event tracking.

## Prerequisites

- .NET 8 SDK
- PostgreSQL (Supabase or local)

## Configuration

Set environment variables:

| Variable | Description |
|---|---|
| `DATABASE_URL` or `ConnectionStrings__DefaultConnection` | PostgreSQL connection string |
| `API_KEY` | API key for X-API-Key authentication |
| `PORT` | HTTP port (default: 10000, auto-set by Render) |

## Run Locally

```bash
export DATABASE_URL="Host=localhost;Port=5432;Database=pours_dev;Username=postgres;Password=postgres"
export API_KEY="dev-api-key-change-me"
dotnet run --project src/Pours.Api
```

## Run Migrations

```bash
dotnet tool install --global dotnet-ef
dotnet ef migrations add InitialCreate --project src/Pours.Infrastructure --startup-project src/Pours.Api
dotnet ef database update --project src/Pours.Infrastructure --startup-project src/Pours.Api
```

## Run Tests

Requires a real PostgreSQL database:

```bash
export TEST_DATABASE_URL="Host=localhost;Port=5432;Database=pours_test;Username=postgres;Password=postgres"
dotnet test tests/Pours.Tests
```

## Deploy to Render

1. Push repo to GitHub
2. Create a new **Web Service** in Render
3. Select **Docker** as runtime
4. Set environment variables: `DATABASE_URL`, `API_KEY`
5. Render auto-detects `Dockerfile` and deploys
6. Health check: `GET /health`

Or use the `render.yaml` Blueprint for automated setup.

## API Endpoints

### GET /health
No auth required. Returns DB connectivity status. Returns 503 when DB is unreachable.

### POST /v1/pours
Requires `X-API-Key` header. Idempotent — duplicate `eventId` returns 200.

### GET /v1/taps/{deviceId}/summary?from=...&to=...
Requires `X-API-Key` header. Returns aggregated pour data.

## Interview Test Checklist (curl)

Assume:

- `BASE_URL` is your deployed URL
- `API_KEY` is the configured key

```bash
BASE_URL="https://your-service.onrender.com"
API_KEY="your-api-key"
```

1) Health

```bash
curl -sS "$BASE_URL/health"
```

2) Unauthorized request

```bash
curl -sS -X POST "$BASE_URL/v1/pours" -H "Content-Type: application/json" -d '{}'
```

3) Validation error (from > to)

```bash
curl -sS "$BASE_URL/v1/taps/tap-device-001/summary?from=2026-02-20T00:00:00Z&to=2026-02-19T00:00:00Z" \
	-H "X-API-Key: $API_KEY"
```

4) Ingest pour (201 on first insert)

```bash
curl -sS -X POST "$BASE_URL/v1/pours" \
	-H "Content-Type: application/json" \
	-H "X-API-Key: $API_KEY" \
	-d '{
		"eventId":"11111111-1111-1111-1111-111111111111",
		"deviceId":"tap-device-001",
		"locationId":"istanbul-kadikoy-01",
		"productId":"efes-pilsen",
		"startedAt":"2026-02-19T10:00:00Z",
		"endedAt":"2026-02-19T10:00:05Z",
		"volumeMl":330
	}'
```

5) Idempotency (same eventId, 200)

```bash
curl -sS -X POST "$BASE_URL/v1/pours" \
	-H "Content-Type: application/json" \
	-H "X-API-Key: $API_KEY" \
	-d '{
		"eventId":"11111111-1111-1111-1111-111111111111",
		"deviceId":"tap-device-001",
		"locationId":"istanbul-kadikoy-01",
		"productId":"efes-pilsen",
		"startedAt":"2026-02-19T10:00:00Z",
		"endedAt":"2026-02-19T10:00:05Z",
		"volumeMl":330
	}'
```

6) Summary

```bash
curl -sS "$BASE_URL/v1/taps/tap-device-001/summary?from=2026-02-19T00:00:00Z&to=2026-02-20T00:00:00Z" \
	-H "X-API-Key: $API_KEY"
```
