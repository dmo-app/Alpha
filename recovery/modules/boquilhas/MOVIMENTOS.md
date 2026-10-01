# Boquilhas — Movimentos

This file defines the quantity, exceptional-movement, discrepancy and correction behavior of the Boquilhas module.

## Implementation boundary — one movement trace per BQ production context

The canonical movement boundary is `bq_repair_trace_id`.

One `bq_id` / production context has one repair trace, and that trace groups all Boquilhas movement cycles for that production. A new `saida` does not create another trace.

A trace may begin before Job On from the canonical BQ `tool_id`, with `bq_id = null`.

For one `tool_id`, there is at most one unresolved pre-production trace at a time. Later movements for that Tool continue in the same trace.

When the BQ Tool is explicitly selected in Job On, the same trace associates to the matching `bq_id`, keeps all movements, and clears its temporary direct `tool_id` anchor.

The existing `boquilhas_id`-based register/persistence is a valid implementation base and its real operational history must be preserved.

## 1. Fixed Tool quantity is the lot base

The canonical BQ Tool/lote has a fixed quantity entered manually in Ferramentas.

Example:

```text
BQ Tool quantity = 20
```

Boquilhas consumes that value as the accounted lot base.

Repair movements do **not** mutate the fixed Tool quantity.

Exceptional movements record what physically happened; they do not silently make the Tool quantity larger or smaller.

## 2. Operational truth has priority

Boquilhas preserves the movement the operator actually observed, including quantities that do not fit the current calculated availability.

The system must not reject, clamp, invent or silently rewrite a real movement merely to make the arithmetic tidy.

This applies to both exceptional Saídas and exceptional Entradas.

## 3. Saída

A Saída records BQ sent to the repairer resolved automatically from Boquilhas Definições.

A Saída may exceed the quantity currently derived as in-house.

Example:

```text
Tool quantity = 20
already out for repair = 5
derived in-house = 15

observed Saída = 17
```

The movement is accepted and recorded in full.

In this example, the recorded Saída exceeds the currently derived in-house quantity by 2. That fact does not change the fixed Tool quantity.

A Saída does **not** create the Entrada discrepancy/Saldo described below.

## 4. Entrada and discrepancy

Discrepancy is specifically the quantity **too many on an Entrada** compared with the quantity that can legitimately return at that point in the trace.

Example:

```text
Saída:   5
Entrada: 7

matched quantity = 5
extra on Entrada = 2
Saldo = -2
```

The full observed Entrada is recorded.

Only the explainable/matched portion returns to the normal accounted flow.

The extra observed quantity:

- remains recorded as part of the real Entrada;
- does not increase the fixed Tool quantity;
- creates the movement discrepancy.

Conceptually:

```text
matched_quantity = explainable return portion
unmatched_quantity = observed Entrada - matched_quantity
movement_discrepancy = -unmatched_quantity
```

## 5. EntradaSemReparação

`entrada_sem_reparacao` means BQ returned without having been repaired.

It is a distinct movement meaning. It reduces the quantity that was out for repair according to the movement flow.

It is **not itself a discrepancy concept**.

Do not treat “sem reparação” as a reason to create the Entrada excess-discrepancy calculation. Its movement meaning and the discrepancy rule are separate concerns.

## 6. Meaning of Saldo

The **Saldo** column is a per-movement Entrada discrepancy indicator.

It is not stock, quantity out for repair, or a conventional running accounting balance.

Normal movement:

```text
Saldo = blank
```

Exceptional Entrada:

```text
Saldo = -extra Entrada quantity
```

Do not display `0` for an ordinary movement.

Saída exceptional quantity is not represented by this Entrada Saldo.

## 7. Trace discrepancy

A later normal movement does not automatically cancel an earlier Entrada discrepancy.

```text
trace_discrepancy
= sum(all movement discrepancies in this trace)
```

Example:

```text
Entrada discrepancy A = -2
Entrada discrepancy B = -1

trace discrepancy = -3
```

A later movement that happens to offset the arithmetic does not erase those facts.

A deliberate correction/removal of the actual movement is different and follows the correction rules below.

## 8. Production scope

The discrepancy belongs to one `bq_repair_trace_id`.

A pre-production trace may later associate to its `bq_id` without resetting the trace.

A new BQ production context receives a new trace with its own derived discrepancy state.

Previous movement facts remain on the previous trace.

Late returns remain on the trace where the original repair activity occurred.

## 9. Correction and removal order

Movement calculations depend on the movement sequence.

Therefore **only the latest movement in a trace may be directly corrected or removed**.

To correct an older movement:

```text
target old movement
→ remove every later movement, newest first
→ target becomes the latest movement
→ correct target
→ re-enter later real movements if they still need to exist
```

Example:

```text
M1 Saída 5
M2 Entrada 7  → Saldo -2
M3 Saída 4
M4 Entrada 4

to correct M2:
→ remove M4
→ remove M3
→ correct M2
```

This prevents a correction in the middle of the trace from silently changing the meaning of later movements that were recorded against the previous sequence.

### Correcting the latest Entrada

If the latest Entrada was entered incorrectly, correcting its quantity recalculates the discrepancy of that same movement against the preceding trace state.

Example:

```text
latest Entrada originally = 7
explainable return = 5
Saldo = -2

correct latest Entrada to 5
→ movement discrepancy recalculated
→ Saldo becomes blank
```

### Audit

Correction/removal must remain auditable.

The exact technical representation of that audit, including how a correction/removal is persisted, is an implementation decision. The functional rule here is only that older movements cannot be corrected while later movements remain after them.

## 10. Explicitly forbidden interpretations

An implementation must not:

- mutate the fixed Tool quantity because of repair movements;
- reject an exceptional Saída merely because its quantity exceeds derived in-house quantity;
- reject an Entrada merely because it contains extra observed quantity;
- truncate a physical movement to the mathematically expected quantity;
- invent a missing movement to reconcile quantities;
- treat `entrada_sem_reparacao` as synonymous with discrepancy;
- use a later ordinary movement to cancel an earlier Entrada discrepancy;
- show `0` in Saldo for ordinary movements;
- implement Saldo as a stock/running-balance column;
- edit an old movement while later movements remain active after it;
- move historical movements to a later production trace.
