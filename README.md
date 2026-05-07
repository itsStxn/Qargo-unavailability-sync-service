# Qargo Unavailability Sync Service

**Purpose** – Synchronises resource unavailabilities from a master Qargo environment into a target Qargo tenant.
Only entries that fall in the year **2025** are considered.

---

# Table of Contents

1. Architecture Overview
2. Key Design Patterns
3. Assumptions
4. Prerequisites
5. Configuration (.env)
6. Building & Running
7. Error Handling & Logging
8. Security Considerations
9.  Extending the Tool
11. License

---

# 1. Architecture Overview

```
Program.cs
│
└── Context  → shared HttpClient, Serilog logger, .env loader
    ├── EnvSource → loads environment variables (DotNetEnv)
    └── Cancellation Token (10s timeout)
│
└── Tenant (abstract)
    ├── MyRequest      → base HTTP request (Template Method)
    └── MyAuthRequest  → OAuth2 client-credentials handling
│
└── QargoService
    → reads resources & unavailabilities from target tenant
    → maps only entries with ExternalId
│
└── MasterService
    → reads resources & unavailabilities from master tenant
    → builds action map (create / update)
│
└── InteractService
    → orchestrates sync
       ├── QargoToMaster()
       └── SyncUnavailabilities()
```

---

# 2. Key Design Patterns

| Pattern                    | Where it appears             | Purpose                                                                    |
| -------------------------- | ---------------------------- | -------------------------------------------------------------------------- |
| Template Method            | `MyRequest`, `MyAuthRequest` | Defines HTTP request flow while allowing customization (auth, retry logic) |
| Dependency Injection       | All services                 | Constructor-based DI for testability and decoupling                        |
| Retry / Resilience (Polly) | `MyRequest`                  | Exponential backoff retry for network failures                             |
| Command-like Action Map    | `UActions`                   | Collects create/update operations for idempotent sync                      |
| Facade                     | `InteractService`            | Single entry point hiding internal complexity                              |

---

# 3. Assumptions

* Only **unavailability data** is synchronized; resource entities themselves remain unchanged.
* Synchronisation direction is strictly **Master → Qargo**, where the master system is the source of truth and Qargo is the target.
* The master environment is treated as **read-only** for this integration; only **GET operations** are performed against it.
* Any **orphan unavailability entries** in Qargo (i.e. records with an `external_id` that does not exist in the master dataset) are intentionally preserved, as they may originate from other upstream systems.
* The `external_id` field is assumed to uniquely identify an unavailability within the scope of a resource.
* Cross-environment resource matching is based on **resource name**, not on internal system identifiers.
* Only unavailability records with a time window in **calendar year 2025** are considered relevant for synchronization.
* A missing `external_id` implies the record has **no external provenance** and is not part of the master-managed dataset.
* Pagination is implemented using a cursor-based mechanism and is considered complete when either:

  * the returned `cursor` is `null`, or
  * the returned `items` collection is empty.
* When using cursor-based pagination, additional query parameters are avoided to ensure consistency with API expectations and prevent unstable paging behavior.
* The `external_id` in Qargo is assumed to be equivalent to the **master record identifier**, enabling deterministic matching between systems.
* Database and API operations are optimized for efficiency:

  * Only **update operations** are executed when a matching `external_id` exists *and* at least one relevant field differs
  * Redundant writes are intentionally avoided to reduce load and improve performance

---

# 4. Prerequisites

| Requirement           | Version / Notes                     |
| --------------------- | ----------------------------------- |
| .NET SDK              | 10.0                                |
| dotnet CLI            | Installed and on PATH               |
| dotnet-env package    | Auto-installed via project          |
| Environment variables | See configuration section           |
| Serilog               | Auto-installed via project          |
| Qargo tenant access   | Client ID + Secret for both tenants |

---

# 5. Configuration (.env)

Create a `.env` file in the repository root (ignored by git):

```env
# Target tenant (where data is written)
QARGO_CLIENT_ID=your_target_client_id
QARGO_SECRET=your_target_client_secret

# Master tenant (source of truth)
MASTER_CLIENT_ID=your_master_client_id
MASTER_SECRET=your_master_client_secret
```

Only these four variables are required and loaded at startup via `EnvSource`.

---

# 6. Building & Running

## Restore dependencies

```bash
dotnet restore
```

## Build

```bash
dotnet build -c Release
```

## Run

```bash
dotnet run --project "Qargo Unavailability Sync Service.csproj"
```

---

## What happens on execution

1. Logger initialization (console + daily file rotation in `Logs/`)
2. OAuth2 token acquisition (cached in `Cache/`)
3. Fetch resources from both tenants
4. Filter unavailabilities for year **2025**
5. Build create/update action map
6. Sync to target tenant
7. Graceful shutdown

---

## Optional publish

```bash
dotnet publish -c Release -r linux-x64 --self-contained false -o out
./out/Qargo\ Unavailability\ Sync\ Service
```

---

# 7. Error Handling & Logging

* Custom exceptions:

  * `AppException`
  * `NetworkException`
  * `ParseException`
  * `ConfigException`
  * `StreamException`
* All HTTP calls use **Polly retry policy**

  * max 5 retries, but can be chosen
  * exponential backoff
  * respects `Retry-After`
* Logging via **Serilog**

  * Fatal → unrecoverable errors
  * Warn → recoverable issues
* Logs:

  * Daily rotation
  * Retained for 3 days (`LOGS_TTL`, can be chosen)
* Non-zero exit code includes fatal log output for debugging

---

# 8. Security Considerations

* OAuth2 client-credentials flow (no hardcoded secrets)
* Secrets stored only in `.env`
* Access tokens cached separately (no client secret stored)
* `.env` and `Cache/` excluded from version control
* All requests over HTTPS (`https://api.qargo.com/v1/`)
* No string concatenation in URLs → prevents injection risks

---

# 9. Extending the Tool

### Add new resource types

* Extend DTOs under `ResourceListComponents/*`
* Update `QargoService.MapResources()`

### Change sync window

* Modify `Constants.UNAVAIL_YEAR`
* Or expose CLI argument

### Scheduling

* **Publish the service** using the Release build step described above (`dotnet publish`). This produces a self-contained executable (or framework-dependent binary, depending on configuration) in the output directory.

* **Execute the published binary manually** to validate it works outside of the build context. The service can be run directly from the publish folder by invoking its absolute or relative path, e.g.:

  ```bash
  ./out/Qargo\ Unavailability\ Sync\ Service
  ```

* **Integrate with cron for scheduled execution** (e.g. every 15 minutes) by referencing the absolute path of the published binary in a crontab entry. Since cron runs in a minimal shell environment, avoid relying on relative paths or environment assumptions.

* **Recommended cron pattern (robust execution):**
  Ensure the working directory is explicitly set before execution to avoid path-related issues (e.g., missing `.env`, logs, or cache directories):

  ```bash
  */15 * * * * cd /absolute/path/to/publish/dir && ./QargoUnavailabilitySyncService >> /var/log/qargo-sync.log 2>&1
  ```

* **Key considerations:**

  * Always use **absolute paths** in cron jobs.
  * Explicitly `cd` into the application directory so relative file dependencies (e.g., `.env`, `Logs/`, `Cache/`) resolve correctly.
  * Redirect `stdout` and `stderr` to a log file for observability and debugging.
  * Ensure the binary has execute permissions (`chmod +x` if needed).
  * If environment variables are required, either:

    * load them via `.env` inside the application (as implemented), or
    * explicitly source them in the cron command.

This approach ensures deterministic execution, avoids cron’s restricted execution context pitfalls, and keeps logging and configuration consistent with local runs.

---

# 10. License

The starter code is provided by **Qargo** for interview purposes.
You may modify, extend, or redistribute it for the assignment.
