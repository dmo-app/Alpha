# Controlo Approve — Aprovar

Approval acts on the existing durable record.

It must not create an approval copy.

## Peso

For Peso, the same `peso_id` persists through the decision lifecycle.

The approval-status vocabulary is:

```text
por_aprovar
aprovado
nao_aprovado
```

A Peso enters `por_aprovar` only when Controlo Create explicitly submits it.

Available decisions include:

- approve;
- reject;
- reopen where permitted.

A rejection requires a non-blank reason.

Decision events must preserve who decided, when the decision occurred and the relevant prior/new state required by the workflow.

## Reopen

Reopen preserves the previous decision event in history and returns the same Peso to editable Controlo Create work.

Reopen does not immediately place the Peso back in the approval queue.

The user may save corrections as needed. When the corrected Peso is explicitly submitted again:

```text
Submit
→ status = por_aprovar
→ available again in Controlo Approve
```

The earlier approve/reject/reopen events remain historical evidence and are never erased.

## Human decision

Calculated values, alerts and technical NOK/OK states may inform the reviewer but must not silently choose the approval decision.
