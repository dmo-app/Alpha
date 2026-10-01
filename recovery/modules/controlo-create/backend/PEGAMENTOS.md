# Pegamentos — Implementation Slice

**Readiness:** BLOCKED — measurement behavior is defined, but the durable Pegamentos record identity/ownership boundary is not yet explicit enough to implement persistence without invention.

**Owning domain/capability:** Controlo / Controlo Create

**Canonical source:** `modules/controlo-create/PEGAMENTOS.md`

## Objective

Build the Pegamentos workflow for the CM, BQ and MF production contexts already established for the applicable production.

Pegamentos records dimensional measurements, derives Ovalização and Média, evaluates non-blocking tolerance alerts and preserves the resulting operational record without creating private Tool or production identities.

Pegamentos has no separate approval workflow and may legitimately be absent for a production.

## Production context

Pegamentos consumes the real production component contexts:

```text
cm_id
bq_id
mf_id
```

Those contexts already resolve the canonical Tools selected for the production.

Pegamentos must not reselect CM, BQ or MF merely to perform the measurement.

Known Tool technical values required by the workflow are resolved through those real contexts and canonical Tool ownership.

## Measurement inputs

Normal two-axis measurement row:

```text
component context
measurement number / row identifier
Costura (0°)
Contra costura (90°)
```

Where the CM geometry makes a valid 90° measurement impossible:

```text
Costura
→ supplied

Contra costura
→ not applicable / not measurable
```

The frontend must not fabricate a second-axis value.

## Units and precision

Dimensional values use millimetres (mm).

Entered measurements, nominal values, tolerance bounds and derived dimensional results use **two decimal places**.

## Derived values

Normal two-axis row:

```text
Ovalização = Costura - Contra costura

Média = (Costura + Contra costura) / 2
```

Ovalização is a signed derived result and may be:

- positive;
- negative;
- zero.

Its sign must be preserved.

Single-axis CM row:

```text
Contra costura = not applicable
Ovalização = not calculated
Média = Costura
```

These derived values are backend/domain calculation truth. The frontend may display them but must not become a second independent calculation authority.

## Tolerance behavior

Default tolerance:

```text
nominal ± 0.20 mm
```

unless the owning configuration explicitly supplies another tolerance.

Tolerance is evaluated **per measurement row**.

For the normal two-axis case:

```text
row Média
→ compare with component nominal/tolerance corridor
```

For the valid single-axis CM case:

```text
row Costura
→ compare with component nominal/tolerance corridor
```

Reaching either boundary already raises an alert.

Going outside either boundary also raises an alert.

The alert is informative and non-blocking.

A component-level/global average may be shown as summary information, but a valid summary average must not cancel an alert produced by an individual measurement row.

## Reads

Pegamentos needs only the context required for the measurement workflow, including:

- applicable CM/BQ/MF production contexts;
- canonical Tool facts required to identify the component;
- applicable throat-diameter nominal/technical value;
- applicable tolerance configuration;
- existing Pegamentos record/rows when editing or reopening the same durable record, once its identity boundary is decided.

Reads must remain purpose-specific.

Do not load unrelated Tool, Job On or Controlo state merely because it is reachable.

## Writes

The eventual persistence must store the real user-entered measurement facts required to reconstruct the Pegamentos record truthfully.

Derived values such as Ovalização, Média, alerts and component-level summaries must not automatically become independent mutable sources of truth merely because they are displayed.

The exact durable record anchor/identity remains blocked below and must be decided before schema implementation.

## Frontend contract requirements

The Pegamentos frontend contract must define at least:

- how the applicable production context is entered/opened;
- how CM, BQ and MF sections are shown;
- how measurement rows are added/edited;
- how an impossible 90° CM measurement is represented;
- two-decimal millimetre input/display;
- signed Ovalização display;
- row-level tolerance alert presentation;
- component summary averages;
- loading/error/conflict states;
- submit/save behavior once the durable record lifecycle is closed.

The frontend must not:

- reselect production Tools;
- send Tool metadata as authority;
- send actor/time as authority;
- decide approval/rejection from tolerance alerts;
- duplicate backend-owned calculations as a second truth.

## Backend contract requirements

The backend contract must eventually define:

- durable Pegamentos operation anchor;
- canonical ID allocation if Pegamentos has its own identity;
- create/read/update operations;
- measurement-row identity or stable row addressing where edits are allowed;
- Tool/context resolution from the real production identities;
- calculation of Ovalização/Média;
- tolerance evaluation;
- guarded-write concurrency behavior;
- typed refusals for missing context, invalid input and stale write;
- purpose-specific read model returned to the frontend.

## Explicit non-goals

This slice does not:

- create CM/MF/BQ identities;
- modify Tool identity/reference/lot;
- create a second approval workflow;
- turn tolerance alerts into automatic industrial decisions;
- create private copies of Tool technical values;
- require the testing-stage assisted diameter suggestion behavior for the initial implementation;
- define a generic measurement framework for unrelated Controlo workflows.

## Missing product decisions blocking implementation

Before this slice is READY for persistence implementation, the canonical blueprint must close:

1. **durable Pegamentos identity/anchor** — whether Pegamentos has its own canonical ID or is uniquely anchored by another already-canonical Controlo/production identity; do not invent `pegamentos_id` merely for symmetry;
2. **cardinality/history** — whether one production may have one Pegamentos record or multiple durable Pegamentos events over time;
3. **edit lifecycle** — when an existing Pegamentos record/row may be edited and whether any explicit completion/submission state exists;
4. **measurement-row addressing** — whether the visible measurement number is a business identifier, a label, or merely presentation; define the stable edit/history rule without deriving identity from display text;
5. **tolerance configuration ownership** — where the configurable deviation from the default ±0.20 mm is owned and how it is resolved for a component/production;
6. **component applicability** — whether every applicable Pegamentos record always contains CM + BQ + MF, or whether any component may be legitimately absent.

If a decision already exists elsewhere, point this slice to that canonical source instead of restating it here.

## Acceptance evidence

When the missing decisions are closed and this slice is implemented, reviewers must be able to prove that:

- dimensional values use mm and two decimal places;
- signed negative/positive Ovalização is preserved;
- valid single-axis CM measurement does not require fabricated 90° data;
- tolerance is evaluated per measurement row;
- a summary average does not erase an individual row alert;
- reaching/exceeding a boundary produces a non-blocking alert;
- CM/BQ/MF are resolved from existing production contexts rather than reselected;
- Tool technical values remain Tool-owned;
- derived values do not become duplicate mutable truth without an explicit need;
- stale guarded writes do not silently overwrite newer data;
- no Pegamentos identity/lifecycle is invented beyond the canonical decision.
