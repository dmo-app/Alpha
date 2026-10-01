# Job On — Consultar

Job On View is the consultation surface for saved production occurrences.

It allows the user to inspect the production facts and the Tool contexts attached to that production without editing them.

## Planning-calendar discovery

The normal Job On consultation entry point is the planning calendar.

The calendar reads planned Job Ons by date/machine and returns only the information needed to identify the planned production(s) for the selected day.

Clicking a day does not make a Job On active automatically.

```text
calendar day selected
→ read planned Job On candidates for that day
→ show candidate production(s)
→ user explicitly selects one
→ use persisted jobon_id
→ open consultation
```

Even when only one planned Job On is returned, the calendar remains a discovery surface rather than a new source of production identity.

The persisted `jobon_id` is the context anchor used to open the saved Job On.

## Expected context

A saved Job On can expose, as applicable:

- reference;
- production number;
- machine;
- production date;
- selected CM / MF / BQ contexts;
- production-specific configuration;
- links or navigation into downstream module information for that same production.

The page must resolve the saved production by `jobon_id`.

Visible fields such as reference or production number help humans find the record; they do not replace the stable production identity.

## Boundary

Consultation must not mutate the Job On.

Job On View does not gain edit behavior through a hidden UI state or historical role mapping.
