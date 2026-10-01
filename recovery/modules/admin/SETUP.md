# Admin — Setup / Blank State

This file defines initial DMO setup and ADMIN reassociation.

## 1. Blank State

A fresh DMO installation may start in **Blank State**.

Blank State means the application has not yet completed its infrastructure/admin configuration.

It is a setup state, not an authenticated ADMIN session.

In Blank State:

- no DMO username/password is required to reach the setup surface;
- the application routes to the setup/admin-configuration surface;
- the purpose is to connect and validate the installation infrastructure and associate an existing Supabase Auth user as ADMIN.

The implementation must not pretend that an ADMIN identity already exists during Blank State.

## 2. Graphical infrastructure setup

Setup is intentionally available through the application UI so initial installation does not depend on terminal-only configuration.

The setup surface may collect the normal connection information/keys required to connect to the intended Supabase/database infrastructure and validate that connection.

The exact secret/key types are deployment details. The durable rule is:

> Setup connects DMO to an explicitly supplied existing infrastructure; it does not silently choose or create one.

## 3. ADMIN association during setup

After the infrastructure connection is valid, Setup associates the DMO ADMIN.

The selected identity must already exist in Supabase Auth.

The ADMIN operator-facing login identifier is that Auth user's **email**. Setup therefore associates the DMO ADMIN with an existing provider Auth identity identified for login by email.

The flow is:

```text
Supabase administration
→ create Auth user outside DMO

DMO Setup
→ validate that Auth user already exists
→ associate that identity as ADMIN
→ complete setup
```

DMO must not create the Auth user.

If the Auth user does not exist, Setup must not create it as a convenience. The operator must create it in Supabase first and then return to Setup.

## 4. Security boundary

The security boundary is that creation of the identity used as DMO ADMIN remains outside the application.

A person who only has access to DMO must not gain the ability to create the external Auth identity that can be associated as ADMIN.

Setup/reassociation must therefore continue to depend on valid access to the configured infrastructure rather than turning into an in-app "create administrator" mechanism.

## 5. Deleting the current ADMIN Auth user must not brick the installation

The DMO ADMIN association points to an existing Supabase Auth user.

If that Auth user is later deleted in Supabase, the DMO database and operational data remain valid.

The installation must not become permanently inaccessible or require a database reset.

The same Setup flow may be used to repair the ADMIN association against the same existing database/infrastructure.

The operator:

```text
1. creates a replacement Auth user in Supabase administration;
2. opens Setup for the existing DMO installation;
3. supplies/validates the existing infrastructure connection as required;
4. selects/validates the replacement existing Auth user;
5. replaces the invalid ADMIN association.
```

No production data, USER data, module data or history is recreated merely because the ADMIN Auth identity changed.

## 6. One setup concept, not a chain of bootstrap states

DMO does not need separate business concepts such as:

- "first-user bootstrap";
- "emergency administrator";
- "automatic admin recovery user";
- "temporary admin account";
- "recovery-created Supabase user".

Initial installation and later ADMIN reassociation use the same setup concept.

The difference is only whether the installation is being configured for the first time or an existing ADMIN association is being repaired.

## 7. Explicitly forbidden behavior

An implementation must not:

- make the first login automatically become ADMIN;
- create a Supabase Auth user from DMO and then promote it to ADMIN;
- expose an in-app "Create ADMIN user" operation that creates Auth credentials;
- create an ADMIN from a seed/migration merely because no ADMIN currently resolves;
- reset or recreate the database when the ADMIN Auth user is deleted;
- delete or rewrite operational data when ADMIN is reassociated;
- treat Blank State as if it were a real authenticated ADMIN identity;
- require a terminal-only bootstrap when the same connection can be configured through the supported setup UI.

## 8. Core invariant

```text
Supabase Auth
= owns existence and credentials of the ADMIN authentication identity
= ADMIN login identifier is email

DMO
= owns the association of that already-existing identity with the ADMIN function
```

These two ownership boundaries must remain separate.
