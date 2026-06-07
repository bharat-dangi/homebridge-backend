# HomeBridge API (backend)

ASP.NET Core 10 **Web API** for the HomeBridge affordable-housing platform. It serves JSON to the
React single-page app, stores data in **PostgreSQL** via Entity
Framework Core, and authenticates with **JWT bearer tokens** issued by ASP.NET Core Identity.

> **New machine? Follow this README top to bottom.** It assumes nothing is installed yet.

---

## 1. Prerequisites

You need three things installed: the **.NET SDK 10**, **PostgreSQL 14+**, and (optionally) the
**`dotnet-ef`** tool. Install commands per operating system are below.

### .NET SDK 10

| OS | Install |
|----|---------|
| **macOS** | `brew install --cask dotnet-sdk` (or download from https://dotnet.microsoft.com/download/dotnet/10.0) |
| **Windows** | `winget install Microsoft.DotNet.SDK.10` (or the installer from the link above) |
| **Linux** | Follow https://learn.microsoft.com/dotnet/core/install/linux for your distro |

Verify (expect `10.x`):

```bash
dotnet --version
```

### PostgreSQL 14+

| OS | Install | Start the server |
|----|---------|------------------|
| **macOS** | `brew install postgresql@17` | `brew services start postgresql@17` |
| **Windows** | `winget install PostgreSQL.PostgreSQL.17` (or EnterpriseDB installer) | Runs as a Windows service automatically |
| **Linux (Debian/Ubuntu)** | `sudo apt install postgresql` | `sudo systemctl start postgresql` |

Verify the server is accepting connections:

```bash
pg_isready                      # → "accepting connections"
```

> The Homebrew `psql` client may not be on your `PATH`. If `psql` is “command not found” on macOS, run:
> `export PATH="/opt/homebrew/opt/postgresql@17/bin:$PATH"` (Apple Silicon) and re-open the terminal.

### dotnet-ef (only needed if you change the data model)

```bash
dotnet tool install --global dotnet-ef
# then make sure the tools dir is on PATH (macOS/Linux):  export PATH="$PATH:$HOME/.dotnet/tools"
```

---

## 2. Create the database

The app connects with the credentials in [`appsettings.json`](appsettings.json):
`Host=localhost;Port=5432;Database=homebridge;Username=homebridge;Password=homebridge`.

Create a matching role and database **once**. Run these from any terminal:

**macOS / Linux** (connects as your OS superuser):

```bash
psql -d postgres -c "CREATE ROLE homebridge LOGIN PASSWORD 'homebridge';"
psql -d postgres -c "CREATE DATABASE homebridge OWNER homebridge;"
```

**Windows** (run in *SQL Shell (psql)* or use the `postgres` superuser):

```bash
psql -U postgres -c "CREATE ROLE homebridge LOGIN PASSWORD 'homebridge';"
psql -U postgres -c "CREATE DATABASE homebridge OWNER homebridge;"
```

You do **not** need to create any tables — EF Core migrations build the schema automatically on first
run (see §3).

> **Want different credentials?** Edit `ConnectionStrings:DefaultConnection` in
> [`appsettings.json`](appsettings.json) to match whatever role/password/database you created.

---

## 3. Run the API

From this `backend/` folder:

```bash
dotnet restore                       # restore NuGet packages (first time)
dotnet run --launch-profile http     # → http://localhost:5180
```

On first launch the app automatically:

1. **applies the EF Core migrations** (creates all tables), and
2. **seeds** roles, two demo users, 18 sample listings, and a few demo applications/shortlist entries.

You should see `Now listening on: http://localhost:5180`. Quick check:

```bash
curl http://localhost:5180/api/home          # → JSON with counts + featured listings
```

The OpenAPI document is served at `http://localhost:5180/openapi/v1.json`.

> An HTTPS profile is also available: `dotnet run --launch-profile https`
> (→ https://localhost:7180; run `dotnet dev-certs https --trust` once to trust the dev certificate).

### Demo accounts

| Role | Email | Password |
|------|-------|----------|
| Administrator | `admin@homebridge.org` | `Admin#12345` |
| Applicant | `applicant@example.com` | `Applicant#123` |

---

## 4. Project layout

| Folder | Purpose |
|--------|---------|
| `Controllers/` | JSON API endpoints (Auth, Listings, Applications, Saved, Admin, Meta) |
| `Models/` | EF Core domain entities + `ApplicationUser` (Identity) |
| `Dtos/` | Request/response contracts (validated server-side) |
| `Data/` | `ApplicationDbContext` + `DbSeeder` |
| `Services/` | `TokenService` — signs/issues JWTs |
| `Migrations/` | EF Core migrations (PostgreSQL) |

### Key endpoints

| Method | Route | Auth |
|--------|-------|------|
| `POST` | `/api/auth/register`, `/api/auth/login` | public |
| `GET`  | `/api/auth/me` · `PUT /api/auth/profile` · `POST /api/auth/change-password` | bearer |
| `GET`  | `/api/listings` · `/api/listings/{id}` | public |
| `GET`  | `/api/home` · `/api/options` · `POST /api/contact` | public |
| `GET`  | `/api/applications/mine` · `POST /api/applications` · `POST /api/applications/{id}/withdraw` | bearer |
| `GET`  | `/api/saved` · `POST /api/saved/{id}/toggle` | bearer |
| `*`    | `/api/admin/**` (dashboard, listing CRUD, application review) | bearer + Administrator |

---

## 5. Authentication (server-side)

1. `POST /api/auth/login` verifies the password against the **PBKDF2 hash** stored by ASP.NET Identity
   (with a 5-attempt lockout).
2. On success the server signs a **JWT** (`TokenService`) containing the user id, email and roles.
3. The browser sends it as `Authorization: Bearer <token>` on later requests.
4. The server **validates the signature/expiry** on every request and enforces `[Authorize]` /
   `[Authorize(Roles = "Administrator")]` — the client cannot grant itself access.

---

## 6. Common tasks

```bash
dotnet build                              # compile only
dotnet ef migrations add <Name>           # after changing Models/ (needs dotnet-ef)
dotnet ef database update                 # apply migrations manually (the app also does this on start)
```

**Reset the database** (wipes all data, re-seeds on next run):

```bash
psql -d postgres -c "DROP DATABASE homebridge;"
psql -d postgres -c "CREATE DATABASE homebridge OWNER homebridge;"
```

---

## 7. Troubleshooting

| Symptom | Fix |
|---------|-----|
| `dotnet: command not found` | .NET SDK not installed / not on PATH — see §1, then open a new terminal. |
| `Npgsql ... 28P01 password authentication failed` | The role/password don’t match the connection string. Re-run §2, or edit `appsettings.json`. |
| `Npgsql ... 3D000 database "homebridge" does not exist` | You skipped §2 — create the database. |
| `Failed to bind to address http://localhost:5180 (address in use)` | Another instance is running. Stop it: macOS/Linux `lsof -nP -iTCP:5180 -sTCP:LISTEN -t \| xargs kill`; Windows `netstat -ano \| findstr :5180` then `taskkill /PID <pid> /F`. |
| `pg_isready` says “no response” | PostgreSQL isn’t running — start it (§1). |
| Frontend shows network/CORS errors | Make sure this API is running on `:5180`. CORS already allows `http://localhost:5173`; if you run the frontend on another port, add it to `Cors:AllowedOrigins` in `appsettings.json`. |
| `permission denied for schema public` (Postgres 15+) | Grant it: `psql -d homebridge -c "GRANT ALL ON SCHEMA public TO homebridge; ALTER SCHEMA public OWNER TO homebridge;"` |
