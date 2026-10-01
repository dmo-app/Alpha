# Admin — Access Templates

This file defines Access Templates used by normal DMO Users.

Access Templates are not email templates and are not a generic "Templates" domain shared with Controlo document/email configuration.

## 1. Purpose

An Access Template defines the operational access configuration that can be assigned to a User.

Conceptually:

```text
Template
→ selected modules/capabilities/permissions

User
→ associated Template
→ effective operational access
```

A Template may have a human-facing name such as `Reparador`, `Chefe`, `Operador` or another configured name.

The name helps people understand the profile. The name itself is not an authorization rule.

## 2. Admin → Templates

Opening a Template shows:

- the Template identity/name;
- the modules/capabilities/permissions configured for that Template;
- the Users currently assigned to it.

ADMIN may add or remove Users from the Template detail.

The same association is visible and editable from Admin → Users → User.

## 3. One canonical User ↔ Template relation

The User page and Template page are two management views over the same relation.

```text
Admin → Users → User
→ assign/change/remove Template

Admin → Templates → Template
→ add/remove Users
```

These actions must update one canonical User ↔ Template association.

They must not create two lists that can disagree.

The current model gives a User one current Access Template, or none. Template stacking/multiple simultaneous access templates are not implied by this model.

## 4. Legacy User title is replaced by Template label

Older User data may contain a separate title/profile field such as:

- Operador;
- Reparador;
- Chefe.

That field must not continue as a parallel access/profile source.

The User's visible access/profile label comes from the actual associated Template name.

Example:

```text
User: João
Template: Reparador

displayed profile/access label
→ Reparador
```

Effective permissions still come from the Template's configured modules/capabilities/permissions, not from the text `Reparador`.

## 5. Modules inside Templates

There is no standalone Admin → Modules tab.

Modules appear inside Template configuration because the Template defines what its assigned Users may access.

This use of modules is access configuration only.

It must remain distinct from:

```text
Admin → App Definições
→ select a module
→ edit selected settings owned by that module
```

Templates answer:

> What may this User access/do?

App Definições answers:

> How is this module administratively configured?

## 6. Boquilhas module access

For the current Beta, assigning the BQ/Boquilhas module through the User's effective Access Template is sufficient for that User to access Boquilhas consultation, including its module-local Registo/Histórico information.

Boquilhas does not rely on PDF/email distribution as a substitute for module access.

This rule defines visibility/access to the BQ module. It does not create a second profile label or a separate Boquilhas user identity.

## 7. Reviewer invariants

An implementation must not:

- treat an Access Template as an email template;
- grant permissions by matching a Template name as free text;
- keep the old User title as a second editable role/access source;
- create an independent Template→User list that can disagree with User→Template;
- invent a standalone Modules admin destination merely because modules are selectable inside a Template.
