# Boquilhas Repair Trace Implementation

**Status:** CANONICAL MODEL CLOSED — IMPLEMENTATION RECONCILIATION REQUIRED

**Type:** Boquilhas persistence / migration / workflow implementation

## Purpose

Bring the Boquilhas implementation into line with the canonical repair-trace model without losing real operational history.

The existing `boquilhas_id`-based register/movement persistence is a valid implementation base. It must be adapted, not discarded merely to rename or normalize schema.

## Canonical identity model

```text
tool_id
= canonical physical BQ Tool/lote

jobon_id
→ bq_id
→ one bq_repair_trace_id
→ many movement_id
```

A `bq_repair_trace_id` is one movement trace for one BQ production context, not one repair trip.

## Pre-production trace cardinality

A trace may begin before Job On:

```text
tool_id
→ one unresolved bq_repair_trace_id
→ movements

bq_id = null
```

For one BQ `tool_id`, the backend must enforce **at most one unresolved pre-production trace at a time**.

If an unresolved trace already exists, a new movement for that Tool reuses it rather than creating another unresolved trace.

## Association to production

When the operator explicitly selects the BQ Tool in Job On and the matching `bq_id` is created/resolved:

```text
pending trace.tool_id = selected bq_id.tool_id
→ keep same bq_repair_trace_id
→ set bq_id
→ clear direct trace.tool_id
→ preserve all movements
```

The Tool selection in Job On is already the association decision. No second association command/confirmation is required.

After association, the Tool is resolved through:

```text
trace → bq_id → tool_id
```

Clearing the temporary direct `tool_id` makes it possible for a future unresolved trace for the same physical Tool to be created later.

## Production trace lifecycle

There is no separate trace open/closed state.

A new production/BQ context receives a new trace even if it uses the same physical Tool.

Late returns stay on the earlier trace where their repair activity originated.

## Tool quantity

The fixed BQ lot quantity is owned by the canonical Tool in Ferramentas.

Boquilhas reads that quantity as its accounted base.

Repair movements and exceptional movements must not mutate the Tool quantity.

## Movement rules

Implementation must follow:

- `modules/boquilhas/MOVIMENTOS.md`;
- `modules/boquilhas/REGISTO.md`;
- `modules/boquilhas/HISTORICO.md`;
- `modules/boquilhas/DEFINICOES.md`.

Key current rules include:

- exceptional Saída is accepted and recorded rather than hard-blocked;
- Entrada excess produces the per-movement discrepancy/Saldo;
- `entrada_sem_reparacao` is a return-without-repair movement, not a discrepancy classification;
- only the latest movement may be directly corrected/removed;
- correcting an older movement requires removing every later movement newest-first;
- corrections/removals remain auditable.

## Repairer resolution

A Saída does not accept an arbitrary repairer choice from the operator.

The backend resolves:

```text
BQ tool_id
→ Tool-associated machine
→ Boquilhas Definições machine→repairer
→ resolved repairer used by the movement
```

The repairer resolved when the movement occurred remains historical truth. The exact persistence representation is an implementation decision.

## Reviewer checks

Reject an implementation that:

- creates a new trace for every Saída;
- permits multiple simultaneous unresolved traces for the same BQ `tool_id`;
- asks for a redundant trace-association confirmation after the BQ Tool was selected in Job On;
- keeps the temporary direct `tool_id` on a trace after it has associated to `bq_id`;
- recreates or resets the trace when Job On appears;
- moves late movements to the current production;
- mutates the fixed Tool quantity because of repair movements;
- hard-blocks a physically observed exceptional Saída/Entrada merely to make arithmetic tidy;
- applies Entrada discrepancy semantics to `entrada_sem_reparacao`;
- allows an older movement to be edited while later movements remain active;
- lets a later repairer-setting change rewrite historical movement attribution.
