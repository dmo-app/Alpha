# Controlo Approve — Overview

Controlo Approve is the review and decision capability of the broader Controlo domain.

It does not create a second copy of the underlying control record.

Where a workflow is approvable, Create and Approve operate on the same durable record identity and the same underlying production/control state. In the current Beta, the defined approval lifecycle applies to submitted Peso records.

## Responsibilities

Controlo Approve may:

- review submitted Peso information;
- read the current Resumo/control context required to understand the Peso;
- read other operational facts only as context where the composition exposes them;
- approve;
- reject with the required reason where applicable;
- reopen where the workflow allows it;
- expose the decision history.

## Read-only operational boundary

Controlo Approve does not edit the operational content created/maintained through Controlo Create.

Seeing Resumo, measurements, observations or other operational facts does not grant ownership to change them. Folha itself is not an approvable record.

Approve writes only approval-side facts defined by the approval workflow, such as the decision, required reason/context, actor, timestamp and decision history.

Conceptually:

```text
Create writes/edits operational control state
-> Approve reads that same updated state

Approve
-> writes approval decision/history
-> does not rewrite Create-owned operational facts
```

## Shared context

The approval surface reads the same production and control context as the create side.

Resumo is a shared composition over that same state; it is not an Approve copy. Folha remains a Create-side control/evaluation surface and is not approved, rejected or reopened here.

Shared Controlo context rules belong in `../controlo/OVERVIEW.md`.

UI parity or a shared read model does not create a new persistence identity.
