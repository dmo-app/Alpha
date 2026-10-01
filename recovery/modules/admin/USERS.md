# Admin — Users

This file defines the normal DMO User administration surface.

A normal USER is distinct from the special DMO ADMIN association described in `OVERVIEW.md` and `SETUP.md`.

## 1. User list

The Admin → Users list must expose the operational identity information needed to identify and manage a User.

The current User facts are:

- name;
- operator number / BA Glass identification;
- current Access Template, if assigned;
- current account availability such as active or stand-by.

The operator number / BA Glass identification is at most four digits.

It is an identifier, not a quantity. Its representation must preserve the entered identification faithfully, including any meaningful leading zero.

## 2. User detail

Opening a User shows the same canonical User record and its current Template association.

Conceptually:

```text
User
├── Name
├── Operator / BA Glass ID
├── Template
└── Account availability
```

The operator / BA Glass ID is the login identifier known by the normal User.

This is intentionally different from ADMIN, whose operator-facing login identifier is email.

DMO keeps its own User identity and the configured authentication provider keeps the technical authentication identity. Provider-specific attributes required under the hood must not become extra operator-facing login requirements for normal Users.

The User detail must not maintain a second role/profile label independent from the Template.

If an older implementation contains a `title` field such as:

- Operador;
- Reparador;
- Chefe;

that field is no longer the canonical profile/access label.

The visible profile/access label comes from the name of the associated Access Template.

## 3. User ↔ Template association

The current access model uses one current Access Template per User, or no Template.

```text
User
→ Template A

or

User
→ no Template
```

A User without an Access Template may remain registered but has no operational module access derived from a Template.

From the User detail, ADMIN may:

- assign the User to a Template;
- change the User to another Template;
- remove the User from its current Template.

This is the same canonical association managed from the Template detail page.

Changing it from Users or Templates must not create two independent relations.

## 4. User creation and temporary password

ADMIN may create a normal User from DMO using:

- name;
- operator / BA Glass ID;
- Access Template where applicable;
- a temporary password.

DMO uses the configured authentication provider for the normal User's authentication identity and credential.

This differs from ADMIN bootstrap: the Auth identity used as DMO ADMIN must already exist outside DMO according to `SETUP.md`.

The temporary password is bootstrap/reset access only. It is not stored as DMO-owned User data and is not a historical password.

```text
ADMIN creates User
→ temporary password
→ User authenticates
→ password change required
→ User defines new password
→ normal application access
```

The definitive password remains owned by the authentication provider and cannot be read back through DMO.

## 5. Password reset

Reset creates a new temporary-access condition through the authentication provider:

```text
ADMIN resets password
→ new temporary password
→ next successful login requires password change
```

DMO never shows the previous password and never stores the definitive password.

Reset does not change User identity or Template.

## 6. Personal User menu

The application shell exposes a personal User menu with:

- current User identification/name;
- visible Template/profile label;
- change password;
- logout.

This is shell behavior, not a separate operational module.

## 7. Stand-by

A User may be placed in **stand-by**.

Stand-by means the User record and historical attribution remain preserved while normal operational access is disabled.

It is not deletion.

The User may later be returned to active use without recreating historical operational records.

Stand-by must not rewrite operations, approvals, movements or other historical facts previously attributed to that User.

## 8. Delete

If a person has definitively left and the User no longer needs to remain available for operational use, Admin may expose a delete action.

Deletion must never destroy or orphan historical attribution for actions already performed by that User.

The exact persistence strategy may preserve a minimal historical identity/tombstone or another technically truthful representation, but the product invariant is:

```text
User removed from active administration
!=
historical actions lose their actor
```

Deletion is distinct from temporary stand-by and must not happen automatically merely because a User is placed in stand-by.

## 9. Explicitly forbidden interpretations

An implementation must not:

- use a legacy User `title` as a second access/profile authority beside the Template;
- derive permissions by comparing labels such as `Operador`, `Reparador` or `Chefe`;
- allow a stand-by User to retain normal operational access;
- delete historical records when a User is put in stand-by or removed;
- maintain separate User→Template and Template→User assignment systems;
- treat the operator/BA Glass identification as an arithmetic quantity;
- require login email as an operator-facing product credential for a normal User; this prohibition does not apply to ADMIN, which logs in by email;
- allow temporary-password authentication to enter the operational application before password change;
- persist temporary or definitive passwords as DMO-owned User data;
- expose an existing password during reset.
