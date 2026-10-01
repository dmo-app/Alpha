# Admin Users / Access Templates / App Settings Alignment

**Status:** FUNCTIONAL MODEL CONFIRMED — IMPLEMENTATION ALIGNMENT REQUIRED

**Type:** Admin / identity-access / configuration implementation

## Purpose

Align the Admin implementation with the current canonical model for:

- Users;
- Access Templates;
- User ↔ Template assignment;
- replacement of legacy User title/profile labels;
- centralized App Definições for module-owned settings.

Canonical product behavior is documented in:

- `modules/admin/OVERVIEW.md`;
- `modules/admin/USERS.md`;
- `modules/admin/TEMPLATES.md`;
- `modules/admin/APP_DEFINICOES.md`.

## Admin surface

The intended Admin structure is:

```text
Admin
├── Users
├── Templates
└── App Definições
```

There is no standalone Modules tab.

Modules appear:

- inside Templates to configure access;
- inside App Definições as the selected owner of module settings.

These two uses must remain separate.

## User fields and actions

The User administration surface must align with the confirmed User facts:

- name;
- operator number / BA Glass identification of at most four digits;
- current Access Template, if any;
- account availability such as active / stand-by.

For normal Users, operator / BA Glass ID is the operator-facing login identifier. Login email is not a normal-User product field. ADMIN is a separate authentication case and logs in by email.

The User detail also exposes:

- assign/change/remove Template;
- password reset through the authentication provider;
- place User in stand-by;
- later remove a User who has definitively left, without losing historical actor attribution.

## Replace legacy User title with Template label

Older implementation may contain a User `title` field with values such as:

- Operador;
- Reparador;
- Chefe.

That field must no longer act as the canonical profile/access source.

The intended model is:

```text
User
-> associated Access Template
-> Template name is the human-facing profile/access label
```

The implementation must inspect any existing `title` persistence/UI use and remove or migrate its access/profile responsibility safely.

Do not keep `title` as a second editable role source beside the Template.

Do not grant permissions by comparing title or Template-name text.

## Bidirectional management of one relation

The same User ↔ Template association is managed from both sides:

```text
Users → User
→ view/change/remove current Template

Templates → Template
→ view/add/remove assigned Users
```

These are two UI views over one canonical relation.

The current model uses one current Access Template per User, or none.

Implementation must not create independent assignment stores that can disagree.

## App Definições

Implement the centralized administrative settings surface:

```text
Admin
→ App Definições
→ choose module
→ edit selected settings owned by that module
```

This feature exists to keep rarely changed, specialized or sensitive configuration out of each module's daily operational navigation.

Settings remain owned semantically by their module.

Admin provides the privileged editing surface only.

The first confirmed cross-feature example is the Boquilhas production-activation time defined by the Job On production-transition awareness model.

## Historical and auth boundaries

Implementation must preserve these boundaries:

- stand-by disables normal operational use without deleting historical attribution;
- deleting/removing an old User must not orphan historical actions;
- password reset delegates to the authentication provider;
- DMO must not store user passwords;
- ADMIN identity creation/reassociation continues to follow the separate ADMIN rules in `modules/admin/SETUP.md`.

## Implementation work

Planning/Architect must inspect the selected application baseline and identify:

- existing User fields and any legacy `title` usage;
- current User ↔ Template storage/cardinality;
- existing Template module/permission configuration;
- current User and Template management screens;
- current auth-provider password-reset path;
- whether stand-by/deactivation already exists and how it behaves;
- whether App Definições exists in any form;
- any current per-module settings destinations that must be moved/represented through the centralized Admin surface without duplicating stored settings.

Then implement only the delta required to match the canonical Admin blueprint.

## Reviewer checks

Reject an implementation that:

- keeps the legacy User `title` as a second editable profile/access authority;
- derives permissions from visible labels such as `Operador`, `Reparador`, `Chefe` or a Template name;
- allows User→Template and Template→User views to disagree;
- introduces multiple simultaneous Access Templates per User without a new explicit product decision;
- creates a standalone Admin Modules tab merely because modules are selectable inside Templates or App Definições;
- duplicates module settings into an Admin-owned copy;
- adds a visible `Definições` destination to every operational module just to edit rare settings;
- lets stand-by or deletion erase historical actor attribution;
- stores passwords as DMO-owned application data;
- applies the normal-User "no login email" rule to ADMIN; ADMIN intentionally logs in using email.
