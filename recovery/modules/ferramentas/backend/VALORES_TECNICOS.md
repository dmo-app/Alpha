# Tool Technical Values — Implementation Slice

**Readiness:** FUNCTIONAL OWNERSHIP AND CURRENT FIELD SET CLOSED

**Owning domain:** Ferramentas

**Canonical sources:**
- `modules/ferramentas/OVERVIEW.md`
- `modules/ferramentas/VALORES_TECNICOS.md`
- `IDENTITIES.md`

## Objective

Support optional reusable technical values owned by a canonical Tool without creating a second Tool identity or copying those values into consuming workflows.

```text
tool_id
  └── optional tool_technical_values
```

The extension has no independent canonical ID.

## Current type-specific fields

```text
CM
→ volume_puncao
→ diametro_gargalo
→ peso_nominal

BQ
→ volume_marisa
→ diametro_gargalo

MF
→ diametro_gargalo
```

The CM manufacturing process `NNPB | PS` remains a required CM Tool fact on the normal Tool record, not a technical-values identity.

Technical values use two decimal places.

## Create/update behavior

Technical values are optional when a Tool is created.

The implementation must support:

- creating a Tool with none of its optional technical values;
- entering known values during creation;
- adding missing values later to the same `tool_id`;
- editing applicable values later without replacing the Tool.

`technical values absent != invalid Tool`.

## Missing values

A consuming workflow that requires a missing value must return a purpose-specific missing-Tool-value outcome that identifies the Tool-owned field required.

The intended interaction is:

```text
consumer needs missing Tool value
→ tell user which Tool value is missing
→ user completes it in Ferramentas
→ consumer re-reads Tool
→ continue
```

Do not invent defaults, copy another Tool's value, or persist a private mutable duplicate inside the consumer.

## Consumer paths

Peso resolves values through the actual owners:

```text
volume_puncao:
peso_id → cm_id → CM tool_id → volume_puncao

volume_marisa:
peso_id → jobon_id → bq_id → BQ tool_id → volume_marisa

peso_nominal:
peso_id → cm_id → CM tool_id → peso_nominal
```

Pegamentos resolves `diametro_gargalo` separately through `cm_id`, `bq_id` and `mf_id`.

Normal Ferramentas list/search remains light and does not automatically load specialized technical values for every Tool.

## Historical stability

Canonical ownership stays with the Tool. A consumer may freeze the exact value it consumed only where that is required to reproduce historical operational truth. Such a frozen consumed value is evidence of what the operation used; it is not a second editable owner.

## Reviewer checks

Reject an implementation that:

- introduces `technical_values_id`;
- applies every technical field to every Tool type;
- makes technical values mandatory for Tool creation;
- creates a new `tool_id` merely to add/edit technical values;
- resolves `volume_marisa` from the CM Tool instead of the BQ Tool;
- manually re-enters Tool-owned technical values in Peso or Pegamentos;
- invents a missing value;
- loads the extension into every normal Tool list/search query;
- rewrites historical consumed values when a Tool value is edited later.
