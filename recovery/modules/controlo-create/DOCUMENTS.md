# Controlo Create — Documents / PDFs

Generated documents are derived operational artifacts.

They must remain conceptually separate from the persisted record and from the filesystem location used to store the generated file.

```text
record ≠ PDF ≠ filesystem path
```

## Source of truth

The persisted operational record is the source of truth.

Examples include:

- Peso;
- Folha;
- Pegamentos;

A PDF is generated from persisted application facts for presentation, printing or distribution.

If a generated PDF and the persisted record disagree, the persisted record prevails.

A PDF must not become a replacement persistence model for the underlying operational record.

## Filesystem path

The configured path is a storage location, not a domain identity.

Filename and path must never be used as join keys or as substitutes for record identities.

The base directory is configured in Controlo Create → Definições.

The production document tree is:

```text
<base_directory>/
└── <reference>/
    └── <production_number>/
        ├── Peso_<reference>_<machine>.pdf
        ├── Resume_<reference>_<machine>.pdf
        └── Pegamentos_<reference>_<machine>.pdf
```

Reference precedes production number in the directory hierarchy.

Required directories are created or reused automatically. Re-entering the same production path must not create a duplicate parallel tree.

Job On and Controlo use the same production/document relationship. Job On does not own a second document tree.

## Generation

PDF generation belongs to the relevant Controlo Create workflow.

Generated content is composed from the persisted facts and read data required for that document.

The document layer must not invent domain facts or become a second source of truth for calculations or decisions.

Generated PDFs must not be silently overwritten.

If an existing generated artifact would be replaced, the workflow must make that replacement explicit according to the owning document flow.

### Peso PDF

The Peso PDF has a specific operational rule:

- the Peso must already have a completed decision/approval before its production PDF is generated for use in production;
- its detailed field/data-provenance contract is defined in [PESO_PDF.md](./PESO_PDF.md);
- after reopen/correction/resubmission and a new approval, the same peso_id may explicitly regenerate/replace its production PDF from the newly approved state.

This requirement is specific to Peso because the Peso document is sent to production as an operational reference.

It must not be generalized automatically to Folha, Resumo, Pegamentos or other documents.

Each other document follows the lifecycle/preconditions of its own workflow.

## Date shown on production documents

Where the production date is shown, it comes from the active Job On production context.

The document does not create an independent document-owned production date.

## Sending

Sending a PDF is an explicit/manual action from the relevant Controlo Create workflow.

Recipient routing is configuration-driven.

The sender does not construct an arbitrary recipient list on each send.

The recipient configuration and machine-group routing are owned by Controlo Create → Definições.

Current routing includes:

- B1/B2/B3 → Group B recipient configuration;
- C1/C2/C3 → Group C recipient configuration.

Templates and recipient email addresses must not be hardcoded in the document-sending workflow.

Configured production/email recipients are not automatically application-authentication users.

See:

- `DEFINICOES.md`

## Boquilhas boundary

Boquilhas does not produce a PDF/email artifact in the current Beta. Users with the BQ module assigned consult its Registo/Histórico directly.

This Controlo document contract must not be used to invent a Boquilhas repair PDF merely because Boquilhas movement records are persisted.

## Availability

Document absence is not automatically an application error.

A document may legitimately not exist yet because it has not been generated.

Where document availability is represented in the UI, distinguish operationally different conditions rather than collapsing them into one generic failure.

The shared availability vocabulary may include:

- `Available`;
- `NotGenerated`;
- `NotFound`;
- `WorkspaceUnavailable`;
- `Refused`.

These states describe document availability. They do not redefine the state of the underlying persisted operational record.
