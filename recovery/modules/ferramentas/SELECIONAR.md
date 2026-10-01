# Ferramentas — Selecionar

## Goal

Select an existing canonical Tool for an originating workflow such as Job On.

The normal path is to search and reuse a Tool that is already registered.

## Explicit choice

Tool selection is always explicit.

Search, filters, ordering, or a single remaining candidate must never silently choose a Tool for the user.

The selected value passed back to the originating workflow is the canonical `tool_id`.

Candidate discovery and identity must remain separate:

```text
reference
machines / lines compatibility
lot
process (NNPB / PS)
optional MF-reference association

= search / identification attributes

filters reduce candidates
!=
filters determine identity
```

Even if filtering leaves exactly one candidate, the user still explicitly selects that Tool before its existing canonical `tool_id` is accepted by the originating workflow.

The application must never calculate or infer a `tool_id` from a combination such as reference + machine + lot.

## Selection surface

The Ferramentas surface used from Job On may support:

- consultation of registered Tools;
- search;
- filtering;
- opening the relevant Tool information;
- explicit selection;
- creation of a new Tool when the required Tool does not exist.

Creation is a fallback from the selection flow, not the normal outcome.

## Missing Tool

If the required Tool does not exist, the user may invoke Ferramentas creation, create the new canonical Tool, and then continue with the resulting `tool_id`.

The originating workflow context should be preserved so the user can return directly to the selection/use of the new Tool.

## Relationship boundary

Ferramentas owns Tool identity and creates `tool_id`.

It does not create or infer the production-context identities that may later reference that Tool:

- `cm_id`;
- `mf_id`;
- `bq_id`;
- `jobon_id`.

Those are created only by their owning workflow according to the relevant module rules.
