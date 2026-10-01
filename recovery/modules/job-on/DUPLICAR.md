# Job On — Duplicar

Duplicate Job On creates a new production from an explicitly selected existing Job On.

The source is chosen by the user. The application must not assume that the immediately previous production is the correct source.

## Identity behavior

Duplication creates:

- a new `jobon_id`;
- new production component context identities as applicable.

The source Job On is not modified.

Tool selection is carried forward by canonical `tool_id`, not by reusing the source production's `cm_id`, `mf_id` or `bq_id`.

New component contexts are created for the new production.

## Copied values

Copied values are starting defaults for convenience.

They are not immutable inherited facts.

A user with Job On Create may change the values that are editable for the new production before continuing.

## Result

After duplication, the workflow continues on the newly created Job On so the user can review and adapt it for the new production.
