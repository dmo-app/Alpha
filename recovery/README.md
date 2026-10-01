# DMO Blueprint

This repository is the canonical functional blueprint for the DMO application.

It defines the **business rules**, **scope**, **canonical identities**, **relationships**, **module boundaries**, and cross-module functional behavior that implementation must obey.

## Ecosystem

- **this repository (`dmo-app/dmo-recovery`)** — current canonical product and functional blueprint while the fresh-start blueprint is being consolidated;
- **`dmo-app/blueprint`** — fresh-start target/scaffold; it is not a competing source of product truth until the consolidated blueprint is deliberately promoted there;
- **`dmo-app/dmo-design`** — visual and interaction design source: UI prototypes and presentation decisions;
- **`dmo-app/development-dmo`** — development workflow, phase process and role responsibilities.

Older implementation repositories and backups may be consulted only as historical evidence. They are not product authority for the fresh-start application.

Implementation or design may reveal a conflict or missing decision, but they do not silently redefine the product blueprint.

## Core principle

The Beta is a **scope-reduced product**, not a disposable test version of a larger application.

A capability outside `SCOPE.md` must not be introduced into the Beta merely because it existed in an older repository.

Historical repositories may be used as evidence when recovering information, but they do not define current product behavior by themselves.

## Repository map

- `HOW_THE_APP_WORKS.md` — concise global explanation of how the application works.
- `IDENTITIES.md` — canonical identities and identity boundaries.
- `RULES.md` — cross-cutting product and architecture rules.
- `SCOPE.md` — Beta scope.
- `IMPLEMENTATION_STATUS.md` — current Beta implementation state, transitional conditions and known gaps; not product canon.
- `modules/` — the DMO application organized by module.
  - files directly in a module describe functional/product behavior;
  - `backend/` inside a module contains deeper backend contracts, persistence, reads/writes and technical behavior for that same module;
  - `frontend/` inside a module contains deeper frontend states, interactions and integration behavior for that same module.
- `backend/` — cross-cutting backend/runtime technical information that does not belong to only one module.
- `prototype/` — prototype-only technical support, clearly separated from production backend authority.

The blueprint contains decided product behavior only. Unresolved product questions stay outside the canonical blueprint until the owner decides them; once decided, the rule is written directly into the owning module or cross-cutting file.

The detailed functional rule belongs in the file owned by the relevant module or cross-cutting concern. Git history preserves how the blueprint evolved; historical curation records do not define a second product source.


## Application documentation structure

There is no separate product area called `features-to-implement`.

If a behavior belongs to DMO, it belongs to its real module even when code for it is not finished yet.

```text
modules/<module>/
├── functional files
├── backend/
│   └── technical backend detail for that module
└── frontend/
    └── technical frontend detail for that module
```

Implementation state is tracked by `IMPLEMENTATION_STATUS.md`; it does not determine where the app behavior belongs.

This keeps product behavior and technical implementation detail close to the same module without mixing backend and frontend concerns in one file.
