# Job On — Overview

Job On represents one concrete production occurrence.

Its canonical identity is `jobon_id`.

Job On owns the production context: reference, production number, machine, production date, selected Tools and the production-specific configuration that belongs to that run.

It does not own Tool master data, quality-control measurements, approval decisions or Boquilhas repair history.

## Capabilities

Job On has two access capabilities:

- **Job On View** — consultation only;
- **Job On Create** — creation/editing capability and the viewing behavior required to perform that work.

These are independently assignable module capabilities, not historical role titles.

**Job On Create** already includes the consultation behavior needed to perform its work, so a user assigned Job On Create does not also need Job On View. **Job On View** exists for users who need consultation without creation/editing authority.

The current Beta may intentionally require only Job On Create for its Job On operator. This does not conflict with Users, Access Templates or the broader access model; it is a scope choice about which capability is needed for that operational use.

Inside Job On Create, an existing Job On normally opens in a safe View state and requires an explicit switch to Edit before editable values are changed. This UI state is not a separate permission.

## Planning landing page

The Job On landing page is the production-planning calendar.

It is a read/discovery surface over saved Job Ons, organized by their planned production date and machine.

```text
saved Job On
→ planned production date
→ machine
→ calendar marker
```

Clicking a day shows the Job On production(s) planned to enter on that day.

If more than one Job On is planned for the selected day, all relevant candidates are shown and the user explicitly selects which one to open.

```text
day
→ planned Job On(s)
→ explicit selection
→ jobon_id
→ open saved Job On context
```

The calendar is not a second production source and does not create a calendar identity.

```text
calendar marker
!= duplicated production data

calendar marker
→ projection/read model from Job On
→ points back to jobon_id
```

A Job On becomes visible in this planning surface as soon as it is successfully created.

This creation-time visibility is planning/discovery information. It does **not** mean every downstream module receives or adopts the future production immediately.

In the current model, Controlo may use the saved future `jobon_id` immediately as an explicit preparation/Resumo selection. Operational modules such as Boquilhas keep their current production context until their own real transition rule says the production has changed.

## Production context

A Job On selects canonical Tools from Ferramentas.

For the production, the application establishes:

```text
jobon_id
├── cm_id → tool_id
├── mf_id → tool_id
└── bq_id → tool_id
```

The component contexts belong to that production occurrence and preserve the required production-time Tool context.

The same canonical Tool may be used in several Job Ons. Each production receives its own component context identity.

## Downstream use

Controlo and other operational modules consume the production context established by Job On.

They must not silently reinterpret or replace the selected Tool identities.

Tool replacement follows the operational-use boundary.

Before the component context has been operationally consumed, a planning-only Tool replacement may keep the same `cm_id`, `mf_id` or `bq_id`.

After operational use, replacing the Tool creates a new component-context identity, preserves the previous context for historical records, permanently logs the change and exposes lightweight awareness to the consumers of that operational context.

The awareness mechanism does not decide what downstream work should be done. A module acknowledgement means only that the change was seen.

Detailed flows:

- `CRIAR.md`
- `CONSULTAR.md`
- `EDITAR.md`
- `DUPLICAR.md`
- `SELECIONAR_FERRAMENTAS.md`
- `CONTEXT_CHANGE_AWARENESS.md`
