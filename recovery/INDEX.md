# DMO Blueprint — Index

This repository is the canonical functional blueprint for DMO.

Read it to understand how the application must behave before changing product behavior, architecture, backend, frontend, persistence, identities, flows, or module behavior.

## Global blueprint map

- `HOW_THE_APP_WORKS.md` — global end-to-end explanation of the application.
- `IDENTITIES.md` — canonical identities and identity boundaries.
- `RULES.md` — cross-cutting product and architecture rules.
- `SCOPE.md` — Beta scope.
- `IMPLEMENTATION_STATUS.md` — current Beta implementation state, bugs, transitional conditions, and known gaps; not product canon.
- `README.md` — repository entry point.

There is no separate governance/definition file outside this map that an agent must discover before using the blueprint.

The blueprint contains decided product behavior only. Unresolved product questions are not kept as a canonical "open decisions" queue; once the owner decides them, the resulting rule is written directly into the file that owns that behavior.

## Module technical structure

DMO behavior stays inside the module it belongs to.

A module may contain:

```text
modules/<module>/
├── functional/product documents
├── backend/
│   └── backend contracts and technical implementation detail
└── frontend/
    └── frontend states, interaction and integration detail
```

A behavior does not move to a separate "to implement" area merely because its code is unfinished.

Cross-cutting backend/runtime technical material lives under `backend/`. Prototype-only backend support lives under `prototype/backend/`.

## Detailed module blueprint

Module detail is organized under `modules/`.

- `modules/job-on/`
- `modules/ferramentas/`
- `modules/controlo/`
- `modules/controlo-create/`
- `modules/controlo-approve/`
- `modules/boquilhas/`
- `modules/admin/`

See `modules/INDEX.md` for the task-level map.

## Reading rule

Start with `HOW_THE_APP_WORKS.md` for the global model.

Then read the relevant module folder.

Start with the functional files directly in the module. For implementation work, read the relevant `backend/` and/or `frontend/` documents inside that same module.

Use `IMPLEMENTATION_STATUS.md` only to understand what code currently exists, is missing, defective or unverified; it does not redefine the module.

Do not infer missing behavior.

Historical labels such as Operador, Responsável, and Controlador are not current authorization identities. Preserve the underlying functional action, but express current behavior through the current module/capability model.

The detailed module files are populated from verified current knowledge. Empty or placeholder task files must not be treated as defined product behavior until populated.
