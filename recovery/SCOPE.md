# Beta Scope Boundaries

**Definition:** The Beta is a **scope-reduced** DMO product. Capabilities listed as OUT remain outside this Beta unless the blueprint is explicitly changed.

## IN SCOPE

1. **Identity & Access / Admin** — Users, Access Templates and App Definições. Users are associated with one current Access Template or none; Templates define module/capability access; App Definições centralizes selected module-owned administrative settings without creating a standalone Modules tab.
2. **Ferramentas / Tools** — canonical `tool_id` registry for CM, MF and BQ; consultation/filtering is the primary surface and creation is an action inside the registry. Selection is explicit and Ferramentas remains contextual to consuming workflows.
3. **Job On (Light)** — production planning and production-context creation: `jobon_id`, `cm_id`, `mf_id`, `bq_id`. The current Beta requires only the **Job On Create** capability for its Job On operational user; Create already includes consultation. **Job On View** remains a valid independent read-only capability in the complete access model and does not need to be assigned alongside Create.
4. **Controlo**
   - **Resumo** — tab/function inside Controlo; the consolidated output is a product of Controlo, not the production-level node itself.
   - **Peso** — measurement, calculation and submission.
   - **Comparação** — optional child workflow of an existing **approved** Peso.
   - **Pegamentos** — in Beta scope; backend still not implemented.
   - **Folha** — Controlo Create control/evaluation surface; it has no approval lifecycle of its own.
   - **Approve** — review and explicit human decision over submitted Peso records.
5. **Boquilhas** — one `bq_repair_trace_id` per BQ production context (`bq_id`), many movements per trace, at most one unresolved pre-JobOn trace per canonical BQ `tool_id`, automatic association when that Tool is selected into production, late-return continuity, derived quantities/discrepancy and module-local history. Boquilhas consultation stays inside the BQ module in the Beta; its movement history/discrepancy is not embedded into Job On as a second consultation surface.

## OUT OF SCOPE

- **Armazém** — stock, locations and logistics.
- **Reparação Interna / Reparação Programada** — workshop flows.
- **Tampões as an independent module.**
- **Full Tool lifecycle** — verification/reset/deactivation workflows.
- **Tool `% de uso` / utilisation workflow** — not part of the current Beta; no automatic or manual `% de uso` functionality is required in the Beta.
- **HISTÓRICO GLOBAL** as a top-level aggregating destination. Module-local Histórico remains in scope where its owning module requires it.
- **MCaliper** — control-template integration, links and related workflow are outside the current Beta.
- **Boquilhas PDF/email artifacts** — Boquilhas is consulted directly by Users who have the BQ module assigned; no BQ PDF or email workflow is required in the Beta.
