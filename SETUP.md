# Local Setup

Quick reference for getting the app running after a fresh `git clone` / `git pull`.

## Prerequisites

- .NET 10 SDK
- One of:
  - **SQL Server LocalDB** (ships with Visual Studio), or
  - Nothing extra — use the **SQLite** path, which creates `RealEstate.db` on first run

## First-time setup

1. Clone the repo and open the solution in Visual Studio, or `cd Real_Estate_App`.
2. Create a file named **`appsettings.Development.json`** next to `appsettings.json`. This file is in `.gitignore` so your secrets stay local. See the next section for what to put in it.
3. Build and run. Visual Studio: F5. CLI: `dotnet run` from `Real_Estate_App/`.

The DB schema is created automatically on first run (no `Update-Database` needed unless you're changing the schema).

## `appsettings.Development.json` — what to put in it

This file **overrides** values from `appsettings.json` when the app runs in Development mode. You only need to include the keys you want to change.

### Minimum (if the committed `appsettings.json` defaults already match your machine)

If you're on SQL Server and only need to enable email, this is enough:

```json
{
  "SmtpSettings": {
    "Host": "smtp.gmail.com",
    "Port": "587",
    "SenderEmail": "your.gmail@gmail.com",
    "SenderPassword": "<16-char Gmail app password>",
    "SenderName": "Real Estate App"
  }
}
```

### Full example — SQLite path

If you'd rather run against SQLite (no SQL Server install needed):

```json
{
  "Logging": {
    "LogLevel": { "Default": "Information", "Microsoft.AspNetCore": "Warning" }
  },
  "DatabaseProvider": "Sqlite",
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=RealEstate.db"
  },
  "SmtpSettings": {
    "Host": "smtp.gmail.com",
    "Port": "587",
    "SenderEmail": "your.gmail@gmail.com",
    "SenderPassword": "<16-char Gmail app password>",
    "SenderName": "Real Estate App"
  }
}
```

### Full example — SQL Server path

`appsettings.json` already defaults to `"DatabaseProvider": "SqlServer"` and ships a LocalDB connection string, so most people don't need to override anything DB-related. If you want to point at a different SQL Server instance:

```json
{
  "DatabaseProvider": "SqlServer",
  "ConnectionStrings": {
    "DefaultConnectionSQL": "Server=(localdb)\\mssqllocaldb;Database=Real_Estate_SQLServer_DB;Trusted_Connection=true;MultipleActiveResultSets=true"
  },
  "SmtpSettings": {
    "Host": "smtp.gmail.com",
    "Port": "587",
    "SenderEmail": "your.gmail@gmail.com",
    "SenderPassword": "<16-char Gmail app password>",
    "SenderName": "Real Estate App"
  }
}
```

## Gmail app password

The `SenderPassword` is **not** your normal Google account password. It's a 16-character app-specific password.

1. Go to **Google Account → Security → 2-Step Verification** (must be enabled first).
2. Scroll to **App passwords**.
3. Generate one named e.g. "Real Estate App".
4. Paste the 16-char value into `SenderPassword`. Spaces are fine — the SMTP library strips them.

If `SenderEmail` / `SenderPassword` are left blank, the app **does not crash** — it logs a warning (`SMTP sender credentials are not configured. Skipping…`) and the confirmation page shows an "unable to send" notice instead of the usual "Email Sent!" banner.

## Choosing the database provider

`DatabaseProvider` in config controls which backend is used. Supported values: `"SqlServer"` (default) or `"Sqlite"`.

- **SQL Server path** reads `ConnectionStrings:DefaultConnectionSQL`.
- **SQLite path** reads `ConnectionStrings:DefaultConnection`.

The app auto-creates the schema at startup for both providers, so local dev doesn't need `Add-Migration` / `Update-Database` unless you're actually modifying models.

## Common gotchas

- **"The value cannot be an empty string. (Parameter 'address')" on checkout** → `SenderEmail` is blank. Fill in `appsettings.Development.json`.
- **Missing `appsettings.Development.json`** → it's gitignored on purpose; you need to create it yourself after a fresh clone.
- **SQL Server login errors** → make sure LocalDB is installed, or switch `DatabaseProvider` to `"Sqlite"` for a dependency-free run.
- **Migrations out of sync** → the app's auto-create covers a fresh DB; if you already have an older schema lying around, delete `RealEstate.db` (SQLite) or drop the LocalDB database and restart.

## Who owns what

See [Objectives.md](Objectives.md) for the feature split between Justin (properties / search / checkout / email / security) and Ravi (users / admin / login / viewing / cookies / migrations).
