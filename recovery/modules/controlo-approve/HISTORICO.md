# Controlo Approve — Histórico

Approval history preserves decision events without replacing the underlying operational record.

For an approvable record, the history must make it possible to understand the sequence of human decisions, including reopen events where supported.

## Principles

- the operational record keeps its own stable identity;
- approval does not create a copied record;
- previous decisions are not erased by a later reopen or new decision;
- history records facts that actually occurred;
- UI summaries must not become a second source of truth for those events.

The exact event fields depend on the workflow, but actor, time, decision and required reason/context must remain attributable where applicable.
