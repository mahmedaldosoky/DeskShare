# DeskShare

A desk booking app. Employees book a desk for a day; office managers manage the desks and see all bookings.

**Stack:** ASP.NET Core (.NET 10) · EF Core + SQLite · React + TypeScript + SCSS (Vite) · OpenID Connect SSO

## Features

| Role | Can do |
| --- | --- |
| Employee | See free desks for a date, book one, see and cancel their upcoming bookings |
| Office Manager | Everything above, plus create, edit and delete desks, and see all bookings for a date |

## Run it

Requires the .NET 10 SDK and Node.js 20+.

```bash
# API (creates and seeds the SQLite database on first run)
dotnet run --project src/DeskShare.Api --launch-profile http

# Frontend, in a second terminal
cd src/DeskShare.Web
npm install
npm run dev
```

Open http://localhost:5173.

By default the app uses a **development sign-in** form, so it runs without an identity provider. Pick "Sara Employee" or "Omar Manager".

## Single sign-on

The backend signs users in with OpenID Connect (authorization code + PKCE) and keeps the session in an HttpOnly cookie, so the React app never handles tokens. Members of the `DeskShare-OfficeManagers` group get the Office Manager role.

It is tested with **Auth0** and works with any OIDC provider (ADFS, Entra ID, Keycloak) through configuration only. To turn it on, store the settings as user secrets:

```bash
cd src/DeskShare.Api
dotnet user-secrets set "Authentication:Mode" "Oidc"
dotnet user-secrets set "Authentication:Authority" "https://<tenant>.auth0.com/"
dotnet user-secrets set "Authentication:ClientId" "<client id>"
dotnet user-secrets set "Authentication:ClientSecret" "<client secret>"
dotnet user-secrets set "Authentication:GroupsClaim" "https://deskshare.app/groups"
```

Restart the API and the login page shows **Sign in with your company account** instead of the development form. To go back to the development form:

```bash
dotnet user-secrets remove "Authentication:Mode"
```

### Auth0 setup

1. Create a **Regular Web Application** with callback URL `http://localhost:5173/signin-oidc` and logout URL `http://localhost:5173/signout-callback-oidc`.
2. Create a role named `DeskShare-OfficeManagers` and assign it to managers.
3. Add a **Post-Login Action** that sends the roles to the app:

   ```js
   exports.onExecutePostLogin = async (event, api) => {
     api.idToken.setCustomClaim('https://deskshare.app/groups', event.authorization?.roles ?? []);
   };
   ```

### Other providers

Providers name their claims differently. Set these keys in the `Authentication` section to match:

| Provider | Authority | EmailClaim | NameClaim | GroupsClaim |
| --- | --- | --- | --- | --- |
| Auth0 | `https://<tenant>.auth0.com/` | `email` | `name` | `https://deskshare.app/groups` |
| ADFS | `https://<adfs-host>/adfs` | `upn` | `unique_name` | `group` |
| Entra ID | `https://login.microsoftonline.com/<tenant-id>/v2.0` | `preferred_username` | `name` | `groups` |

`OfficeManagerGroup` sets which group grants the Office Manager role (default `DeskShare-OfficeManagers`). Entra ID sends group IDs, so use the group's Object ID there.

## Tests

```bash
dotnet test
```

A Postman collection with one request per endpoint is in [postman/](postman/DeskShare.postman_collection.json).

## Project structure

| Project | Purpose |
| --- | --- |
| `DeskShare.Domain` | Entities: `Desk`, `Booking`, `Employee` |
| `DeskShare.Application` | Use cases, DTOs and repository interfaces |
| `DeskShare.Infrastructure` | EF Core, repositories, migrations |
| `DeskShare.Api` | Controllers, authentication, error handling |
| `DeskShare.Web` | React frontend |
