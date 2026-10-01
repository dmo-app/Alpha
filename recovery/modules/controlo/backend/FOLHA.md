# Folha Persistence / Evaluation Implementation

**Status:** FUNCTIONAL BLUEPRINT DEFINED — PERSISTED EVALUATION LAYER MISSING IN INSPECTED BASELINE

**Type:** Controlo Create + Approve feature implementation

## Functional source

Implementation must follow `modules/controlo-create/FOLHA.md` and the shared Controlo rules.

Folha is one shared Controlo surface/state exposed through both capabilities.

It must not be implemented as separate Create and Approve copies.

Create-side actions:

- read;
- edit;
- evaluate;
- submit.

Approve-side behavior:

- read the same current Folha/control state;
- approve;
- reject;
- reopen where permitted.

Approve does not edit the operational Folha content. Approval actions persist approval-side decisions/history only.

A Create-side edit must be visible when Approve reads the same control state without copying/synchronizing a second Folha object.

## Families

Folha covers:

- CM;
- BQ;
- MF;
- PU;
- CS.

Each applicable piece may carry:

- OK/NOK;
- observation;

OK/NOK is recorded/evaluated information. It does not automatically authorize or stop production.

## Beta boundary

Folha reuses the production context already present in the Beta Job On.

The Beta must not expand Job On with final-application fields merely because Folha needs additional evaluation data.

Additional data required only for the current Beta Folha may be entered manually in Folha.

That manual-entry behavior is a Beta scope/UX solution. It does not establish permanent ownership for the full application.

## Shared Controlo context interaction

The Folha implementation must be designed together with the pending `controlo_id` shared-context work.

Do not assume that every Folha-visible value must be copied into `controlo_id`.

Use the real owner/relation where historical reconstruction remains truthful. Persist/freeze only facts that genuinely require Controlo-owned historical stability.

## Reviewer checks

Reject an implementation that:

- creates separate persisted Folha copies for Create and Approve;
- lets Approve edit Create-owned operational Folha content;
- requires a copy/synchronization step for Create changes to appear in Approve;
- expands Job On merely to host Beta-only Folha input;
- treats Folha manual entry as permanent full-application ownership;
- turns OK/NOK into automatic production approval/rejection;
- creates new Tool identities to satisfy missing Folha inputs;
- duplicates stable Job On/Tool truth without a historical reason.
