# Canonical Identities

This file defines domain identities. A database table, read model, UI tab or document does not become an identity merely because it exists.

## 1. `tool_id` — canonical Tool

- Identifies one canonical physical Tool registered in Ferramentas.
- Applies to CM, MF and BQ.
- Consumer modules reference the Tool; they do not create replacement Tool identities.
- A different lot is a different canonical Tool and therefore a different `tool_id`.
- A lot is not mutable state on an existing Tool: new lot means new Tool and new `tool_id`.

## 2. `jobon_id` — production occurrence

- Identifies one concrete production occurrence.
- Job On owns this identity.
- It is not replaced by a generic `production_id`.
- Downstream workflows use the exact production/context identities they actually need instead of inventing a second production identity.

## 3. `cm_id`, `mf_id`, `bq_id` — Tool-in-production contexts

- Identify the CM, MF or BQ context used in one `jobon_id`.
- They reference a canonical `tool_id` but are not Tools themselves.
- Clients never mint these identities.
- Tool selection in Job On is explicit. Selecting the Tool for a CM/MF/BQ slot is itself the production-association decision; the application must not ask for a second confirmation just to associate that selected Tool.
- **Before operational use**, while the Job On/component context is still only planning and no operational record has consumed that context, replacing the selected Tool may update that same `cm_id`, `mf_id` or `bq_id` in place.
- **After operational use**, once downstream history exists for that component context, replacing the Tool must create a new `cm_id`, `mf_id` or `bq_id`. The old context remains attached to its original `tool_id` and remains referencable by the records that used it.
- A different lot is a different canonical Tool, so the same temporal rule applies when the operator changes to another lot.
- Historical downstream facts are never retargeted to a replacement Tool.

## 4. `controlo_id` — Controlo context in a production

- Identifies the persistent Controlo context for one production.
- It is created and associated when its `jobon_id` is created; Controlo does not wait for the first Peso, Folha, Pegamentos or other operational record to create this context.
- One Job On has its own associated Controlo context, and Resumo may navigate between the `controlo_id` values of different productions.
- The association does not lock the Resumo UI to one Job On; it only keeps each Controlo context truthfully attached to its production.
- It is the truthful home for facts that belong to Controlo as a production context rather than to Job On, a Tool, or one specific Controlo function.
- It does not replace `jobon_id`, `cm_id`, `mf_id`, `bq_id`, `peso_id` or `comparacao_id`.
- It is not a generic god-parent for every Controlo record.
- UI grouping under Controlo does not imply persistence ownership under `controlo_id`.

The exact technical representation may be designed during implementation, but the creation-time association with `jobon_id` is canonical.

## 5. `peso_id` — Peso record

- Identifies one specific Peso control/result record.
- It persists through create, save, submit, approval, rejection and reopen.
- There is no approval copy.
- It is not a production identity, Tool identity, Job On identity, CM identity or revision.
- A Peso may begin **before production association** anchored directly to the canonical CM `tool_id`.
- When that same CM Tool is explicitly selected in a Job On and the applicable `cm_id` exists, the same `peso_id` becomes associated to that `cm_id`; the direct `tool_id` anchor on the Peso is then cleared.
- No second association confirmation is required: the explicit CM Tool selection in Job On already expresses the association intent.
- After association, the Tool remains reachable through `peso_id → cm_id → tool_id`; Peso must not keep a competing direct Tool anchor.
- The same `peso_id` then follows that production through later save/calculate/submit/approve/reject/reopen/history actions. Approval never reassigns it to another production.
- The production `cm_id` identifies the CM Tool/context used for the production; it does **not** identify each individual CM unit/position observed during the Peso measurement.
- One `peso_id` may contain several measurement rows while remaining associated with the same production `cm_id`.
- A Peso measurement row does not create another `cm_id`, another `tool_id`, or another canonical CM entity merely because an individual CM position/unit was measured.
- Any visible CM number/position recorded on a Peso measurement row is measurement data inside that `peso_id`, not a canonical Tool-in-production identity.

## 6. `comparacao_id` — Peso Comparação event

- Identifies one Comparação event started for an existing **approved** `peso_id`.
- A Peso without a Comparação is valid.
- `por_aprovar` and `nao_aprovado` Pesos are not eligible to start Comparação.
- Multiple Comparação events may exist for the same Peso.
- Comparação reuses existing `cm_id`; it does not create a new CM.
- It does not alter the original Peso and does not create `previous_peso_id`.

## 7. `bq_repair_trace_id` — Boquilhas trace for one production BQ context

- Identifies the durable Boquilhas movement trace for one BQ context in one production.
- Normal relationship: one `bq_id` has one `bq_repair_trace_id`, and that trace contains many `movement_id` events.
- The trace is **not** the identity of each individual repair trip/cycle. Several `saida` / `entrada` / `entrada_sem_reparacao` cycles may occur inside the same trace.
- A trace may begin before Job On and initially reference the canonical BQ `tool_id` directly, with `bq_id = null`.
- For one BQ `tool_id`, **at most one unresolved pre-production trace may exist at a time**. While that trace still carries the direct `tool_id`, later repair movements for that Tool continue in the same trace; a second unresolved trace is not created.
- When the BQ Tool is explicitly selected in Job On and the matching `bq_id` is created/resolved, that Tool selection is the association decision. The same pre-production trace is associated to the `bq_id` automatically; no second confirmation is required.
- On association, the trace keeps the same `bq_repair_trace_id` and all existing movements, sets `bq_id`, and clears its direct `tool_id` anchor. The canonical Tool remains reachable through `bq_repair_trace_id → bq_id → tool_id`.
- Clearing the direct `tool_id` after association permits a later pre-production repair trace for the same physical Tool when a future production cycle requires one.
- The same physical `tool_id` may therefore have many repair traces **historically**, but never more than one simultaneous unresolved pre-production trace.
- There is no trace `open` / `closed` lifecycle.
- If no pre-production trace exists when a production `bq_id` is created, that production uses a new trace.
- If the trace starts after the production `bq_id` already exists, it is associated immediately to that production context.
- A new production creates a new `bq_id` and therefore a new production trace, even when the canonical physical BQ `tool_id` is the same as in a previous production.
- Late returns remain movements of the original production trace where their repair activity originated; a later current production never steals or reassigns them.

The existing `boquilhas_id`-based implementation is a valid implementation base and its real records/history must be preserved. The implementation must reconcile that persistence with this canonical trace relationship without destructive loss.

## 8. `movement_id` — Boquilhas movement

- Identifies one Boquilhas quantity movement/event.
- Movement type is exactly one of `saida`, `entrada`, or `entrada_sem_reparacao`.
- Each movement belongs to one `bq_repair_trace_id`.
- Movement identity is distinct from `bq_repair_trace_id`, `bq_id`, `tool_id`, the Boquilhas register identity, and any audit-entry identity.

## Structures and names that deliberately do not create canonical identities

### `resumo_id`

`resumo_id` does not exist.

Resumo is a read composition / derived dashboard-document surface inside Controlo. It is not a persisted parent, FK anchor or independent canonical identity.

### `tp_id` / `tampao_id`

TP/Tampão has no independent operational identity in the current process.

DMO preserves the applicable production value in the Job On context. It must not invent a `tp_id` or `tampao_id` merely because Peso reads that value.

### `tool_technical_values`

`tool_technical_values` is an optional 1:1 extension of a Tool and uses `tool_id` as its PK/FK.

There is no separate `technical_values_id`.

The normal Tool registry/search path stays light; specialized technical values are loaded only when a consuming workflow asks for them.
