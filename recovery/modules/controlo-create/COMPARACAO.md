# Controlo Create — Comparação

Comparação belongs entirely to Controlo Create.

There is no separate Comparação approval workflow.

## Purpose and eligibility

Comparação exists when a problem or doubt about the current production Peso needs to be checked again against the CM that is actually producing.

It may be started **only from an approved Peso**:

```text
peso.status = aprovado
→ eligible for Comparação

peso.status = por_aprovar / nao_aprovado
→ not eligible
```

This is not a second approval of the Peso.

The operator performs a new confirmation measurement against the current CM context and decides whether that CM can remain in use.

## Flow

1. Start from an existing approved `peso_id`.
2. Allocate a new `comparacao_id`.
3. Reuse the production `cm_id` already associated with that Peso.
4. Record the new confirmation measurement(s) required by the real situation.
5. Decide explicitly:
   - `Manter`;
   - `Colocar de parte`.
6. Confirm the Comparação.

`Colocar de parte` requires a non-empty justification.

Warnings or calculated results may inform the person but must not silently choose the decision.

## Identity and relations

- Comparação has its own `comparacao_id`.
- It references one existing approved `peso_id`.
- It reuses that Peso's production `cm_id`; it does not create another CM production-context identity.
- Physical CM number/position labels used inside comparison measurements are operational measurement data, not new `cm_id` or `tool_id` identities.
- Multiple Comparação events may exist for the same `peso_id`.
- A later problem creates another `comparacao_id`; it does not reopen or overwrite the previous confirmed event.

Comparação does not create a `previous_peso_id` relation. It is not a comparison between two Peso records.

## Decision boundary

The decision is about the CM being checked:

```text
confirmation good
→ Manter

confirmation not good
→ Colocar de parte
```

Comparação does **not** change the original Peso approval status.

It does not approve, reject or reopen the original Peso and does not rewrite the Peso's measurements, averages, frozen inputs or generated outputs.

## Corrections and history

While the Comparação is still being filled in, the user may correct the current input before confirming it.

Once confirmed, the event is historical. It is not reopened for a later operational issue.

If another check is required later:

```text
same approved peso_id
→ new comparacao_id
→ new confirmation event
```

This preserves each verification independently.

## What Comparação does not do

Comparação does not:

- replace or revise the original Peso;
- create another approval lifecycle;
- create new Tool or production-context identities;
- require every physical CM position to participate;
- silently infer `Manter` or `Colocar de parte`;
- overwrite a previously confirmed Comparação event.
