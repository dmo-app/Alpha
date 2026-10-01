# Ferramentas — Overview

## Purpose

Ferramentas is the canonical registry and consultation surface for the physical Tools known to DMO.

Opening Ferramentas presents the **registry of existing Tools in a table**. Creation is an action available from that same registry surface; the user can inspect what already exists and create a missing Tool without treating creation as a separate product area.

## Canonical identity

Every Tool is identified by `tool_id`.

The registry covers the Tool types currently in scope:

- CM
- MF
- BQ

A different lot represents a different canonical Tool and therefore a different `tool_id`.

This is an identity invariant:

```text
new lot
→ new Tool
→ new tool_id
```

A lot change is never an in-place update of an existing Tool identity.

Reference, compatible machines/lines, lot, quantity and other visible Tool facts help a person find and distinguish candidates, but they do not form a derived Tool identity key.

## Tool quantity

A Tool record also carries the quantity of physical tools represented by that Tool/lot.

Example:

```text
Tool type = BQ
lot = 4
quantity = 120
```

This means that the canonical BQ Tool for lot 4 represents an accounted quantity of 120 physical BQ tools.

`quantity` is Tool-owned master data stored with the canonical `tool_id`. It is an attribute of the Tool, not a second identity and not one `tool_id` per physical piece.

For BQ Tools, the Boquilhas module consumes this quantity as the accounted lot total used together with repair movements to derive operational values such as quantity in house and quantity out for repair. Boquilhas does not become the owner of the Tool quantity merely because it consumes it.

Repair movements must not silently increase the Tool quantity when an unexplained Entrada is observed; the discrepancy rules remain owned by Boquilhas.

For CM Tools, an exceptional compatibility case may be recorded through one or more optional MF-reference associations. These associations exist only to help candidate discovery when the CM's real reference differs from the relevant MF/production reference.

An MF-reference association:
- does not replace the CM's canonical reference;
- does not create another Tool identity;
- is optional and may be added later;
- must not be displayed as though it were the real CM reference;
- is search/compatibility metadata only.

Conceptually:

```text
SEARCH
→ reference / optional MF-reference association / machine / lot

SELECTION
→ explicit human choice of an existing canonical Tool

IDENTITY
→ persisted tool_id
```

Therefore:

```text
filters reduce candidates
!=
filters determine identity
```

## Main surface

The Ferramentas surface must allow the user to consult the existing Tool park and narrow it using operational metadata that belongs to the current application scope.

The relevant filtering dimensions include:

- Tool type;
- reference;
- lot;
- compatible machines/lines;
- process.

The registry/detail view exposes the Tool quantity as Tool-owned data.

The current Tool process values are:

- `NNPB`;
- `PS`.

Process is a Tool fact used for display/filtering and candidate assistance. It is not part of a derived identity key.

Filtering narrows the visible candidates. It never selects a Tool automatically.

The surface may expose actions such as:

- view Tool details;
- edit a Tool where the workflow allows it;
- create a missing Tool;
- delete a Tool where the delete operation is valid.

For the current Beta, Tool creation and Tool deletion are available to any user who has access to the DMO application. A separate Ferramentas permission is not required for those two actions in the Beta. This broad access exists to support testing and does not define the permanent full-application authorization model.

This access rule answers **who may invoke** create/delete. It does not, by itself, define whether a Tool that is already referenced by persisted production/history may be physically deleted; relation/history safety must follow the canonical identity and historical-truth rules.

## Contextual use

Ferramentas remains contextual in the current application. It is reached from workflows that need a Tool, such as Job On or Boquilhas, rather than becoming a mandatory top-level destination.

When Ferramentas is opened from another workflow, the origin context is preserved.

The user explicitly selects the required Tool. The selected canonical `tool_id` is then returned to the originating workflow.

Ferramentas never creates `jobon_id`, `cm_id`, `mf_id`, or `bq_id`. Those identities belong to their owning workflows.

## Creation is an action, not the page identity

Creating a Tool does not redefine Ferramentas as a creation form.

If the required Tool does not exist, the user may invoke **Create Tool** from the registry, create the missing canonical Tool, and continue the originating workflow with the resulting `tool_id`.

The UI implementation of this action belongs to `dmo-app/dmo-design`; this blueprint defines the functional behavior.

## Optional technical values

A Tool may have specialized reusable technical values stored in the optional `tool_technical_values` extension.

That extension:

- is 1:1 with the Tool;
- is keyed directly by `tool_id`;
- creates no second identity or UUID;
- has no independent lifecycle;
- is not loaded as part of the normal registry/search path;
- is retrieved only when a consuming workflow needs those values;
- contains reusable Tool-owned technical facts, not production-specific or module-specific facts.

A separate table does not imply a separate domain identity.

Technical values are optional at Tool creation and may be completed or corrected later on the same Tool. If a consuming operation needs a missing value, the application identifies the missing Tool value, the user completes it in Ferramentas, and the operation re-reads the Tool before continuing. Consumers must not invent or independently re-enter missing Tool values.

The dedicated contract is in [VALORES_TECNICOS.md](./VALORES_TECNICOS.md).

## Ownership boundary

Ferramentas owns canonical Tool identity and Tool-owned reusable facts, including the Tool quantity.

It does not absorb:

- production-specific configuration;
- Controlo measurements or decisions;
- Boquilhas movement history;
- arbitrary module-specific records.

A consuming module may reach Tool-owned information through the real persisted relations without taking ownership of that information.

## Current implementation association

In `dmo-app/dmo-app-beta`, the current implementation is anchored by:

- `src/DMO.Application/Tools/IToolService.cs`
- `src/DMO.Application/Tools/ToolService.cs`
- `src/DMO.Application/Tools/ToolTechnicalValuesReadModel.cs`
- `src/DMO.Infrastructure/Persistence/ToolJobOn/`
- migration `20260927114825_011_ToolTechnicalValues`

These paths describe current implementation reality; this document remains the functional blueprint for Ferramentas.
