# Foundation / Runtime Gaps

**Status:** CURRENT IMPLEMENTATION GAPS — VERIFY AGAINST LATEST APP BASELINE BEFORE WORK

**Type:** Platform fixes and integration verification

This file collects small implementation-level blockers/gaps currently recorded in `IMPLEMENTATION_STATUS.md`. These are not new product features.

## Login redirect defect

Unauthenticated access to a protected route currently challenges to:

`/Account/Login`

The inspected baseline reports that this returns 404 because `CookieAuthenticationOptions.LoginPath` is not configured for the actual login route.

This is a defect, not intended behavior.

## Module availability / routes

The inspected baseline records:

```text
ModuleRegistrations.CurrentBuildAvailable = []
```

Operational modules therefore resolve unavailable at runtime.

Availability/route registration must be completed or re-verified against the latest baseline.

## Live Supabase authentication

The authentication architecture exists, but end-to-end live Supabase authentication had not yet been proven with real credentials in the verified runtime session.

This is an integration/test gap, not a product rule.

The newer provider-assisted Setup Mode is documented separately and may change how installation credentials are obtained; it does not remove the need to prove runtime authentication.

## Documents / PDF base-directory configuration

Peso PDF generation/storage/send support exists.

The inspected environment lacked the required base-directory configuration row, so document operations could report the workspace as not configured.

Planning must determine whether this is only environment seeding/configuration or whether supported Setup/Definições flow still needs implementation.

Do not turn one unconfigured test environment into product behavior.

## Email transport

SMTP support exists but may be unconfigured.

An unconfigured transport must produce the defined typed refusal/outcome rather than silently changing product semantics.

## Verification rule

Each item must first be checked against the latest `dmo-app-beta` commit because these are volatile implementation facts.

Do not implement a stale fix solely because this file exists.

## Reviewer checks

Reviewers must distinguish:

- a current code defect;
- a test/environment configuration gap;
- a product rule.

Fixes here must not redefine functional behavior to match an implementation accident.
