# Controlo Approve — Resumo

Resumo is the same shared read/composition surface used to understand the control state of a production.

It is not an Approve-specific copy.

It is not a persisted parent and has no canonical `resumo_id`.

On the approval side, Resumo exposes the current records and states needed to understand the production/control state and make the separate approval decision where applicable.

When Create changes an underlying operational fact, the Resumo seen by Approve must reflect that updated state through the real relations.

There is no copy/synchronization step between "Create Resumo" and "Approve Resumo" because those are not separate persisted objects.

The underlying records retain their own identities and ownership.

Approve reads those operational facts; it does not edit them through Resumo.

Resumo composes them through the real production/control relations and does not take ownership of them.
