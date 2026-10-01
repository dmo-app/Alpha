# Controlo — Definições

This file documents configuration owned by Controlo operational workflows.

The settings remain Controlo-owned, but their administrative editing surface is centralized under:

```text
Admin
→ App Definições
→ Controlo
```

This file does **not** imply a visible Definições tab inside the daily Controlo Create navigation.

Centralizing the editing surface keeps infrequently changed or sensitive configuration out of the operational hot path without transferring ownership of those settings to Admin.

## Current settings

The Controlo settings include at least:

- base directory for generated PDFs;
- glass density by process where applicable;
- email templates;
- production recipient/person configuration;
- Line B recipient configuration;
- Line C recipient configuration;
- fast sending of Peso/PDF artifacts.

## PDF recipients

Controlo owns the operational configuration used to resolve PDF recipients.

These configured recipients are production/email recipients. They must not be treated as application-authentication users merely because a person may also have an application account.

Recipients may be associated with the relevant machine/line group.

Current routing:

- B1/B2/B3 resolve to the B recipient configuration;
- C1/C2/C3 resolve to the C recipient configuration.

When a PDF is sent, the workflow resolves the applicable group and its configured recipients.

The person sending the PDF does not need to select an arbitrary recipient list on every send.

Recipient email addresses must not be hardcoded into the sending workflow.

The values remain Controlo configuration even though ADMIN edits them through App Definições.

Settings provide configuration consumed by operational workflows; they do not become a second source of truth for records created by those workflows.
