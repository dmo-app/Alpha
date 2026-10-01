# Peso / Tool Technical Values Alignment

**Status:** REQUIRED RECOVERY ALIGNMENT

**Type:** Controlo Create recovery alignment / regression prevention

## Purpose

Ensure Peso consumes Tool-owned technical values through the real owner/context relations and preserves historical truth without duplicating ownership.

## Peso identity and production association

Peso may begin before Job On with a temporary direct anchor to the canonical CM `tool_id`.

When that CM Tool is explicitly selected in Job On:

```text
peso_id → cm_id → CM tool_id
direct peso.tool_id = null
```

The same `peso_id` continues through the production and approval lifecycle. The Job On Tool selection itself is the association decision; no second association prompt is required.

## Required technical-value paths

```text
volume_puncao:
peso_id → cm_id → CM tool_id → tool_technical_values.volume_puncao

volume_marisa:
peso_id → jobon_id → bq_id → BQ tool_id → tool_technical_values.volume_marisa

peso_nominal:
peso_id → cm_id → CM tool_id → tool_technical_values.peso_nominal
```

The CM manufacturing process is resolved separately from the CM Tool:

```text
peso_id → cm_id → CM tool_id → Tool.process
```

Supported process values are `NNPB` and `PS`.

## Missing Tool values

Missing Tool technical values must not be invented or privately re-entered inside Peso.

When calculation needs a missing value:

```text
backend identifies missing owner field
→ UI tells user which Tool value is missing
→ user completes it in Ferramentas
→ Peso re-reads the Tool
→ calculation continues
```

The Tool remains valid while the optional technical value is absent.

## Measurement/calculation distinction

Physical weighing uses:

```text
CM + TP
```

TP/Calote belongs to the Job On production context.

The later main calculation uses:

```text
capacity = water weight / water density
glass weight = (capacity + volume_marisa - volume_puncao) * glass density
```

TP does not become a term in this main formula.

## Historical stability

Canonical ownership stays with Tool/Job On as applicable. Where reproducing a completed Peso requires the exact consumed input, Peso may freeze that consumed value as historical evidence.

Do not:

- create a second editable owner for stable Tool facts;
- overwrite historical consumed values with later Tool edits;
- resolve `volume_marisa` from the CM Tool;
- move TP/Calote into Tool technical values.

## Reviewer checks

Reject an implementation that:

- manually re-enters Tool technical values in Peso;
- keeps a direct `tool_id` on Peso after association to `cm_id`;
- creates a second Peso during approval;
- invents a missing technical value;
- obtains `volume_marisa` from CM instead of BQ;
- treats TP as part of the main Peso formula;
- rewrites historical consumed values from current Tool data.
