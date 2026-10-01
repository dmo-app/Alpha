# Comparação UI Completion

**Status:** BACKEND/DOMAIN/PERSISTENCE REPORTED IMPLEMENTED — USER-FACING UI UNFINISHED IN INSPECTED BASELINE

**Type:** Controlo Create UI completion + integration verification

## Functional source

The UI must follow `modules/controlo-create/COMPARACAO.md`.

Comparação:

- belongs to Controlo Create;
- starts against an existing decided `peso_id`;
- has its own `comparacao_id`;
- may include one or more existing `cm_id` subjects;
- records comparison measurements separately from the original Peso;
- requires an explicit per-CM decision: `Manter` or `Colocar de parte`;
- requires justification for `Colocar de parte`;
- never rewrites the original Peso.

## UI work

The unfinished UI must expose the implemented domain flow without inventing a second business model.

It must allow:

- explicit selection/start from an eligible decided Peso;
- adding/selecting the CM subjects required by the workflow;
- recording comparison measurements;
- per-CM explicit decisions;
- required justification when placing a CM aside;
- confirmation only after all participating CM subjects have a final decision;
- access to multiple comparison events over time without overwriting older events.

## Related known bugs

The historical Peso-selection and unequal-CM-count defects are tracked separately in:

`CONTROLO_PESO_COMPARACAO_KNOWN_BUGS.md`

Those regressions must not be introduced while completing this UI.

## Reviewer checks

Reject an implementation that:

- creates new CM identities for comparison;
- modifies the original Peso measurements/status/results;
- requires every CM in the production to participate;
- infers Manter/Colocar de parte from calculated values;
- removes the justification requirement for `Colocar de parte`;
- treats Comparação as a second approval workflow.
