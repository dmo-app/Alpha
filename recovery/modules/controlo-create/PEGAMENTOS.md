# Controlo Create — Pegamentos

Pegamentos belongs to Controlo Create.

It has no separate approval capability or workflow and may legitimately be absent for a production.

## Measurement rules

- Costura = axis 0°.
- Contra costura = axis 90°.
- Ovalização = Costura - Contra costura.
- Média = (Costura + Contra costura) / 2 when both axes are measurable.
- Tolerance = nominal ± 0.20 unless the configured tolerance value is changed.
- Dimensional values are expressed in millimetres (mm).
- Entered measurements, nominal values, tolerance bounds and derived dimensional results use two decimal places.

Tolerance is evaluated per measurement row against the measurement value applicable to that row.

For the normal two-axis case:

- Costura and Contra costura are the source measurements;
- Ovalização is derived, uses two decimal places and preserves its positive/negative/zero sign;
- each row's Média is the value compared against the component nominal/tolerance corridor;
- reaching either tolerance boundary already raises an alert;
- going outside either boundary also raises an alert;
- component-level averages may be shown as summary information, but a valid overall average does not cancel an alert produced by an individual measurement row.

Tolerance alerts are informative and non-blocking. They do not decide, approve, reject or stop production automatically.

### When Contra costura cannot be measured

Some CM geometries do not allow a valid Contra costura / 90° measurement.

This must not block Pegamentos.

In that case:

- record Costura only;
- Contra costura is treated as not applicable / not measurable, not as an error;
- Ovalização is not calculated;
- Média = Costura, because it is the only measurable value available for that CM;
- tolerance evaluation uses that single measurable value for that row;
- the same millimetre and two-decimal representation rules apply.

The workflow must not require a fabricated second-axis value merely to complete the record.

## Context

Pegamentos uses the real production component identities:
- `cm_id`;
- `mf_id`;
- `bq_id`.

Known technical inputs are resolved from the relevant production context and canonical Tool data. The frontend must not invent them or become a second source of calculation truth.

The component Tools are already established by the active Job On context. Pegamentos does not reselect CM, MF or BQ merely to perform the measurement.

The relevant throat-diameter nominal values are Tool technical values resolved separately through the applicable component context:

```text
cm_id → CM tool_id → diametro_gargalo
bq_id → BQ tool_id → diametro_gargalo
mf_id → MF tool_id → diametro_gargalo
```

If Pegamentos requires a missing Tool diameter, it tells the user which Tool-owned value is missing. The user completes that value in Ferramentas, then Pegamentos re-reads the Tool and continues. Pegamentos must not invent or privately re-enter the canonical Tool diameter.

## Implementation during testing

Consider an assisted-entry behavior for Tool throat diameters:

- when one CM/BQ/MF diameter is entered first, suggest the related expected diameters using the configured dimensional difference;
- apply the same relation regardless of which of the three components is entered first;
- treat the generated values as suggestions only;
- never overwrite an already registered canonical Tool technical value automatically.

This is a testing-stage usability enhancement, not a requirement for the initial development path.
