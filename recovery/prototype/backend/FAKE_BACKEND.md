# Prototype Fake Backend Rework

**Status:** PROBLEM CONFIRMED — REDESIGN NOT IMPLEMENTED

**Type:** Prototype / development-support change

## Purpose

The DMO prototype is a frontend prototype published through GitHub Pages.

It needs fake data and fake backend-like behavior so interaction flows can be exercised without the real application backend.

That support layer exists only to make the prototype testable.

It is **not** an implementation model for `dmo-app-beta`.

## Current problem

The prototype fake backend was shaped too similarly to an earlier/real backend model.

That similarity created audit drift: reviewers could inspect prototype internals and infer that fake schemas, API shapes, persistence relationships or service structure represented the real DMO backend.

This must be removed as a source of ambiguity.

## Required boundary

The prototype may validate:

- layout;
- interaction;
- navigation;
- form behavior;
- draft preservation;
- user-visible workflow;
- visual states;
- intentionally prototyped frontend behavior.

The fake backend must not establish:

- canonical database schema;
- backend architecture;
- real persistence ownership;
- canonical API contracts;
- canonical ID allocation;
- real table relationships;
- production authentication/authorization behavior;
- implementation completeness.

Conceptually:

```text
GitHub Pages prototype
= frontend + fake support needed to exercise UX

fake support
!= real backend design
!= schema source
!= API source
!= persistence source
```

## Redesign goal

The fake support layer should be structured so that its non-production nature is obvious to both humans and automated reviewers.

It should provide only the minimum behavior/data needed by the prototype surface.

It must not imitate real backend internals merely for realism.

Where the frontend requires an interaction such as search, create, select, save, load or error display, the fake layer may simulate that behavior without claiming that the production backend will use the same storage shape or contract.

## Audit/reviewer rule

When reviewing the prototype:

- review intentionally prototyped frontend/interaction behavior;
- do not use fake backend structure as evidence of production persistence or backend architecture;
- compare any domain behavior against the current canonical blueprint in this repository;
- do not use an older implementation repository as product authority.

If the prototype contains a behavior that appears to create a new identity, ownership rule or backend contract not present in the blueprint, that behavior is prototype behavior only until explicitly defined elsewhere.

## Design principles for the replacement

The redesign should favor:

- clearly named mock/fake/fixture boundaries;
- minimal data structures required by the UI;
- no unnecessary imitation of production tables;
- no fake migration/schema layer presented as if it were production;
- no backend-specific architecture copied into the prototype merely to make it feel realistic;
- easy reset/reload of prototype data;
- deterministic behavior useful for frontend testing.

The exact JavaScript structure is not defined by this file.

## Implementation work still required

Before implementation, inspect the current prototype and identify:

- which fake services/data structures mirror the old backend;
- which frontend surfaces actually depend on those shapes;
- which behavior is genuinely needed for UX testing;
- which fake contracts can be collapsed into simpler prototype-only fixtures/services;
- what documentation or naming is needed so audits cannot mistake the fake layer for production architecture.

## Reviewer checks

A reviewer must reject a redesign that:

- preserves production-looking schemas merely because the old fake backend already had them;
- treats the prototype fake backend as implementation evidence;
- changes real DMO backend architecture to match the prototype;
- removes frontend behavior needed for UX testing solely to make the fake layer simpler;
- invents product rules while redesigning the mock.

The target is not "no backend-like code".

The target is **fake support that is obviously fake and cannot become accidental backend authority**.
