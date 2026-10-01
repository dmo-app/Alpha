# Controlo — Folha

Folha is a Controlo Create control/evaluation surface.

It does **not** have an approval lifecycle.

There is no Folha approve/reject/reopen workflow and no approval copy of Folha.

## Capability behavior

Controlo Create may:

- read Folha;
- edit its operational fields;
- record/evaluate the applicable OK/NOK facts;
- save the resulting control state.

Controlo Approve must not treat Folha as an approvable record.

Folha information may be visible through the broader Controlo/Resumo composition where useful, but visibility does not create a Folha approval state.

## Families and identity boundary

Folha covers the visible families:

- CM;
- BQ;
- MF;
- PU;
- CS.

CM, BQ and MF use their real production Tool contexts:

```text
CM → cm_id
BQ → bq_id
MF → mf_id
```

PU and CS are different.

They are fixed Folha fields named **PU** and **CS** where the user records the applicable OK/NOK control fact (and the observation/content already required by the Folha design).

They are **not** Tool identities or dynamic canonical entities.

Do not create:

- `pu_id`;
- `cs_id`;
- PU/CS Tool selection;
- a dynamic identity mapping for those fields.

## Per-piece facts

Each applicable Folha field may carry:

- OK/NOK;
- observation where applicable.

NOK does not automatically stop production.

OK does not automatically authorize production.

These are recorded/evaluated control facts, not approval decisions.

## Beta boundary

Folha reuses the production context already available from Job On for CM, BQ and MF.

It must not create replacement Tool identities or duplicate the production configuration.

MCaliper is outside the current Beta. Folha therefore has no Beta requirement for:

- MCaliper links;
- MCaliper integration;
- MCaliper URL validation/history;
- MCaliper permissions;
- MCaliper content in Resumo.

Folha and Resumo must function without MCaliper.
