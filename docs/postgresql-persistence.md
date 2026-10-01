# Alpha durable PostgreSQL foundation

Status: **foundation ready.** The first EF Core migration (`20261001185228_InitialCreate`)
represents the **current** `DmoDbContext` model exactly. It was generated from the
model as implemented (no entity redesign, no new fields, no semantic changes) and
verified with `dotnet ef migrations has-pending-model-changes` → *No changes have
been made to the model since the last migration.*

## What the migration creates

Exactly the current model, in the default `public` schema:

| Table | Key constraints (as currently encoded in EF) |
| --- | --- |
| `Users`, `AdminAssociations` | existing identity/auth persistence; unique `OperatorId` (max 4), unique `Email` |
| `Machines` | PK `Id`; `Code` varchar(50) required |
| `JobOns` | PK `Id`; `MachineId` FK → `Machines` required (cascade); unique `ProductionNumber` |
| `CmContexts` / `MfContexts` / `BqContexts` | PK `Id`; `JobOnId` FK → `JobOns` required (cascade) **+ unique index** (one context per Job On); `ToolId` varchar(100) anchor, no FK |
| `Tools` | PK `ToolId` (text, backend-issued identity); `Type` text tokens `CM`/`MF`/`BQ`; no uniqueness on Reference/Lot |
| `BqRepairTraces` | PK `Id`; `ToolId` FK → `Tools` required (cascade, permanent anchor); `BqContextId` FK → `BqContexts` **optional** (null = unresolved bq_id, no cascade); **unique index on `BqContextId`** (one trace per bq_id); non-unique index on `ToolId` (pending-trace cardinality deliberately open) |
| `BqMovements` | PK `Id`; `BqRepairTraceId` FK → `BqRepairTraces` required (cascade); `Type` text tokens `saida`/`entrada`/`entrada_sem_reparacao`; `Quantity` required; `Discrepancy` nullable |

Movement type storage is unchanged: canonical text tokens, never integer enums.

## Configuring PostgreSQL (no credentials committed)

The connection string is read from configuration key `ConnectionStrings:DmoDatabase`.
Any standard configuration source works; nothing is committed to the repository:

```powershell
# environment variable (recommended for servers/CI)
$env:ConnectionStrings__DmoDatabase = "Host=...;Database=...;Username=...;Password=..."
# Supabase direct or pooled connection strings are accepted as-is
```

Local alternatives: `dotnet user-secrets` (set a `UserSecretsId` in the Web project
first) or the gitignored `src/DMO.Alpha.Web/appsettings.Development.json`.

### Runtime policy

- Connection string configured → PostgreSQL (`UseNpgsql`, migrations assembly
  `DMO.Alpha.Infrastructure`).
- No connection string **in Development** → the explicit local InMemory seam
  (`DmoAlphaModule1`). Unchanged local-dev workflow; tests construct their own
  InMemory contexts.
- No connection string **outside Development** → the application **fails fast at
  startup** with instructions to set `ConnectionStrings__DmoDatabase`. A deployed
  environment never silently pretends in-memory persistence is durable.
  Proven by `DMO.Alpha.Web.Tests.PersistencePolicyTests`.

`AddDmoInfrastructure` follows the same policy: no connection string is a
configuration error, not an InMemory fallback.

## EF tooling

The startup project is `DMO.Alpha.Web`; migrations live in `DMO.Alpha.Infrastructure`
(see `DmoDbContextFactory`, the design-time factory). Generating or scripting
migrations never connects to a database.

```powershell
$env:DOTNET_ROOT = "C:\Users\vk_do\.dotnet"   # user-level .NET install: ef's apphost needs it

dotnet ef migrations list --project src\DMO.Alpha.Infrastructure --startup-project src\DMO.Alpha.Web
dotnet ef migrations has-pending-model-changes --project src\DMO.Alpha.Infrastructure --startup-project src\DMO.Alpha.Web
dotnet ef migrations script --project src\DMO.Alpha.Infrastructure --startup-project src\DMO.Alpha.Web --output temp\InitialCreate.sql
dotnet ef migrations add <Name> --project src\DMO.Alpha.Infrastructure --startup-project src\DMO.Alpha.Web
```

Note: on machines whose only installed runtime is .NET 10 (user-level install),
`dotnet ef` may fail with *"You must install .NET to run this application"* — the
global tool's apphost is an 8.0.30 shim. Setting `DOTNET_ROOT` as above fixes it;
the direct fallback is running the tool DLL:
`dotnet "$env:USERPROFILE\.dotnet\tools\.store\dotnet-ef\10.0.12\dotnet-ef\10.0.12\tools\net8.0\any\dotnet-ef.dll" ...`

## Applying the migration to a real database

No PostgreSQL/Supabase target is configured in the current environment, so the
migration has been **generated and validated only** (schema-frozen SQL verified in
`MigrationFoundationTests`). To apply it, a real database connection is needed:

1. Point `ConnectionStrings__DmoDatabase` at the target database (Supabase direct
   or pooled connection string). Use a role allowed to create the tables above.
2. Apply: `dotnet ef database update --project src\DMO.Alpha.Infrastructure --startup-project src\DMO.Alpha.Web`
   (or review/apply `dotnet ef migrations script` SQL manually).
3. Verify: `dotnet ef migrations list ...` shows `20261001185228_InitialCreate` as applied.
4. Optional live proof: point `DMO_POSTGRES_CONNECTION` at a **disposable test
   database** and run the `PostgresMigrationIntegrationTests` (they apply the
   migration, verify the canonical schema in `pg_indexes`/`information_schema`,
   exercise the create/move/associate write path and clean up after themselves;
   they skip while the variable is unset).

## Tests

| Suite | Scope |
| --- | --- |
| `DMO.Alpha.Infrastructure.Tests/Data/MigrationFoundationTests` | offline, always runs: single initial migration; snapshot vs current model zero-diff; operation-level canonical constraints; generated PostgreSQL SQL for all tables/FKs/unique indexes; token text storage |
| `DMO.Alpha.Infrastructure.Tests/Data/PostgresMigrationIntegrationTests` | live PostgreSQL (skippable via `DMO_POSTGRES_CONNECTION`) |
| `DMO.Alpha.Web.Tests/PersistencePolicyTests` | runtime persistence policy (fail-fast vs durable Npgsql) |

InMemory remains only where tests explicitly construct it; all pre-existing
InMemory-based Registo/trace/movement tests are unchanged and green.
