# Controlo Shared Context / controlo_id Implementation

**Status:** FUNCTIONAL IDENTITY CONFIRMED — REQUIRED WHEN RECOVERING FROM THE OLDER APP BACKUP

**Type:** Controlo persistence / shared-context recovery implementation

## Why this feature is listed

`controlo_id` is a canonical current DMO identity.

The older application backup intended as the recovery base does **not** contain `controlo_id`.

Recovery must therefore add the canonical Controlo context deliberately. The absence of `controlo_id` in the backup must not be interpreted as evidence that the identity is unnecessary.

This brief defines the boundary for that recovery work.

## Purpose

Implement the canonical `controlo_id` without turning it into an artificial parent for every record shown under Controlo.

`controlo_id` identifies the durable shared Controlo context of one production where facts genuinely owned by Controlo as a shared production context belong.

## Existing production context remains real

Controlo already operates through the Job On production identities:

- `jobon_id`;
- `cm_id`;
- `mf_id`;
- `bq_id`.

Those identities are not replaced by `controlo_id`.

Function-specific records keep their own identities, such as:

- `peso_id`;
- `comparacao_id`;
- other durable identities only where the actual workflow requires them.

Resumo remains a read/composition surface. There is no `resumo_id`.

## Required design work before schema changes

The implementation must identify which durable facts genuinely belong to the shared Controlo context.

Particular care is required around Folha/Resumo:

- facts already owned by Job On or Tool must continue to be read through their real relations;
- facts created by Controlo may belong in the Controlo context;
- an external value should only be frozen/copied when historical truth genuinely requires the value used at that time;
- do not create a full Job On/Tool snapshot merely because an old Folha/Resumo must remain viewable.

The design must be able to reconstruct/read an old Controlo truthfully without creating duplicate sources of truth for stable facts.

## Recovery / migration boundary

The older backup may already contain Peso, comparison, approval or other Controlo-related data without `controlo_id`.

Recovery must preserve that valid operational history.

Planning/Architect must determine how canonical Controlo contexts are introduced and associated with existing production data without:

- replacing existing `jobon_id`, `cm_id`, `mf_id`, `bq_id`, `peso_id` or `comparacao_id`;
- fabricating ownership relations merely to attach old rows to `controlo_id`;
- rewriting old operational facts to fit a cleaner schema;
- creating duplicate function records.

The migration must add the missing shared context around existing truthful identities, not reinterpret those identities.

## Forbidden interpretations

`controlo_id` must not become:

- a replacement for `jobon_id`;
- a replacement for `cm_id`, `mf_id` or `bq_id`;
- a replacement for `peso_id` or `comparacao_id`;
- a generic parent added because several screens share navigation;
- a convenience FK added only to simplify queries;
- a blanket snapshot of Job On and Tool state.

## Implementation work

Planning/Architect must:

1. inspect the recovered Controlo persistence/read paths;
2. map Folha/Resumo inputs by ownership;
3. classify the facts needed by the shared Controlo context;
4. define the minimum truthful persistence for `controlo_id`;
5. define migration/compatibility with existing backup records;
6. keep function-specific persistence independent where appropriate;
7. add tests proving that recovery preserves existing identities/history.

## Reviewer checks

Reject an implementation that:

- removes `controlo_id` from the model merely because the backup lacks it;
- creates `controlo_id` only because the UI groups records under Controlo;
- duplicates Job On or Tool truth without historical justification;
- invents `resumo_id`;
- makes every Peso, Comparação, Pegamentos or Folha record a child of `controlo_id` by default;
- removes existing truthful relations solely to route everything through `controlo_id`;
- replaces valid old record identities during recovery instead of introducing the missing Controlo context around them.
