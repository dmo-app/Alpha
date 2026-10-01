# Ferramentas — Valores Técnicos

## Purpose

Reusable technical values belong to the canonical Tool. They are optional Tool-owned facts used by specialized workflows and are not required by the normal Ferramentas registry/list read.

They live in an optional 1:1 Tool extension.

## Identity

The extension has **no independent identity**.

```text
tool_id
  └── tool_technical_values
```

`tool_id` is both the relationship and the identity key of the extension.

Do not introduce:

- `technical_values_id`;
- a second UUID;
- an independent lifecycle for the extension.

A separate physical table does not create a separate domain identity.

## Tool-type ownership

The confirmed technical values are type-specific.

### CM

- `volume_puncao`;
- `diametro_gargalo`;
- `peso_nominal` from the applicable technical drawing/reference.

The CM manufacturing `process` (`NNPB` or `PS`) is also a Tool-owned fact, but remains part of the normal CM Tool record rather than this optional extension.

### BQ

- `volume_marisa`;
- `diametro_gargalo`.

### MF

- `diametro_gargalo`.

PU has no independent Tool identity in the current Beta. `volume_puncao` is therefore owned by the CM Tool rather than by a separate PU Tool.

When entered, stored and shown as numeric Tool technical values, these values use **two decimal places**.

For Peso presentation:

```text
volume_marisa → Volume BQ
volume_puncao → Volume PU
```

The Peso PDF displays Volume BQ in cm³. The display label/unit does not create a second stored field or identity.

## Optional at creation; completable later

Technical values are **not mandatory when the Tool is created**.

A Tool remains a valid canonical Tool when one or more optional technical values are missing.

The user may:

- enter known technical values during Tool creation;
- complete missing values later;
- edit the applicable values later on the same `tool_id`.

A missing extension row or missing field means only that the value has not yet been registered. The system must not invent a substitute.

## Missing value required by an operation

When an operation needs a Tool technical value that is missing:

```text
operation needs Tool value
→ backend identifies the missing Tool-owned field
→ application tells the user which Tool value is missing
→ user completes that value in Ferramentas
→ operation re-reads the Tool
→ operation continues
```

The consuming workflow must not:

- calculate a fake replacement value;
- copy a value from another Tool;
- ask the user to re-enter the value as a private consumer-owned field;
- create a replacement Tool merely because the technical value was missing.

## Consumption by workflows

Consumers follow the real component context to the correct Tool.

Peso:

```text
volume_puncao
peso_id → cm_id → CM tool_id → tool_technical_values.volume_puncao

volume_marisa
peso_id → jobon_id → bq_id → BQ tool_id → tool_technical_values.volume_marisa

peso_nominal
peso_id → cm_id → CM tool_id → tool_technical_values.peso_nominal
```

Pegamentos resolves `diametro_gargalo` independently for the applicable CM, BQ and MF contexts:

```text
cm_id → CM tool_id → diametro_gargalo
bq_id → BQ tool_id → diametro_gargalo
mf_id → MF tool_id → diametro_gargalo
```

The frontend does not calculate or infer these owner values.

The values remain owned by Ferramentas/Tool and must not be duplicated into consumer-module persistence merely for query convenience. Where historical truth requires preserving the exact value actually consumed by an operation, that consuming workflow may freeze the consumed value explicitly; that does not transfer canonical ownership away from the Tool.

## Boundary

`tool_technical_values` contains reusable, relatively stable technical facts of the Tool.

It must not become storage for:

- production-specific configuration;
- measurement results;
- module-specific workflow state;
- unrelated historical events.

The exact field set grows only when a real current-scope workflow requires another reusable Tool-owned technical fact.
