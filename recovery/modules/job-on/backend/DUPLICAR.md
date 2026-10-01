# Job On Duplication Alignment

**Status:** VERIFY CURRENT IMPLEMENTATION — IMPLEMENT CHANGES WHERE MISSING

**Type:** Job On workflow alignment

## Functional source

Implementation must follow `modules/job-on/DUPLICAR.md`.

## Required behavior

Duplicate Job On:

1. requires explicit source selection;
2. creates a new `jobon_id`;
3. carries Tool selection by canonical `tool_id`;
4. creates new production-context identities for the new production;
5. leaves the source Job On unchanged;
6. opens/continues on the newly created Job On so the user can review and edit allowed values.

Source `cm_id`, `mf_id` and `bq_id` values must not be reused as the new production-context identities.

Copied values are editable starting defaults, not immutable inherited facts.

## Verification outcome

This task may end as:

- `VERIFIED_IMPLEMENTED`;
- `IMPLEMENTED`;
- `BLOCKED_BY_BLUEPRINT_GAP`.

## Reviewer checks

Reject an implementation that:

- silently assumes the immediately previous Job On is the duplication source;
- reuses the source `jobon_id`;
- reuses source `cm_id`, `mf_id` or `bq_id` as the new production contexts;
- modifies the source production;
- treats copied values as permanently frozen inheritance.
