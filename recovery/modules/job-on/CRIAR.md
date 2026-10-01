# Job On — Criar

Job On Create creates a new production occurrence.

## Beta scope

The Beta Job On keeps only the production context required by the current Beta workflows.

It records:

- production number;
- production reference composed from the selected MF and BQ Tools;
- machine;
- production start date;
- the selected CM, MF and BQ Tools and their production contexts;
- TP/Tampão/Calote;
- only other production values that are strictly required by current Beta Peso or Boquilhas flows.

It does not expand into final-application configuration merely because other piece families or future modules may exist elsewhere.

## Production reference

The production reference is composed from the selected MF and BQ Tool references.

The MF provides the main numeric/product-form reference. The BQ provides the suffix associated with the finish/closure interface.

Example:

```text
MF = 5447
BQ = T173

production reference = 5447T173
```

CM does not define the production reference.

CM is selected independently and may have the same reference as MF or a different one.

Example:

```text
CM = ST100
MF = 320
```

This is valid and must not be treated as a mismatch merely because the references differ.

The production reference therefore must not be reconstructed from CM or from an assumption that CM and MF references are equal.

The composition rule explains the production reference; it does not force the operator to begin the workflow by selecting MF or BQ.

In normal work, the operator may already know the production reference. Job On may use that known reference as search assistance to narrow relevant Tool candidates before the exact canonical Tools are selected.

The reference may therefore help find candidates, but it never determines `tool_id`.

## Flow

1. Identify the production using the information already known by the operator:
   - production number;
   - production reference;
   - machine;
   - production start date.

2. Select the relevant CM, MF and BQ Tools from Ferramentas, using the known reference and compatibility information to narrow candidates where applicable.

3. Create the production context:
   - new `jobon_id`;
   - production-specific `cm_id`, `mf_id` and `bq_id` as applicable;
   - each component context references the explicitly selected canonical `tool_id`;
   - selecting the Tool for the slot is itself the association decision; do not ask the user for a second association confirmation;
   - where a current-Beta pre-production record already exists on the same `tool_id`, its owning workflow associates that same durable record to the newly resolved production context and clears its temporary direct `tool_id` anchor where defined;
   - create the production's `controlo_id` and associate it immediately with this `jobon_id`.

4. Record the production-specific values required by the current Beta workflows:
   - TP/Tampão/Calote.

5. Persist the Job On:
   - the new `jobon_id` becomes discoverable immediately in the Job On planning calendar for its planned production date/machine;
   - the same saved future `jobon_id` becomes available immediately as an explicit production/context choice in Controlo Resumo for advance preparation;
   - this creation-time availability does not switch Boquilhas or other operational modules to the future production.

6. Continue into the downstream operational modules using the saved Job On context.

## TP / Tampão / Calote

The applicable TP/Tampão/Calote value is defined as part of preparing the production in Job On.

This is a production-specific decision. The person preparing the Job On may choose the applicable calote according to the production setup and the glass-distribution behavior that needs to be balanced.

The value therefore belongs to the Job On production context and is reachable through `jobon_id`.

Peso later uses this value as technical context because the physical Peso measurement is performed with CM + TP. TP adds mass to that physical measurement, so its known contribution can be considered when interpreting/correcting the observed value for wear analysis.

This does not make TP/Calote part of the main Peso formula that produces the value sent to production.

This also does not transfer ownership of TP/Calote to Peso and does not require duplicating the production TP/Calote into `controlo_id`.

No canonical physical `tampao_id` is introduced by this rule. The physical piece may be reused across different references/lots, but DMO currently preserves the production value needed by the workflow rather than inventing a separate Tool identity for Tampão.

## Ownership

Job On owns the production occurrence and its selected component contexts.

It does not create replacement Tool identities.

Production-specific values must not be pushed into Ferramentas merely because they are used alongside a Tool.

## Human choice

Tool selection is explicit.

Explicit selection has two consequences that must not be separated into redundant prompts:

```text
user selects Tool
→ canonical tool_id accepted
→ component context created/resolved
→ applicable pre-production record on that Tool may bind to that context
```

For Boquilhas this means a matching unresolved BQ repair trace is attached to the new `bq_id` and its direct `tool_id` anchor is cleared.

For a pre-production Peso anchored to the selected CM Tool, the same `peso_id` becomes associated to the new `cm_id` and its direct `tool_id` anchor is cleared.

Tool selection is explicit.

Filtering and relevance assistance may help the user find valid Tools, but the system must not silently choose the Tool that the user intended.


## Creation-time planning visibility

Successful Job On creation immediately establishes the future production as planned information.

```text
Job On created
→ jobon_id persisted
→ machine + planned production date available
→ Job On planning calendar can project it now
→ associated controlo_id exists immediately
→ Controlo Resumo can expose that production/context now
```

This does not activate the production early.

The Job On calendar and Controlo Resumo selection are read/discovery projections over Job On truth. Neither duplicates the production record or replaces `jobon_id`.

No generic downstream production-transition ping is created merely because a future Job On was saved. Modules whose operation should change only when production actually changes keep their current context until their own transition rule fires.
