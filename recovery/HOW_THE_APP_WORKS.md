# DMO Beta — How the App Works

This document is the fast canonical map of the application.

It explains what DMO is, the principal relationships between modules, and the general operational flow. Detailed rules live in the module documents under `modules/`.

The intended reading pattern is:

```text
HOW_THE_APP_WORKS.md
→ understand the application and its main relations

modules/<module>/
→ understand the functional behavior of the area being worked on

modules/<module>/backend/
→ load deeper backend detail when implementing/reviewing backend work

modules/<module>/frontend/
→ load deeper frontend detail when implementing/reviewing frontend work
```

Do not use this file as a substitute for the detailed module documents.

---

## 1. What DMO is

DMO is an operational factory application for glass-container manufacturing.

It connects:

- Tools and their stable identity;
- production occurrences;
- the exact Tools used in each production;
- quality-control measurements and human decisions;
- Boquilhas repair movements;
- users, access templates and module permissions;
- historical facts that must remain attributable to the context in which they occurred.

The broad principle is:

> Persist the real operational facts and durable relations. Compose the context required by each screen when it is read.

The application must not create new identities, ownership relations or duplicated facts merely to make one UI or query more convenient.

---

## 2. Main identity and context chain

The central production relationship is:

```text
tool_id
→ canonical Tool

jobon_id
→ one production occurrence

cm_id / mf_id / bq_id
→ the selected Tool in that production context
```

A Tool is registered in Ferramentas and may be used in many productions.

A Job On establishes one production occurrence and selects the relevant Tools.

The component contexts connect the selected canonical Tool to that specific Job On and preserve the required production-time context.

A different lot of the same reference is a different canonical Tool and receives a different `tool_id`.

Detailed identity rules live in `IDENTITIES.md` and the relevant module documents.

---

## 3. General application flow

At a high level:

```text
Admin
→ Users and Access Templates
→ App Definições for centralized editing of selected module-owned settings

Ferramentas
→ register/select canonical Tools

Job On
→ planning calendar is the landing/discovery surface
→ clicking a day shows the production(s) planned to enter that day
→ explicit selection opens the persisted jobon_id
→ create or consult a production
→ select CM / MF / BQ
→ establish production context

Controlo Create
→ record measurements and operational control facts
→ Peso / Comparação / Pegamentos / Folha / Resumo

Controlo Approve
→ review the relevant submitted control records
→ approve / reject / reopen where the workflow allows it
→ preserve decision history

Boquilhas
→ register BQ repair movements
→ derive operational quantities and discrepancy from movement facts
→ preserve movement history
```

Each module owns its own facts. One module consuming another module's context does not transfer ownership.

---

## 4. How the frontend and backend work together

DMO does not require the backend to pre-consolidate the complete domain state before the UI can operate.

The frontend carries the operational context of the current action, but the backend remains responsible for validating identities, relationships, authorization and persisted truth.

The frontend knows the context it is currently using, such as:

- module or capability;
- `jobon_id`;
- relevant component context such as `cm_id`, `mf_id` or `bq_id`;
- record identity such as `peso_id`;
- requested operation;
- current screen/workflow state.

The frontend sends a contextual request.

The backend then:

1. validates the supplied identities and their relationships;
2. validates authorization;
3. traverses only the **real persisted relations** required by that operation;
4. filters and projects the required data;
5. returns a purpose-specific read model.

The page renders that read model.

For list/search surfaces that expose filters, the same rule applies:

```text
user enters filter/search criteria
→ frontend sends the criteria
→ backend applies them to the focused query
→ backend returns only the matching read-model packet
→ frontend renders the result
```

The frontend may keep the current filter controls as local UI state, but it must not obtain the complete dataset merely to perform the operational filtering itself.

### Core distinctions

**READ MODEL ≠ ENTITY**

A model assembled for one screen or operation is not automatically a persisted domain entity.

It does not gain its own identity, table or lifecycle merely because the UI needs that shape.

**QUERY JOIN ≠ DOMAIN RELATION**

A backend query may traverse several existing relationships without creating new foreign keys between the final records.

Example:

```text
peso_id
→ cm_id
→ jobon_id
```

If this already expresses the real relationship, Peso does not need a duplicate `jobon_id` merely to make a read easier.

**UI GROUPING ≠ DATABASE OWNERSHIP**

Several functions may appear together under Controlo without becoming children of one generic database parent.

Peso, Comparação, Pegamentos, Folha and Resumo remain connected through the identities and relationships that represent the real process.

UI organization does not redefine domain ownership.

**DTO / PROJECTION ≠ DOMAIN IDENTITY**

A purpose-specific backend response exists to serve an operation.

Its existence does not justify a new persisted ID or domain object.

### Peso read example

For an existing Peso, a focused read may start from:

```text
peso_id
→ cm_id
→ jobon_id
```

Before production association, the same durable Peso may temporarily be anchored directly to its canonical CM `tool_id`. When that Tool is explicitly selected in Job On, the existing `peso_id` associates to the resulting `cm_id` and its temporary direct `tool_id` anchor is cleared. The same `peso_id` continues through the later approval lifecycle.

When Tool-owned technical values are required, the query follows the actual owner Tool for each value:

```text
volume_puncao / peso_nominal
peso_id → cm_id → CM tool_id → Tool technical values

volume_marisa
peso_id → jobon_id → bq_id → BQ tool_id → Tool technical values
```

If a required Tool value is missing, the user completes it in Ferramentas and Peso re-reads the Tool; Peso does not invent or privately re-enter it.

When the Peso view requires TP/Calote, the backend reads that production value from the Job On context.

TP is a production-specific Job On value. It has no independent operational identity and therefore no `tp_id` or `tampao_id`.

Peso may consume the TP value without becoming its owner and without requiring a new foreign key.

The physical Peso measurement uses:

```text
CM + TP
```

TP adds mass to that physical measurement. Its known contribution may be used to correct or interpret the observed value for the relevant technical/wear analysis.

This is distinct from the later technical calculation, which uses the applicable drawing values, including the required BQ and PU technical volumes.

TP does not become a term in the main Peso formula merely because it is physically present in the measurement process.

### Resumo read example

Resumo is a composition of the relevant persisted facts for one production.

A request may begin from:

```text
jobon_id
```

The backend can then compose the required state by traversing the real persisted relations for that production, including the relevant component contexts and Controlo records.

This does not require a persisted `resumo_id`, nor does it require every Controlo function to become a child of `controlo_id`.

> Resumo is a composition of the relevant persisted facts for that production.


### Access and interaction rules

Visible destinations and actions follow the effective module permissions resolved by the backend. Labels such as `Operador` or `Responsável`, template names, hidden buttons or the current URL do not grant access.

Controlo Create and Controlo Approve may share the visible Controlo destination while remaining distinct permissions and workflows.

Choices that change operational context must remain explicit. This includes Tool selection, Job On production/source selection, repairer selection where applicable, and approval/rejection decisions.

Even when a Tool search returns exactly one valid result, the system must not silently associate it. The user must explicitly select or confirm that Tool.

A production lookup may order results for convenience, but it must not silently change the active production context.

The normal flow is:

```text
frontend establishes active context
→ sends the relevant identity/context
→ backend validates it
→ backend traverses truthful persisted relations
→ workflow receives a purpose-specific read model
→ user enters only facts that are new now
```

Information already available from the active context should not be manually re-entered just because another screen needs it.

The frontend must not mint canonical IDs, invent persistence relationships, infer Tool identity from labels, duplicate backend-owned industrial formulas, turn warnings into decisions, synthesize audit facts, infer permissions from role labels, or invent backend behavior from prototype/demo data.

When a workflow temporarily leaves a form to select or create a Tool or resolve missing context, preserve the values already entered. Cancel returns unchanged. A successful return applies only the explicitly selected or created result.

---

## 5. Hot path vs history

The daily operational path should stay narrow.

A normal operation loads or writes only the records and relations required for that action.

Examples:

```text
Open Peso
→ peso_id
→ cm_id
→ Peso facts required by the page
```

```text
Open Resumo
→ jobon_id
→ relevant production contexts
→ relevant function records for that production
```

Submitting, approving or deciding a record writes to that record and its own decision/history facts. It does not require unrelated aggregates to be recalculated or rewritten.

### Deep history is explicit

Historical investigation may require longer traversals.

For example, an explicit request to understand a Tool across several productions may traverse:

```text
tool_id
→ production component contexts
→ jobon_id
→ relevant operational records
→ decision/history facts where applicable
```

A heavier read is acceptable when the user explicitly asked for historical depth.

The persistence model must not be denormalized merely to make exceptional history queries shorter.

If a fact is already truthfully reachable through an existing relation, do not duplicate the same anchor on every daily-write record just to remove one join from a rare read.

> Prefer simple truthful writes and focused reads over duplicated write-side relationships created for exceptional queries.

---

## 6. How new app behavior should fit

Before introducing a new fact, relation, identity or persistence object, follow this reasoning sequence.

### What real operational event creates the fact?

Identify the concrete moment when the fact becomes real.

Examples include:

- a measurement is entered;
- a production is created;
- a Tool is registered;
- a human decision is made;
- a calculation resolves a result.

If no real event or condition makes the fact true, it may not need persistence.

### When does the fact become known?

Determine whether it is known:

- at Tool registration;
- at Job On creation;
- during measurement;
- at decision time;
- only during later historical review.

The moment when a fact becomes known constrains where it can truthfully live.

### What context is it valid in?

Determine whether the fact is:

- Tool-owned and reusable;
- valid for one production;
- specific to one workflow;
- derived from other persisted facts;
- required as an exact historical value consumed at a specific moment.

### What existing relation already reaches it?

Trace the real persisted relation graph first.

If the required fact is already reachable through an existing truthful relation, use that relation rather than adding a shortcut solely for convenience.

### Is this a write need or a read need?

A screen needing to display several related facts is normally a **read need**.

The backend may compose those facts with a focused query/read model.

A **write need** exists when a new durable fact actually comes into existence and has no truthful existing home.

Do not denormalize the write model merely because one screen needs a convenient shape.

### Does it need a new identity or relation?

A new identity or relation requires a real persistent business reason.

Possible justifications include:

- independent lifecycle;
- genuine 1:N multiplicity;
- independent concurrency boundary;
- append-only operational events;
- a durable fact with no truthful existing owner.

A 1:1 extension keyed by an existing identity may also be valid when it isolates optional specialized data without inventing a second identity.

Do not create UUIDs, duplicate relations or catch-all containers without such a reason.

### What is the shortest truthful hot path?

For the common daily action, identify the minimum real traversal needed.

Do not force routine operations to load unrelated history or module state.

### What history actually needs preserving?

First determine whether history already exists naturally through:

- durable operational records;
- production contexts;
- decision/event records;
- values frozen when consumed.

Do not create a generic history mechanism unless a specific historical fact cannot be preserved or reconstructed from the existing model.

### Can the required screen be composed without a new persisted shortcut?

If the frontend already carries the operation context and the backend can validate and retrieve the required data through real persisted relations, a new persisted shortcut is usually unnecessary.

---

## 7. Guardrails against model drift

### Do not create an ID for every noun

A named concept does not automatically deserve an identity.

An identity requires a meaningful durable entity, lifecycle, multiplicity or ownership boundary.

### Do not create direct foreign keys only for navigation convenience

If an existing relation chain already expresses the truth, do not duplicate a direct relation simply to make a query or diagram shorter.

### Do not mirror frontend navigation as database hierarchy

UI grouping does not establish persistence ownership.

Controlo grouping Peso, Comparação, Pegamentos, Folha and Resumo does not make every function a child of one generic persistence parent.

A legitimate shared Controlo context must be justified by real Controlo-level persistent facts, not by menu structure.

### Do not create giant backend aggregates just in case

The backend should retrieve what the current operation needs.

It should not build one universal object containing Job On, every Tool, every Controlo function and full history for ordinary operations.

### Do not turn Tool into a container for every fact

`tool_id` identifies the canonical Tool.

Reusable Tool-owned technical facts may live with the Tool or in its `tool_id`-keyed technical extension.

Production-specific facts, measurements, decisions and repeated histories remain in their truthful operational records.

### Do not create generic history/context/metadata structures without a real need

History often emerges naturally from durable operational records and their real relationships.

A separate generic history mechanism requires a concrete fact that the existing records cannot preserve.

### Do not confuse read composition with persisted structure

A focused backend query may join several records and return one purpose-specific read model.

That does not mean those records require a new shared parent, identity or duplicated relation.

---

## 8. Module map

### Admin

Admin owns application administration and access setup.

The main administration surfaces are:

```text
Users
→ User identity/details, current Template, account actions

Templates
→ modules/capabilities/permissions + assigned Users

App Definições
→ select a module and edit selected administrative settings owned by that module
```

There is no standalone Modules tab.

The User and Template pages are two management views over the same User ↔ Template association. A legacy User title such as `Operador`, `Reparador` or `Chefe` is not a second access authority; the associated Template name is the visible profile label.

See:

- `modules/admin/OVERVIEW.md`
- `modules/admin/USERS.md`
- `modules/admin/TEMPLATES.md`
- `modules/admin/APP_DEFINICOES.md`
- `modules/admin/SETUP.md`

### Ferramentas

Ferramentas is the canonical Tool registry.

It owns Tool identity and Tool-owned master/technical facts.

See:

- `modules/ferramentas/OVERVIEW.md`
- `modules/ferramentas/CONSULTAR.md`
- `modules/ferramentas/CRIAR.md`
- `modules/ferramentas/SELECIONAR.md`
- `modules/ferramentas/VALORES_TECNICOS.md`

### Job On

Job On owns the production occurrence and establishes the selected Tool context for that production.

The same `jobon_id` may remain valid while a selected production context is replaced.

Before operational use, a planning-only Tool replacement may keep the same component-context identity. After operational use, the replacement CM, MF or BQ receives a new production-context identity pointing to the newly selected canonical `tool_id`; the previous context remains historically referencable and is not retargeted.

Relevant context changes are permanently logged and exposed as lightweight awareness to the modules that consume that context. A module acknowledgement means only that the change was seen. It does not resolve, recalculate or rewrite operational work.

See:

- `modules/job-on/OVERVIEW.md`
- `modules/job-on/CRIAR.md`
- `modules/job-on/CONSULTAR.md`
- `modules/job-on/EDITAR.md`
- `modules/job-on/DUPLICAR.md`
- `modules/job-on/SELECIONAR_FERRAMENTAS.md`
- `modules/job-on/CONTEXT_CHANGE_AWARENESS.md`

### Controlo

`modules/controlo/` contains the rules and context shared by Controlo Create and Controlo Approve.

`controlo_id` is a canonical functional identity for the shared Controlo production context; its exact technical representation is still pending. It must not be spread through the model as a universal parent merely because Controlo groups several functions.

See:

- `modules/controlo/OVERVIEW.md`

### Controlo Create

Controlo Create owns the create-side operational work:

- Peso;
- Comparação;
- Pegamentos;
- Folha create-side actions;
- Resumo create-side presentation;
- Controlo Definições.

See `modules/controlo-create/`.

### Controlo Approve

Controlo Approve owns review and decision behavior where approval applies, including approval history and reopen behavior.

See `modules/controlo-approve/`.

### Boquilhas

Boquilhas preserves the existing register/movement implementation as a valid base while using one canonical `bq_repair_trace_id` for each BQ production context.

The normal chain is:

```text
jobon_id
→ bq_id
→ bq_repair_trace_id
→ movement_id*
```

A trace contains all Boquilhas movement cycles for that BQ in that production; it is not recreated for each repair trip. A pre-production trace may start from the canonical BQ `tool_id` before `bq_id` exists.

For one BQ `tool_id`, there is at most one unresolved pre-production trace at a time. Later movements reuse it.

When the operator explicitly selects that BQ Tool in Job On, the same pending trace associates to the created/resolved `bq_id` automatically. That Tool selection is already the association decision. The trace keeps its identity and movements and clears its temporary direct `tool_id` anchor; the Tool remains reachable through `bq_id → tool_id`.

A new production creates a new `bq_id` and a new production trace even if it uses the same physical BQ `tool_id`. Late returns remain on the trace of the earlier production where they originated.

The current evolution also includes the fixed BQ Tool quantity as the lot base, automatic repairer resolution from the Tool-associated machine through Boquilhas Definições, exceptional Saída/Entrada preservation, Entrada-only discrepancy/Saldo, and ordered correction rules defined by the Boquilhas module files.

Boquilhas history is module-local in the Beta. Users with the BQ module assigned consult it directly; no BQ PDF/email artifact or duplicate Job On movement-history surface is required.

See:

- `modules/boquilhas/OVERVIEW.md`
- `modules/boquilhas/REGISTO.md`
- `modules/boquilhas/MOVIMENTOS.md`
- `modules/boquilhas/HISTORICO.md`
- `modules/boquilhas/DEFINICOES.md`

---

## 9. Controlo relationship

Controlo is one operational domain with two capability surfaces:

```text
Controlo
├── Controlo Create
└── Controlo Approve
```

The surfaces do not create two different product domains.

Common context and future shared identity rules belong in `modules/controlo/`.

Create-side behavior belongs in `modules/controlo-create/`.

Approve-side behavior belongs in `modules/controlo-approve/`.

Resumo is a read/composition surface. It does not become a persistence parent merely because other Controlo functions are displayed through it.

---

## 10. Historical truth

DMO must preserve what happened in the correct operational context.

That does not mean every module needs a dedicated history table or a new identity.

History may be represented by the durable records and relations already created by the real operational events.

Examples:

- a past Job On retains its production context;
- a decided Peso keeps its own record and decision history;
- a Comparação does not rewrite the original Peso;
- Boquilhas movement discrepancies remain historical facts and are not silently reconciled by later movements.

---

## 11. Documents and derived views

A persisted operational record, a read model and a generated PDF are different things.

```text
record ≠ read projection ≠ PDF ≠ filesystem path
```

Generated documents are derived artifacts. They do not replace the underlying structured record as the persisted product fact.

Document behavior is documented under Controlo Create → Documents / PDFs.

Document configuration such as the base directory and recipient routing remains owned by Controlo and is documented in its configuration blueprint. Administrative editing is reached through Admin → App Definições rather than requiring a dedicated operational settings destination.

---

## 12. Detailed blueprint

For detailed behavior, use the module documents.

Start at:

- `modules/INDEX.md`

The module documents are the canonical detailed explanation of each area. Functional/product documents live directly in the module; deeper technical backend and frontend detail lives in that module's `backend/` and `frontend/` subfolders.

There is no separate `features-to-implement` product area. If part of DMO is not coded yet, it still belongs to its real module; `IMPLEMENTATION_STATUS.md` records implementation state separately.

This file remains intentionally compact so a person or AI can understand the application quickly before loading only the module relevant to the current task.
