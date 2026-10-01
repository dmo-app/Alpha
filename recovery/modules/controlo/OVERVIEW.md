# Controlo — Common Context

Controlo is one operational domain exposed through two capability surfaces:

```text
Controlo
├── Controlo Create
└── Controlo Approve
```

Create and Approve are not separate product domains. They are different operational capabilities over shared production/control context.

This directory contains only the rules that are genuinely common to both surfaces.

## Production relationship

Controlo operates in the context of a Job On production.

The production already provides the identities required to reach the selected component contexts:

- `jobon_id`;
- `cm_id`;
- `mf_id`;
- `bq_id`.

Individual Controlo functions keep their own real identities and persistence boundaries.

Examples include:

- `peso_id`;
- `comparacao_id`;
- the durable records required by Folha or Pegamentos where applicable.

UI grouping under Controlo does not, by itself, make all of these children of one generic parent.

## Pre-production Tool control

A Tool/lote may need to be controlled before any Job On exists.

Job On is **not** a prerequisite for recording a valid pre-production control fact where the owning Controlo workflow supports that operation.

In that state, the record is anchored directly to the canonical `tool_id` it actually controls.

The application must not fabricate:

- `jobon_id`;
- `cm_id`;
- `mf_id`;
- `bq_id`;

merely to make a pre-production record look production-bound.

When that Tool is later explicitly selected into a Job On slot, the selection itself is the association intent. The owning workflow may then bind the same durable record to the applicable production component context and clear its temporary direct `tool_id` anchor.

After association, the Tool remains reachable through the production context relation; the record must not keep two competing canonical anchors for the same association.

Peso follows this rule explicitly as defined in `../controlo-create/PESO.md`.

## Future Job On preparation context

A successfully created Job On immediately receives its associated canonical `controlo_id`.

That association belongs to the production context itself:

```text
jobon_id A ↔ controlo_id A
jobon_id B ↔ controlo_id B
```

The association is stable, but the Resumo surface is not locked to one Job On/Controlo context. The operator may navigate between productions and their respective `controlo_id` contexts.

A successfully saved future Job On is therefore immediately available to Controlo as a selectable preparation context.

Controlo does not need to wait for that production to become the current machine production before the operator can inspect or prepare its Resumo context.

```text
Job On saved
→ jobon_id exists
→ associated controlo_id already exists
→ Controlo Resumo can list that production
→ user explicitly selects the production/context
→ backend resolves the existing jobon_id ↔ controlo_id association
→ preparation may begin where the owning Controlo workflow allows it
```

This is an explicit read/context selection, not automatic activation.

```text
future jobon_id selectable in Controlo
!=
machine production already changed
```

Controlo receives no duplicated production snapshot. Its selection points to the persisted `jobon_id`, and the backend resolves the required current Job On context through the normal focused-read rules.

This early preparation behavior is Controlo-specific. It must not be generalized so that Boquilhas or another operational module adopts a future Job On before its own real transition rule applies.

## Shared read context — Resumo and Folha visibility

Resumo is a shared read/composition surface over the real production/control records.

Folha is a Controlo Create control/evaluation surface and has **no approval lifecycle**.

Controlo Approve may read the production/control context and the Resumo composition needed for an approvable workflow, but that visibility must not be interpreted as Folha approval.

Conceptually:

```text
Controlo Create
→ creates/edits operational facts
→ owns Folha control/evaluation facts
→ sees Resumo composition

Controlo Approve
→ reviews approvable records
→ in the current Beta, approval decisions apply to submitted Peso records
→ may read Resumo/context
→ does not approve/reject/reopen Folha
```

There is no copied Folha or copied Resumo created for approval.

## `controlo_id` — canonical Controlo production context

`controlo_id` is the canonical functional identity of the shared Controlo production context where durable Controlo-level facts belong.

Each Job On creates and associates its `controlo_id` as part of Job On creation. Controlo therefore has an existing context before Peso, Folha, Pegamentos or another Controlo record is created.

The exact technical representation may still be chosen during implementation, but the functional lifecycle is fixed:

```text
create Job On
→ create jobon_id
→ create associated controlo_id
```

It is not permission to rewrite every existing Controlo relation.

It provides a truthful home for facts that belong to Controlo as a shared production context rather than to:

- Job On;
- a canonical Tool;
- one component context;
- Peso;
- Comparação;
- another individual Controlo function.

Its exact implementation boundary must preserve the ownership rules of the individual Controlo functions. Existing function-specific records do not become children of `controlo_id` merely for navigation convenience.

## What `controlo_id` must not become

It must not become:

- a replacement for `jobon_id`;
- a replacement for `cm_id`, `mf_id` or `bq_id`;
- a replacement for function-specific identities such as `peso_id` or `comparacao_id`;
- a generic parent added only because several screens appear under the Controlo navigation;
- a shortcut introduced solely to avoid query traversal.

## Resumo

Resumo is a read/composition surface.

There is no canonical persisted `resumo_id`.

Resumo may compose the state of several Controlo functions for one production, but that read composition does not take ownership of those records.

`controlo_id` and Resumo are therefore different concepts:

```text
controlo_id
→ canonical durable shared Controlo context created with and associated to one Job On

Resumo
→ derived read/document composition
```

## Create / Approve boundary

Controlo Create owns create-side operational actions such as measurement, editing and submission where the workflow uses submission.

Controlo Approve owns review and approval-side decisions where the workflow is approvable. In the current Beta, the defined approval lifecycle is the submitted Peso lifecycle; Folha is not approvable.

The same underlying record identity persists across both surfaces. Approval must not create a duplicate record simply because it occurs in another capability surface.

Visibility is not edit ownership:

```text
Approve can see Create-owned operational state
!=
Approve can edit that operational state
```

## Human decisions

Measurements, calculations, alerts and technical OK/NOK states may inform a person.

They must not silently become human approval/rejection decisions unless the specific workflow explicitly defines such behavior.

## Related blueprint references

Create-side details:

- `../controlo-create/OVERVIEW.md`

Approve-side details:

- `../controlo-approve/OVERVIEW.md`

Production context:

- `../job-on/OVERVIEW.md`

Canonical identities:

- `../../IDENTITIES.md`
