# Rules

## Rule 1: Prototype behavior does not define domain truth

Prototype and design surfaces may validate presentation and interaction, but they do not define persistence, canonical identities, backend contracts, or industrial calculations.

- A prototype must not mint or derive canonical domain identities.
- A prototype must not become a second source of truth for backend-owned calculations or business rules.
- Prototype storage mechanics are implementation detail and do not define product behavior.
- When a backend workflow is implemented, its persistence and contracts must be derived from the current blueprint, not from demo or prototype mechanics.

## Rule 2: Explicit Human Choice

The system must never silently infer industrial decisions.

- If there are valid Tools, Job Ons, or historical Peso candidates that require a human choice, the UI must force the user to click and select one.
- **Even if a search returns only one result, explicit selection is required.** Auto-selection of a single candidate is forbidden.
- Pre-population is assistance only: known context (reference, machine, expected type) may pre-fill search criteria, but unknown Tool facts are never invented.
- Reference, optional MF-reference association, machine/line compatibility and lot are candidate-discovery attributes. They do not form a derived Tool identity key.
- Filters reduce candidates; they never determine identity. The canonical identity is the persisted `tool_id` of the Tool explicitly selected by the user.
- The frontend must never infer Tool identity from reference + lot + machine labels, even when those filters leave exactly one candidate.

## Rule 3: Warnings are not Decisions

Tolerance warnings, negative balances, stale comparisons and threshold alerts must be displayed to the user.

- The frontend must never automatically approve, reject, block, or clamp data based on a warning unless this repo explicitly demands blocking validation.
- **Approve never rejects on warnings and reject never auto-triggers**: status changes only per an explicit human action on the route.
- Decision commands carry no warning/result input and no code path derives a decision from calculation results.
- A warning is a gate that a human acknowledgement opens; it is never a hard prohibition and never a stored state.
- If a selected canonical Tool is already associated with another current machine/production context, the application may warn the user and identify that other context, but it must still allow explicit confirmation and continuation.
- Tool concurrency warnings must never be implemented as physical-exclusivity invariants, uniqueness constraints, hidden eligibility filters or blocking validation.

## Rule 4: Presentation does not define domain truth

The frontend may render, collect input, preserve local draft state and orchestrate published actions.

It must not:
- Mint canonical IDs.
- Invent persistence relationships.
- Infer Tool identity from labels or displayed text.
- Implement backend-owned formulas as a second source of calculation truth.
- Turn warnings into industrial decisions.
- Synthesize actor/time/audit facts.
- Infer permissions from role/profile labels.
- Create production APIs from fixtures.
- Treat display text as identity.

## Rule 5: Unsaved State Preservation

When a flow temporarily leaves a form to select/create a Tool or repair missing context:

- Preserve entered values.
- Preserve row identities.
- Cancel returns unchanged.
- Successful return applies only the explicitly selected/created result.
- The origin token is optional, opaque and echoed verbatim on every event; never parsed, trimmed, normalized, interpreted or embedded in a URL; never a canonical identity.

## Rule 6: Surgical Queries / Light Packets (Standing Rule)

Queries and read models must be context-specific and carry only the data the surface needs.

- No global scans.
- No loading whole relations/tables to filter or project in the frontend.
- No "load the universe and filter client-side".
- Backend reads are built per query contract with the smallest packet that satisfies the surface.
- On a filterable list/search surface, the frontend sends the user's filter/search criteria as query input; the backend applies those criteria before projecting the response and returns only the matching result packet.
- A frontend filter must not require loading the full dataset first merely so JavaScript can hide non-matching rows.
- Each module document defines which filter dimensions are valid for that surface; the global query rule defines where that filtering is executed.
- History is only loaded when the operator explicitly asks for it.

This rule binds every remaining and future Beta backend work.

## Rule 7: No Job On Lifecycle State Machine

No Job On-wide status, stage, phase or state machine exists.

- Specifically absent: `rascunho`, `planeado`, `em fabrico`, `fechado`, `cancelado`, `active`, `locked`, `approved` as Job On lifecycle states.
- Peso approval-status vocabulary is exactly three values: `por_aprovar` / `aprovado` / `nao_aprovado`.
- Saving a Peso before first submission does not create a separate `draft` status and does not place it in the approval queue.
- Explicit Submit places the Peso in `por_aprovar`; approve/reject are explicit human decisions; reopen returns it to editable work until it is submitted again.
- Warnings stay warnings; they never become stored lifecycle states.

## Rule 8: Backend owns canonical-ID allocation and attribution

- New canonical IDs are allocated only by the owning backend workflow/transaction.
- A client may carry or return an **existing** canonical ID that the backend already issued (for example an explicitly selected `tool_id` or an existing record route ID), but it must never mint, guess or derive a new canonical ID.
- This applies to canonical identities including `tool_id`, `jobon_id`, `cm_id`, `mf_id`, `bq_id`, `peso_id`, `comparacao_id`, `bq_repair_trace_id` and `movement_id`.
- The same rule applies to `controlo_id` when that canonical identity is implemented.
- Actor/time are backend facts (`ICurrentAccountContext`, backend clock) — never client-created audit facts.
- Audit trail never invents actor/time.

## Rule 9: Concurrency Discipline

- No silent retry, no last-write-wins, no automatic merge.
- One version increment per committed mutation; reads never bump.
- A refused mutation reports the typed reason and the surface reloads (`conflict` presentation).
- A stale edit writes nothing.
- Every guarded write carries `ExpectedVersion`; the database backstop and the application validator raise the same typed refusal.

## Rule 10: Module Boundaries

- **Controlo Create and Controlo Approve remain distinct modules.** No generic architecture may force their workflows/pages to be identical.
- A module owns its own workflow, rules and module-specific persistence.
- Modules must not reach directly into another module's internal tables or implementation.
- Preferred direction: `Controlo → shared contract / context provider → backend`.
- Forbidden: `Peso service → arbitrary Job On tables → arbitrary Armazém tables → arbitrary Boquilhas tables`.
- Modules meet through stable shared identities (`tool_id`, user/access identities, production context) or through explicit contracts.

## Rule 11: Fixed Desktop Layout

DMO is a fixed-layout desktop operational application, not a responsive public website.

- Canonical design and validation viewport: **1366 × 768**.
- Compact density; region-stable composition.
- Breakpoint-driven structural reflow is prohibited.
- No `@media` / `@container` / `@supports` structural rules in module CSS.
- Mobile and tablet layouts are out of scope.
- Local keyboard-reachable overflow for wide regions; scrolling is handled by the layout, not by script.

## Rule 12: HISTÓRICO LOCAL ≠ HISTÓRICO GLOBAL

- **HISTÓRICO LOCAL**: history capability that belongs to its operational module (for example Histórico de Pesos inside Controlo Approve).
- **HISTÓRICO GLOBAL** (technical identity `historia`): DEFERRED BY DESIGN in this Beta. No route, no availability, no top-level navigation entry.
- Local Histórico requirements remain owned by their modules and do not imply a global history module.

## Rule 13: No Duplication of Truth

- Job On production facts stay Job On truth. Controlo/Resumo outputs may read them but do not become a second persisted source for them.
- Tool identity and Tool-owned facts stay Tool truth, except for explicit historical snapshots owned by a real operational record.
- Do not store Peso status/attribution as a second persisted source on another record.
- PDF bytes/filename/path are derived output, not stored identity.
- No convenience column or FK is added merely to make navigation easier when an existing real relation already expresses the domain.

## Rule 14: Ferramentas is Contextual

- Ferramentas is the canonical Tool registry and consultation flow for the Beta, but it is not exposed as a top-level operational destination merely for convenience.
- Its primary surface is the existing Tool list/search/filter view; Tool creation is an action inside that registry.
- Search and filters narrow candidates but never auto-select a Tool.
- The origin module never becomes Tool owner: neither Job On nor Controlo nor Boquilhas creates private Tool identities or writes a private Tool registry.
- Tool create returns the canonical `tool_id` to the consuming workflow.
- There is one canonical Tool registry.
- Specialized `tool_technical_values` are optional Tool-owned data keyed by `tool_id` and are loaded only on demand; they do not create another Tool identity.

## Rule 15: Record ≠ PDF ≠ File Path

- `database record != generated PDF != file path`.
- Use the planned final directory convention from the start:

```text
<reference>/
└── <production-number>/
    ├── Peso_<reference>_<machine>.pdf
    ├── Pegamentos_<reference>_<machine>.pdf
    └── Resume_<reference>_<machine>.pdf
```

- Path/filename is never used as a join key.
- Document access is gated by the owning workflow/action permission — no separate document authorization model.
- No artificial document identity is created for symmetry.

## Rule 16: External Auth owns ADMIN identity creation

- Supabase Auth owns the existence and credentials of the authentication identity used as DMO ADMIN.
- DMO may associate an **existing** Supabase Auth user as ADMIN, but DMO must never create that Auth user itself.
- No setup page, bootstrap, seed, migration, recovery path or convenience endpoint may create the Supabase Auth identity that will become ADMIN.
- ADMIN must never be assigned merely because someone is the first user, because the database is empty, or because the previous ADMIN no longer resolves.
- Blank State / Setup is an installation/configuration mode, not an authenticated ADMIN identity.
- If the currently associated ADMIN Auth user is deleted externally, the existing DMO installation and data remain valid; the ADMIN association may be repaired through Setup against the same infrastructure using another Auth user that was first created externally in Supabase.
- Reassociating ADMIN must not recreate, reset or rewrite operational data.

## Rule 17: Job On Context Change Awareness

When a production context used by another module changes while the `jobon_id` remains the same:

- Job On permanently records the change, including the previous and replacement context where applicable, backend actor and timestamp.
- A lightweight awareness signal is exposed to the consumers of the changed production context.
- The signal identifies the `jobon_id` and the kind of context that changed; it is not a second snapshot of Job On truth.
- A receiving module exposes the change as pending awareness until that module acknowledges it.
- Acknowledgement means only **"seen / taken notice of"**. It does not mean resolved, corrected, recalculated, approved or otherwise treated.
- Acknowledgement never deletes or rewrites the permanent Job On change log.
- Acknowledgement is scoped per consuming module; acknowledgement by one module must not clear another module's pending awareness.
- Notification routing is derived from the same functional production-context dependencies used by the operational workflows. It must not become a separate source of domain truth.
- This mechanism does not automatically decide impact, create work, recalculate downstream records or rewrite historical records.
- Tool replacement follows the operational-use boundary: before operational use, a planning-only replacement may keep the same `cm_id`, `mf_id` or `bq_id`; after operational use, replacement creates a new production-context identity and preserves the old context for historical records.

Detailed behavior lives in `modules/job-on/CONTEXT_CHANGE_AWARENESS.md`.
