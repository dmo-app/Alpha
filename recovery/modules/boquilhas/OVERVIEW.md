# Boquilhas — Overview

Boquilhas records the real repair movements of BQ Tools while keeping those movements grouped by production context.

The existing register/movement implementation is a valid base to preserve and evolve.

## Canonical relationship

```text
tool_id
= canonical physical BQ Tool/lot

Tool.quantity
= accounted quantity of physical BQ tools in that Tool/lot

jobon_id
→ bq_id
→ one bq_repair_trace_id
→ many movement_id
```

`bq_id` identifies that BQ in one specific production and references its canonical `tool_id`.

Boquilhas consumes the BQ Tool quantity owned by Ferramentas. Example:

```text
BQ Tool
lot = 4
quantity = 120
```

The value `120` is the accounted lot total for that canonical BQ Tool. Boquilhas uses it together with movement facts to derive operational quantities such as quantity in house and quantity out for repair.

Boquilhas does not own or duplicate this Tool master quantity. Repair movements do not silently increase it when unexplained physical returns are observed; those exceptional observations follow the movement rules in `MOVIMENTOS.md`.

For BQ in the current Beta, the Tool/lote also has one associated machine/line. A pre-production BQ may have no current production machine, but its Tool-associated machine still exists and is used by Boquilhas Definições to resolve the repairer automatically.

`bq_repair_trace_id` identifies the movement trace for that BQ production context. It is not created per repair trip. Several cycles may exist inside the same trace:

```text
trace
→ saida
→ entrada
→ saida
→ entrada_sem_reparacao
→ saida
→ ...
```

A new production creates a new `bq_id` and a new trace, even if both the previous and new `bq_id` reference the same physical BQ `tool_id`.

## Pre-production trace

A repair movement may be recorded before the relevant Job On exists.

In that case:

```text
tool_id
→ one unresolved bq_repair_trace_id
→ movements

bq_id = null
```

For one BQ `tool_id`, **only one unresolved pre-production trace may exist at a time**.

While that trace still carries the direct `tool_id`, every later Boquilhas movement for that Tool continues in the same trace. A second unresolved trace for the same `tool_id` is not created.

When the BQ Tool is explicitly selected in Job On:

```text
selected BQ tool_id
→ bq_id created/resolved
→ same pending bq_repair_trace_id associates to bq_id
→ direct trace.tool_id is cleared
→ existing movements remain on the same trace
```

The explicit Tool selection is already the association intent. There is no second association confirmation.

After association, the Tool remains reachable through:

```text
bq_repair_trace_id
→ bq_id
→ tool_id
```

Because the direct `tool_id` anchor is cleared after production association, a later future cycle may create a new pre-production trace for that same physical Tool. The Tool may therefore have many traces historically, but never multiple simultaneous unresolved pre-production traces.

There is no trace `open` / `closed` lifecycle.

If no pending trace exists when the production `bq_id` is created, that production uses a new trace. If a trace starts after the production `bq_id` already exists, it is associated immediately to that production context.

## Late returns

A later production never takes ownership of an earlier trace.

```text
Production A
→ bq_id A
→ trace A
→ saida 5

Production B
→ bq_id B
→ trace B

late entrada 5
→ trace A
```

The current machine/production context is not used to reassign old movements.

## Movement facts

Movement types remain:

- Saída;
- Entrada;
- EntradaSemReparação.

The existing `boquilhas_id`-based persistence remains a valid implementation base and must be reconciled without discarding real operational history.

Quantity-in-house, quantity-out and discrepancy are projections/derivations over the Tool quantity plus movement facts. Their detailed mathematics may be refined independently of this identity structure.

## Beta access and consultation boundary

A User with the BQ module assigned can access Boquilhas consultation.

The Beta keeps Boquilhas operational history inside the Boquilhas module. Job On does not become a second surface for BQ repair movements/discrepancy.

No Boquilhas PDF or email artifact is required in the current Beta.

Detailed rules:

- `REGISTO.md`
- `MOVIMENTOS.md`
- `HISTORICO.md`
- `DEFINICOES.md`
