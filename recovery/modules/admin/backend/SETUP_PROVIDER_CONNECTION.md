# Setup Mode Provider Connection

**Status:** FUNCTIONAL DIRECTION CONFIRMED — NOT IMPLEMENTED — TECHNICAL VALIDATION STILL REQUIRED

**Type:** Installation / infrastructure setup feature

## Purpose

A new DMO installation must be configurable through the application instead of requiring the installer to use terminal commands, manually bootstrap the application, or copy technical configuration that the application can obtain safely itself.

The desired experience is:

```text
DMO starts without installation configuration
-> Blank / Setup Mode
-> Connect provider
-> authenticate through provider
-> choose the intended project/infrastructure
-> DMO obtains the available required configuration
-> validate installation
-> associate an existing Auth user as DMO ADMIN
-> finish setup
-> normal DMO runtime
```

The first provider to explore and implement is Supabase.

The design must not unnecessarily prevent a future provider such as Azure from having its own setup path.

## Supabase setup direction

The preferred Setup Mode experience is an in-application action such as:

```text
Connect Supabase
```

The setup flow should explore using Supabase account/project authentication and management capabilities so DMO can retrieve as much required project configuration as the provider safely exposes.

The intended human flow is:

1. Open DMO in Blank / Setup Mode.
2. Choose **Connect Supabase**.
3. Authenticate/authorize with Supabase through the appropriate provider flow.
4. Show the Supabase projects available to that account/authorization.
5. Explicitly choose the project that will host this DMO installation.
6. Let DMO retrieve the configuration values it can obtain programmatically.
7. Validate that the selected infrastructure is usable by DMO.
8. Associate an **existing** Supabase Auth user as the DMO ADMIN.
9. Persist only the configuration required for normal runtime.
10. Exit Setup Mode and start normal application use.

## ADMIN boundary

This feature does not change the existing ADMIN rule.

Supabase Auth owns creation of the authentication identity.

DMO may associate an already-existing Auth user as ADMIN, but Setup Mode must not create the Supabase Auth identity that becomes DMO ADMIN.

The ADMIN login identifier is the email of that existing Auth identity. This email-based ADMIN login is intentional and distinct from the operator / BA Glass ID login used by normal Users.

ADMIN must not be assigned merely because:

- the user is first;
- the DMO database is empty;
- no ADMIN currently resolves;
- setup happens to be running.

Setup configures an installation and associates an existing identity. It does not manufacture the external authentication identity.

## Setup versus runtime

Provider-management access used during installation is a **setup concern**.

Normal DMO runtime must not require the provider-management login/session merely because it was used to configure the installation.

Conceptually:

```text
provider-specific setup
-> creates/records valid DMO installation configuration
-> normal runtime uses that configuration

normal runtime
!= continuously re-running provider management discovery
```

This boundary is important so that provider-specific installation mechanics do not spread through normal DMO workflows.

## Future provider boundary

The first implementation may be Supabase-specific.

Do not design a large universal cloud abstraction before another provider is actually required.

Preserve only the useful boundary:

```text
provider-specific discovery/configuration
-> Setup Mode boundary

DMO operational behavior
-> normal runtime boundary
```

A future Azure setup path may use a different authentication and discovery mechanism while producing the configuration its DMO runtime adapter needs.

## Manual fallback

The target is zero terminal and zero manual key copying whenever the provider can safely supply the required information programmatically.

If technical validation proves that a required value cannot be obtained through the provider APIs, Setup Mode may request that specific value from the user.

A manual fallback must be a consequence of a real provider limitation, not the default installation experience.

## What this file does not decide yet

Technical validation is still required for:

- which Supabase management/OAuth flow is appropriate for the installed DMO application;
- which project configuration values can be retrieved automatically;
- which runtime credentials DMO actually requires;
- whether direct database credentials are required by the final runtime architecture;
- secure local/server-side persistence of the resulting runtime configuration;
- whether setup-management authorization should be discarded/revoked immediately after configuration;
- recovery/reassociation UX when the infrastructure already exists but ADMIN association must be repaired.

These questions must be answered during architecture/implementation work and must not be silently inferred here.

## Reviewer checks

A reviewer must verify that the eventual implementation:

- offers Setup Mode through the application rather than making terminal bootstrap the primary supported path;
- does not create the external Supabase Auth identity used as ADMIN;
- does not make first-user/empty-database state equivalent to ADMIN;
- does not require the Supabase management session during normal DMO runtime without a separately justified requirement;
- retrieves provider configuration automatically where safely possible;
- keeps any unavoidable manual credential input limited to values that genuinely cannot be obtained programmatically;
- does not prematurely force all future providers into a Supabase-shaped runtime contract.
