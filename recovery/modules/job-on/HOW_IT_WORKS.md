# Job On

## Purpose

Job On represents and manages one concrete production occurrence.

It is where a production is identified and prepared, where the applicable canonical Tools are explicitly selected, and where the production-specific Tool contexts are created.

Job On establishes the production context that other modules consume.

It does not become the owner of the operational facts created by those modules.

---

## Core identities

```text
jobon_id
= one concrete production occurrence
```

A Job On may establish the following production-specific Tool contexts:

```text
cm_id -> tool_id
mf_id -> tool_id
bq_id -> tool_id
```

Where:

- `tool_id` identifies the canonical Tool;
- `cm_id`, `mf_id` and `bq_id` identify that Tool in one specific production context.

The context identity and the Tool identity are not interchangeable.

The same canonical Tool may be used in several productions and therefore have several production-context identities over time.

Example:

```text
tool_id CM-X

Production A
└─ cm_id <production_a_cm_id> -> tool_id CM-X

Production B
└─ cm_id <production_b_cm_id> -> tool_id CM-X
```

A different lot is a different canonical Tool and therefore receives a different `tool_id`.

A lot change is not an in-place mutation of the previous Tool identity.

---

## Production context

Job On creates the relationship between a production occurrence and the Tools selected for that production.

The production context preserves the historical Tool facts required to understand that occurrence later.

This does not mean that the complete current Tool record is copied into every Job On context.

Only the historical facts required by the production context should be preserved.

Changes to mutable Tool master information must not rewrite historical production contexts.

Ferramentas remains the canonical owner of Tool identity and Tool-owned master facts.

---

## Tool selection

Tool selection is explicit.

For each applicable component, the operator searches the available canonical Tools and selects the exact Tool to use.

Metadata such as:

- reference;
- lot;
- machine/line;
- Tool type;

may be used to search and identify candidates.

They do not replace `tool_id` as the persisted canonical identity.

The system must not silently select a Tool merely because only one candidate appears likely.

If the required Tool does not yet exist, the authorized Tool-creation flow may be used. After creation, the new canonical `tool_id` returns to the Job On workflow for explicit association.

Job On does not create a private or competing Tool registry.

---

## Access model

Job On is one operational module that may expose different access functions.

In the complete functional model:

- **Job On View** allows navigation and consultation of Job On information;
- **Job On Create** allows consultation and the creation/editing actions authorized inside Job On.

These are distinct, independently assignable access functions inside the same module.

**Job On Create includes all Job On View functionality, so a user with Job On Create does not also need Job On View assigned. Job On View remains a separate assignable capability for users who need read-only access.**

They are not cumulative grants.

A user with **Job On Create** does not also need to be assigned **Job On View**, because Create already includes the consultation capability required for that work.

Conversely, **Job On View** exists so a user may consult Job On information without receiving creation or editing authority.

The existence of Users, Access Templates and multiple access capabilities elsewhere in DMO does not mean every capability must be assigned in the current Beta. Access configuration and Beta operational scope are separate concerns.

This distinction must not be confused with the UI state of the Job On sheet.

### View mode and Edit mode

Inside **Job On Create**, an existing Job On opens in **View mode** as the safe/default state.

The user must explicitly enter **Edit mode** before changing editable production information.

This reduces accidental modification of production data.

These UI modes are not separate access permissions.

In **Job On View**, consultation is the only behavior and no edit action is available, so an additional internal View/Edit distinction is unnecessary.

---

## Beta scope

The current Beta exposes only **Job On Create** as the required Job On operational access function.

This is an intentional Beta scope decision, not a residue of an obsolete access model.

The current Beta is intended for a single operational user in Job On, and **Job On Create** already provides both the consultation and modification capabilities required for that use.

Therefore the Beta does not need to expose or assign **Job On View** separately.

This remains compatible with the existence of Users, Access Templates and permission configuration elsewhere in DMO. Those mechanisms define how access can be assigned; they do not require every defined capability to be separately assigned in the current Beta.

**Job On View** remains part of the complete functional model as the independent read-only capability for users who need Job On information but must not create or edit Job Ons.

The Beta simplification must not be interpreted as removal of Job On View, nor as evidence that the access model is incomplete.

---

## Creation

Creating a Job On creates a new production occurrence and therefore a new `jobon_id`.

The operator defines the production information required by the current Job On scope and explicitly selects the applicable Tools.

For each selected Tool, Job On creates the corresponding production-specific context:

```text
new Job On
├─ new cm_id -> selected CM tool_id
├─ new mf_id -> selected MF tool_id
└─ new bq_id -> selected BQ tool_id
```

Only contexts that are actually applicable to the production are created.

The backend owns allocation of canonical IDs.

---

## Editing

Editing an existing Job On modifies only the production facts that are explicitly editable under Job On Create.

Entering Edit mode is an explicit user action.

Editing a Job On must not silently rewrite:

- previous production occurrences;
- historical Tool contexts;
- Tool master identity;
- facts belonging to Controlo;
- Boquilhas movements;
- other module-owned operational data.

Changes to Tool selection must preserve truthful history.

If a selected CM, MF or BQ Tool is replaced while the same `jobon_id` remains valid, the identity behavior depends on operational use:

```text
planning-only / not yet consumed
→ same component-context ID may be updated to the newly selected Tool

already consumed by operational history
→ create new cm_id / mf_id / bq_id
→ preserve old context on its original Tool
```

Historical records are never retargeted to the replacement context.

Relevant production-context changes are also permanently logged and exposed through the lightweight awareness mechanism defined in `CONTEXT_CHANGE_AWARENESS.md`.

---

## Duplication

**Duplicate Job On** is a convenience for creating a new production from an existing production.

It is assisted creation, not revision of the source Job On.

The source Job On is always selected explicitly by the user.

The system must not assume that the immediately previous or chronologically latest Job On is the correct source.

A later production may run on another machine or require the configuration of an older production. The user therefore decides which historical Job On provides the useful starting point.

### Identity behavior during duplication

The source production keeps:

- its `jobon_id`;
- its `cm_id`;
- its `mf_id`;
- its `bq_id`;
- all of its historical facts.

Nothing in the source is replaced or reused as the identity of the new production.

A new `jobon_id` is created for the duplicated production.

The source component contexts are used to resolve which canonical Tools were selected:

```text
source cm_id -> CM tool_id
source mf_id -> MF tool_id
source bq_id -> BQ tool_id
```

The new Job On then creates new production-context identities that initially reference those same canonical Tools:

```text
Source Job On
├─ cm_id <source_cm_id> -> tool_id CM-X
├─ mf_id <source_mf_id> -> tool_id MF-X
└─ bq_id <source_bq_id> -> tool_id BQ-X

Duplicate
        ↓

New Job On
├─ cm_id <new_cm_id> -> tool_id CM-X
├─ mf_id <new_mf_id> -> tool_id MF-X
└─ bq_id <new_bq_id> -> tool_id BQ-X
```

Therefore:

- source `cm_id`, `mf_id` and `bq_id` are never reused;
- the corresponding canonical `tool_id` values may be retained as the starting selections;
- the new production always receives its own context identities.

The user may then explicitly change any copied Tool selection or other editable starting value.

### Copied values

The selected source provides convenient starting values.

Copied values are defaults for the new production, not immutable inheritance from the source.

A user with **Job On Create** may change the values that are editable for the new production.

### After duplication

After the new Job On is created, the interface opens the new `jobon_id` in **Edit mode**.

This allows the user to review and adjust the new production before continuing.

The source Job On remains unchanged.

---

## Duplicate Job On vs Duplicate Tool

The two operations follow the same general principle:

> duplication provides a useful starting state while creating the identity required by the new real-world thing or occurrence.

But they create different identities because they represent different things.

### Duplicate Tool

Creates another canonical Tool:

```text
source tool_id T1
→ new tool_id T2
```

For example, another lot of the same reference is another Tool and therefore has another `tool_id`.

### Duplicate Job On

Creates another production occurrence:

```text
source jobon_id J1
→ new jobon_id J2
```

If the new production initially uses the same Tools, it keeps their existing canonical `tool_id` values but creates new production-specific `cm_id`, `mf_id` and `bq_id` contexts.

---

## Relationship with other modules

Job On owns the production occurrence and the Tool contexts established for that production.

Other modules may consume those identities.

For example:

```text
Job On
→ production context

Controlo
→ measurements, evaluations and decisions in that context

Boquilhas
→ BQ repair movement information where associated with that context

Ferramentas
→ canonical Tool identity and Tool-owned facts
```

Using a Job On identity or component context does not transfer ownership of a module's facts to Job On.

---

## What Job On does not own

Job On does not own:

- canonical Tool master data;
- Tool-specific technical master values;
- Peso measurements;
- Comparação records;
- Pegamentos measurements;
- Folha evaluation facts;
- Boquilhas repair movements;
- module-specific approval or audit records.

Job On establishes production context.

The module that creates an operational fact remains responsible for that fact.

---

## Core invariants

- One `jobon_id` represents one production occurrence.
- `tool_id` remains the canonical identity of a Tool.
- `cm_id`, `mf_id` and `bq_id` are production-specific contexts, not Tool identities.
- The same `tool_id` may legitimately appear in multiple Job Ons.
- A different lot means a different canonical Tool and therefore a different `tool_id`.
- Duplicate Job On creates a new `jobon_id`.
- Duplicate Job On never reuses the source `cm_id`, `mf_id` or `bq_id`.
- Duplication may retain the source Tools by resolving their `tool_id` values.
- The source Job On is never modified by duplication.
- Tool selection remains explicit.
- Job On Create includes the consultation behavior required to perform its work.
- Job On View and Job On Create do not need to be granted together.
- The current Beta requires only Job On Create; this does not remove Job On View from the complete model.
- UI View/Edit modes must not be confused with access capabilities.
- Job On provides production context to downstream modules but does not take ownership of their operational facts.
- Replacing a selected CM, MF or BQ before operational use may keep the same component-context identity; after operational use, replacement creates a new context identity and preserves the old one for history.
- Relevant Job On context changes are permanently logged and surfaced as awareness to their functional consumers; acknowledgement means only that the change was seen.
