# Controlo Create — Overview

Controlo Create is the operational surface where control facts are created, measured, edited and submitted where the individual workflow requires submission.

It is one capability surface inside the broader Controlo domain.

## Main areas

- Resumo;
- Peso;
- Comparação;
- Pegamentos;
- Folha;
- Definições.

Each area keeps its own real persistence boundary and lifecycle.

Being grouped under Controlo Create does not make them children of Resumo or force them under one generic persistence parent.

## Shared production context

Controlo Create consumes the Job On production context and the relevant component identities such as `cm_id`, `mf_id` and `bq_id`.

Shared Controlo context rules, including the planned `controlo_id` evolution where applicable, live in `../controlo/OVERVIEW.md`.

## Decision boundary

Create-side workflows record measurements and operational facts.

Approval/rejection/reopen actions that belong to the approval workflow live in Controlo Approve. In the current Beta that approval lifecycle applies to submitted Peso records; Folha has no approval lifecycle.

Comparação is an exception in the sense that its own per-CM operational decision belongs to the Comparação workflow itself and does not create a separate approval flow.
