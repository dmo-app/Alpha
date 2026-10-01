# DMO — Current Implementation Status

This file records the current implementation state of the Beta.

It is **not product canon**. Functional truth belongs in `HOW_THE_APP_WORKS.md`, `IDENTITIES.md`, `RULES.md`, `SCOPE.md`, and the detailed module files.

This file exists so an implementation agent can distinguish:

- canonical product behaviour;
- functionality already implemented;
- functionality still missing;
- transitional Beta conditions;
- current defects;
- environment/integration limitations.

A current defect or missing implementation must never be promoted into product behaviour.

## Proven runtime baseline

The current Beta has been verified to:

- build successfully;
- apply its current EF Core migrations to a clean PostgreSQL database;
- start successfully;
- run against PostgreSQL.

## Current product-code defect

### Login redirect

Unauthenticated access to a protected route challenges to:

`/Account/Login`

That route currently returns 404 because `CookieAuthenticationOptions.LoginPath` is not configured for the actual login route.

This is a defect, not intended product behaviour.

## Current implementation gaps

### Controlo identity

`controlo_id` is a canonical product identity.

The currently inspected schema/application model does not yet persist that identity.

Resumo is currently composed from `jobon_id`; this implementation state must not be treated as evidence against `controlo_id`.

The implementation shape of `controlo_id` must follow the current blueprint and must not invent parentage for all Controlo function records.

### Module availability / routes

The current build has:

`ModuleRegistrations.CurrentBuildAvailable = []`

Operational modules therefore resolve as unavailable at runtime.

This is transitional implementation state. The availability/route registration work remains to be completed.

### Live Supabase authentication

The authentication architecture exists, but end-to-end live Supabase authentication has not yet been proven with real credentials in the verified runtime session.

This is an integration/test status, not a product rule.

### Pegamentos

The functional blueprint is defined.

The current inspected application does not yet contain the production backend/persistence implementation for Pegamentos.

Prototype/browser-only behaviour must not be promoted into backend product rules.

### Folha

The functional blueprint is defined.

The persisted Folha evaluation layer is not yet implemented in the inspected application state.

### Comparação UI

The Comparação backend/domain/persistence path was reported implemented, while the user-facing UI remained unfinished in the inspected baseline.

### Documents / PDF configuration

Peso PDF generation/storage/send support exists.

The inspected environment did not have the required base-directory configuration row seeded, so document operations can report the workspace as not configured.

That environment state is not product behaviour.

### Email transport

SMTP transport support exists but may be unconfigured in the runtime environment.

An unconfigured transport must produce the defined typed refusal/outcome rather than becoming a functional product rule.

## Blueprint additions from 2026-09-29 awaiting implementation verification

The blueprint was materially expanded on 2026-09-29.

Everything in this section must be checked against the chosen current application baseline before being marked implemented. If the application already matches the blueprint, record that as verified. If it does not, implementation work remains.

The blueprint itself defines current product behavior. Commit wording, historical code, and previous implementation must not be used to restore outdated behavior.

### Peso / Tool technical values

Verify the current application against the present blueprint for:

- Peso calculation inputs and their historical stability;
- Tool-owned technical values consumed by Peso through the correct owner paths: `volume_puncao`/`peso_nominal` from CM and `volume_marisa` from BQ;
- the current ownership boundary between Tool facts and production/Controlo facts;
- Duplicate Tool creating a new canonical Tool identity while using the source only as a starting point.

### Job On duplication

Verify the current application against the present Job On duplication rules, including:

- explicit source selection;
- creation of a new production identity;
- carrying Tool selection by canonical Tool identity;
- creation of new production-context identities for the new production;
- source production remaining unchanged;
- opening the newly created Job On in the appropriate editable workflow.

Only the current Job On capability model is valid during this verification.

### Boquilhas

Verify the current application against the present Boquilhas blueprint for:

- preservation of existing Boquilhas register data that already represents real operational history;
- canonical `bq_repair_trace_id` as the single movement trace for one `bq_id` / production BQ context;
- movements belonging to the repair trace rather than being attached as one flat lifetime movement list directly to `bq_id`;
- pre-production traces anchored to canonical BQ `tool_id` where applicable, with `bq_id` initially unresolved;
- at most one unresolved pre-production trace per BQ `tool_id` at a time; later pre-production movements reuse it;
- automatic association of the same pending trace when that BQ Tool is explicitly selected in Job On; the same trace is kept, `bq_id` is set, and the temporary direct `tool_id` anchor is cleared without moving existing movements;
- multiple repair movement cycles belonging to the same production trace rather than creating one trace per repair trip;
- a new production/BQ context using a new trace even when it references the same physical BQ `tool_id`;
- machine-side current-production context;
- independence of a repair trace from machine production changes;
- current-production registration shortcut;
- independent access to old, non-current and pre-production repair traces;
- preservation of the same repair-trace identity when later associated with production;
- fixed BQ Tool quantity as the lot base, not a Boquilhas-owned mutable total;
- automatic machine→repairer resolution from Boquilhas Definições;
- exceptional Saída/Entrada recording without hard blocking;
- Entrada-only discrepancy/Saldo semantics;
- latest-movement-only correction/removal ordering;
- module-local Beta consultation with no Boquilhas PDF/email artifact.

Any remaining semantic question must be resolved from the current blueprint or owner confirmation before implementation is changed.

### Admin / external authentication boundary

Verify and implement where required:

- ADMIN association to an already-existing external Supabase Auth identity;
- Blank State setup;
- ADMIN reassociation without recreating operational data;
- prohibition on application-created ADMIN authentication identities or automatic first-user promotion.

### Controlo canonical context

Verify and implement the canonical Controlo context and `controlo_id` according to the current blueprint.

This includes identifying the facts that genuinely belong to the Controlo production context without turning `controlo_id` into a generic parent for every Controlo function record.

### Controlo functional blueprint documented today

The detailed blueprint content documented today for these areas must be checked against the application baseline and implemented where missing:

- Resumo;
- Peso;
- Comparação;
- Pegamentos;
- Folha;
- Definições;
- the current identity relationships shared by those functions.

Verification must use the current blueprint, not an older implementation as the definition of expected behaviour.

### Access Templates

Blueprint coverage for Access Templates is still incomplete.

Before implementation work relies on Templates, the current product behaviour must first be fully captured in the blueprint. After that, compare the application to that completed blueprint and implement the missing behaviour.

Do not treat demonstration templates, prototype storage, or historical profile concepts as canonical merely because they exist in older material.

### Completion rule for this section

An item leaves this queue only after one of these outcomes is recorded:

- **VERIFIED_IMPLEMENTED** — the selected application baseline already matches the current blueprint;
- **IMPLEMENTED** — the application was changed and verified to match the current blueprint;
- **BLOCKED_BY_BLUEPRINT_GAP** — the current blueprint is still insufficient to implement safely.

Do not mark an item complete merely because related code exists.

## Rule for future updates

When a product decision is closed, update the canonical blueprint file where that rule belongs.

When an implementation defect, gap, or transitional condition is discovered, update this file.

Do not keep unresolved product-question queues inside the canonical blueprint. Once the owner decides a product rule, write it directly into the owning blueprint file.
