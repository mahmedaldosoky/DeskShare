# DeskShare

Employees sign in with their company account and book a desk for a day. Office managers manage the desks and see who sits where.

**Stack:** ASP.NET Core (.NET 10) · EF Core + SQLite · React 19 + TypeScript + SCSS (Vite) · OpenID Connect SSO (ADFS / Entra ID / Keycloak)

## Running it

Prerequisites: .NET 10 SDK and Node.js 20+.

```bash
# 1. API: creates and seeds deskshare.db on first run
dotnet tool restore
dotnet run --project src/DeskShare.Api --launch-profile http      # http://localhost:5266

# 2. Frontend (second terminal)
cd src/DeskShare.Web
npm install
npm run dev                                                       # http://localhost:5173
```

In Development the app uses a **development sign-in** form (pick "Sara Employee" or "Omar Manager"), so it runs without an identity provider. See [Single sign-on](#single-sign-on) to switch to real SSO.

For a production-style run, `npm run build` writes the SPA into `src/DeskShare.Api/wwwroot` and the API serves everything from one origin.

```bash
dotnet test        # unit tests + API integration tests
```

### Postman

Import [postman/DeskShare.postman_collection.json](postman/DeskShare.postman_collection.json) while the API is running. It has one request per endpoint: send "Sign in" first, then any other request. Run top to bottom, it creates a desk, books it, and cleans up after itself. It also runs headless:

```bash
npx newman run postman/DeskShare.postman_collection.json
```

## Features

| Role | Can do |
| --- | --- |
| Employee | See free desks for a date, book one, list upcoming bookings, change date/desk, cancel |
| Office Manager | Everything above, plus desk CRUD (code, floor, features) and all bookings for a date |

Business rules: one booking per desk per day; one desk per employee per day; no past dates; employees only change their own bookings; desks with upcoming bookings can't be deleted. Deleted desks are soft-deleted, so past bookings keep their desk and the code can be reused.

## Architecture

Clean Architecture: dependencies point inwards, so business rules don't depend on frameworks.

```
DeskShare.Api ──► DeskShare.Application ──► DeskShare.Domain
      │                    ▲
      └──► DeskShare.Infrastructure
```

| Project | Responsibility |
| --- | --- |
| `Domain` | Plain entities (`Desk`, `Booking`, `Employee`) holding data and simple behaviour (normalise a desk code, reschedule, soft delete). They never throw. All enums live in `Enums.cs` and all field limits (lengths, floor range) in `Constants.cs`. No dependencies. |
| `Application` | Use cases (`DeskService`, `BookingService`, `EmployeeProvisioningService`), DTOs with their AutoMapper `MappingProfile`, and the ports it needs (`IDeskRepository`, `IUnitOfWork`, `ICurrentUser`...). |
| `Infrastructure` | EF Core `DbContext`, entity configurations, repositories, migrations. Implements the Application ports. |
| `Api` | Thin controllers, authentication/authorization, mapping exceptions to HTTP problem responses. Composition root. |
| `Web` | React SPA: typed API client, feature folders, SCSS design tokens and mixins. |

### SOLID in practice

- **Single responsibility.** `BookingService` owns the booking use cases. `UnitOfWork` only commits and translates DB constraint errors. `DeskSharePrincipalFactory` only builds the cookie principal. `MappingProfile` is the one place entities become DTOs.
- **Open/closed.** A new identity provider is configuration (claim names, authority), not code. A new error type is one line in `ApplicationExceptionHandler`.
- **Liskov.** Unit tests swap EF repositories for in-memory ones and the services behave the same.
- **Interface segregation.** `ICurrentUser` exposes only `EmployeeId`. Each repository has only the queries its use cases need.
- **Dependency inversion.** Application owns the interfaces and Infrastructure implements them. Time comes from `TimeProvider`, so "today" is testable. Interfaces exist only where there is a real second implementation (EF vs. in-memory fakes, HTTP vs. fake user); use-case services are injected directly.

### Enforcing the rules

Rules are checked in three layers:

1. **Request:** validation attributes on the request DTOs (desk code length, floor range) return a 400 listing every invalid field. The JSON converter rejects unknown desk features.
2. **Application:** `BookingDateRules` rejects past dates (400). `BookingService` checks desk/day and employee/day and returns a friendly 409.
3. **Database:** unique indexes on `(DeskId, Date)` and `(EmployeeId, Date)` guarantee the rules even when two requests race. `UnitOfWork` turns the violation into the same 409.

### Operational log

Unexpected exceptions (the ones that become a 500) are recorded in the `OperationalLogs` table by `OperationalLogExceptionHandler`. Expected business answers (`BusinessRuleException`, `ConflictException`, `NotFoundException`, `ForbiddenException`) are skipped: "desk already booked" is normal behaviour, and logging it would bury real bugs. Requests cancelled by the client are skipped too.

- **No duplicates.** A log's primary key is a fingerprint (SHA-256 of the exception type plus the method that threw it), so the same error always maps to the same row. Repeats increase `OccurrenceCount` and refresh `LastOccurredAtUtc`, the message, the stack trace and the request path. Messages are not part of the fingerprint, so the same failure with different ids or values in its message still counts as one error.
- **Safe under concurrency.** `OperationalLogRepository` writes with a single `INSERT ... ON CONFLICT DO UPDATE` statement, so two requests failing at the same moment cannot create two rows.
- **Never hides the real error.** The log is written in its own scope with a fresh `DbContext`. If writing it fails, that failure goes to the regular logger and the client still gets the original error response.

## Single sign-on

The API uses the **BFF (backend-for-frontend) pattern**:

- The ASP.NET backend runs the OpenID Connect authorization-code flow (with PKCE) against the identity provider and issues an HttpOnly, SameSite cookie.
- The SPA never handles tokens.
- On first login an `Employee` profile is created from the name and email claims. Later logins keep it in sync.
- Membership of a configured group grants the `OfficeManager` role, so access is managed in the directory.

Configure it in `appsettings.json`, or with user secrets for the client secret:

```jsonc
"Authentication": {
  "Mode": "Oidc",
  "OfficeManagerGroups": [ "DeskShare-OfficeManagers" ],
  "Oidc": {
    "Authority": "https://adfs.contoso.com/adfs",
    "ClientId": "<client id>",
    "ClientSecret": "<secret>",               // dotnet user-secrets set "Authentication:Oidc:ClientSecret" "..."
    "ClaimTypes": { "Subject": "sub", "Email": "upn", "Name": "unique_name", "Groups": "group" }
  }
}
```

| Provider | Notes |
| --- | --- |
| **ADFS 2016+** | Add an *Application Group* → "Server application accessing a web API". Redirect URI `https://<host>/signin-oidc`. Add issuance rules that send `upn`, `unique_name` and group names as `group`. Use the claim types shown above. |
| **Microsoft Entra ID** | Authority `https://login.microsoftonline.com/<tenant>/v2.0`. Redirect URI `https://<host>/signin-oidc`. Enable the *groups* claim (or app roles: set `Groups` to `roles`). Default claim types work, with `Email` set to `preferred_username` if the `email` claim is empty. |
| **Keycloak** | Authority `https://<keycloak>/realms/<realm>`. Add a *Group Membership* mapper named `groups`. |

When running through the Vite dev server, use `http://localhost:5173/signin-oidc` as the redirect URI. Vite proxies it to the API.

## Trade-offs and next steps

- **SQLite** keeps setup at zero. Moving to SQL Server/PostgreSQL means changing the EF provider, the unique-constraint error check in `UnitOfWork` and the filtered index on `Desks.Code`. The operational-log upsert (`ON CONFLICT`) works as-is on PostgreSQL; SQL Server would need a `MERGE` statement.
- **AutoMapper** maps entities to DTOs by convention (`Booking.Desk.Code` → `DeskCode`), and a unit test validates the configuration. v15+ is commercially licensed: set the `AUTOMAPPER_LICENSE_KEY` environment variable (free community keys are available). Without a key it runs and logs a notice, which the license allows for development and testing only.
- **"Today"** uses the server's time zone. A multi-site company would store a time zone per office.
- **Migrations run on startup** for convenience. In production they would run from the deployment pipeline.
- **Cross-aggregate navigations** (`Booking.Desk`) keep EF queries simple. A stricter DDD model would reference by id only.
