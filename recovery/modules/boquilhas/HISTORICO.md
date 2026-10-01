# Boquilhas — Histórico

Boquilhas history is composed from durable repair traces and the real movement facts recorded inside each trace.

## Repair-trace history

`bq_repair_trace_id` identifies the movement trace of one BQ production context (`bq_id`). It may contain several repair cycles.

Its movements remain grouped under that trace before and after association to production.

A pre-production trace keeps the same identity when later associated to its `bq_id`. A later machine/production change must not move those historical movements onto the new production trace.

`bq_id` identifies the BQ-in-production context and has one production trace. The canonical `tool_id` may repeat across productions, while each production receives a different `bq_id` and trace. Therefore the same canonical BQ Tool may have many historical traces over time. At any one time, however, a BQ `tool_id` may have at most one unresolved pre-production trace. When that trace associates to `bq_id`, its direct `tool_id` anchor is cleared and the Tool remains reachable through `bq_id → tool_id`.

## Movement history

A recorded movement remains part of the historical record of its `bq_repair_trace_id`.

The movement system preserves what was observed rather than silently rewriting the past to make quantities mathematically tidy.

## Discrepancy history

A discrepancy produced by a movement is a historical fact of that repair trace.

It is not a temporary debt to be cancelled by a later movement.

Later movements must not silently compensate or erase an earlier discrepancy merely because their numbers happen to offset one another.

The accumulated discrepancy for a repair trace is derived from the movement discrepancies that occurred inside that trace.

Broader BQ history may compose several traces, but one trace's discrepancy is never silently transferred into another.

## Correction/removal history

Only the latest movement of a trace may be directly corrected or removed.

To correct an older movement, later movements must first be removed from newest to oldest until the target becomes the latest movement.

Corrections and removals remain auditable. The exact technical representation of that audit is an implementation decision.

An explicit correction of the affected movement is different from automatic reconciliation: later ordinary movements must never silently erase an earlier discrepancy.

## Beta consultation boundary

Boquilhas Histórico is module-local in the current Beta.

A User with the BQ module assigned may consult the Boquilhas history directly in the module.

No Boquilhas PDF/email artifact is required, and Job On does not duplicate the BQ repair-trace history as another Beta consultation surface.
