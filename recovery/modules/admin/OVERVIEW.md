# Admin — Overview

This file defines the DMO Admin identity boundary and the administration/configuration surface.

## 1. ADMIN is a DMO role over an external Auth identity

Supabase Auth owns authentication identity and credentials.

DMO owns the application-side access and administration rules for an authenticated identity.

Therefore:

```text
Supabase Auth user
→ existing external authentication identity

DMO
→ associates one existing Auth identity as ADMIN
```

ADMIN is not a special Supabase user type. In Supabase Auth it remains a normal Auth user.

The administrative distinction exists in DMO.

## 2. DMO never creates the Auth identity used as ADMIN

The identity that becomes ADMIN must already have been created outside DMO through Supabase administration.

No DMO page, endpoint, seed, bootstrap, migration, recovery flow or convenience action may create the Supabase Auth user that will become ADMIN.

DMO may only:

1. connect to the configured Supabase/Auth infrastructure;
2. validate that the selected Auth user already exists;
3. associate that existing Auth identity as the DMO ADMIN.

This separation applies specifically to the authentication identity used as DMO ADMIN.

## 3. ADMIN is not "the first user"

DMO must never assign ADMIN because:

- a user is the first person to log in;
- the user table is empty;
- no current ADMIN row exists;
- the application has just started;
- a migration or seed is running.

Technical database state must not silently choose a privileged human identity.

## 4. Admin surface

ADMIN is not a normal USER with every operational module enabled.

The Admin surface is a separate administration/configuration area.

Its functional structure is:

```text
Admin
├── Users
├── Templates
└── App Definições
```

There is **no standalone Modules tab** in Admin.

Modules appear in two different contexts for two different reasons:

```text
Templates
→ choose modules/capabilities/permissions
→ defines operational access for Users assigned to that Template

App Definições
→ choose a module
→ edit selected administrative settings owned by that module
```

These must not be merged conceptually.

Detailed rules:

- `USERS.md`
- `TEMPLATES.md`
- `APP_DEFINICOES.md`

## 5. User access model

Operational access for normal USERS follows:

```text
USER
→ one current Access Template, or none
→ modules/capabilities/permissions defined by that Template
```

The Template name is also the human-facing profile/access label shown for the User.

Examples may be names such as:

- Reparador;
- Chefe;
- Operador;
- another configured Template name.

Those names are labels for the actual Template association.

A separate User `title` such as `Operador`, `Reparador` or `Chefe` must not remain as a second editable source of profile/access truth.

Permissions are not granted by comparing text labels. They come from the User ↔ Template relation and the permissions defined by the associated Template.

## 6. Module settings ownership versus configuration UI

A module owns the meaning of its own settings.

Admin provides the privileged UI used to edit selected settings through:

```text
Admin
→ App Definições
→ select module
→ edit that module's administrative settings
```

This does not transfer ownership of those settings to Admin.

It also does not justify a separate operational `Definições` destination inside every module.

The centralized App Definições surface is intended especially for settings that are:

- changed infrequently;
- specialized/niche;
- operationally sensitive;
- capable of causing incorrect behavior if changed casually.

The owning module remains the canonical source for what the setting means and how it affects that module.

## 7. Authentication boundary

ADMIN and normal USERS deliberately use different operator-facing login identifiers.

```text
ADMIN
→ login identifier = email
→ Auth identity already exists in the configured provider
→ DMO associates that existing identity with ADMIN

normal USER
→ login identifier = operator / BA Glass ID
→ DMO may provision the provider-side technical identity for that User
```

The ADMIN authentication identity follows the special setup rule: it must already exist outside DMO and DMO only associates it with the ADMIN function.

The ADMIN email is therefore an intentional operator-facing login field for ADMIN. The rule that normal Users do not require an operator-facing login email does **not** apply to ADMIN.

Normal USERS follow a different provisioning flow.

ADMIN may create a normal User through DMO, and DMO may use the configured authentication provider to establish that User's technical authentication identity and temporary credential.

For normal Users:

```text
operator / BA Glass ID
→ operator-facing login identifier

DMO User identity
→ application identity and historical attribution

provider Auth identity
→ technical authentication identity
```

The provider remains the owner of password credentials.

DMO may initiate creation/reset using a temporary password, but it must not persist that password as User data. Temporary-password authentication requires the User to define a new password before normal application access.

This normal-User provisioning rule does not weaken or replace the stricter ADMIN bootstrap/reassociation boundary in `SETUP.md`.

See:

- `SETUP.md` for installation and ADMIN reassociation;
- `USERS.md` for normal User administration;
- `TEMPLATES.md` for access-template behavior;
- `APP_DEFINICOES.md` for centralized module configuration.
