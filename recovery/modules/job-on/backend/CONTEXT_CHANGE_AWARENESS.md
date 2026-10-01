# Job On Context Change Awareness

**Status:** FUNCTIONALLY DEFINED — NOT IMPLEMENTED

**Type:** Cross-cutting implementation feature

## Canonical functional source

The complete functional behavior belongs to:

- `modules/job-on/CONTEXT_CHANGE_AWARENESS.md`

Consumer-specific configuration/behavior belongs to the relevant owning module, for example:

- `modules/boquilhas/DEFINICOES.md`
- `modules/boquilhas/REGISTO.md`

This feature file is an implementation/review checklist. It must not become a second copy of the functional rule.

If wording here and the canonical Job On document ever differ, the module blueprint is the functional source.

## Implementation scope

Implement one awareness capability that preserves two distinct event semantics:

```text
PLANNING AVAILABILITY
-> future Job On becomes queryable as soon as it is successfully created
-> Controlo may explicitly select its jobon_id in Resumo for advance preparation
-> this is not a generic downstream transition event

PRODUCTION_TRANSITION
-> operational consumer changes only when its own transition rule says the production now applies
-> consumer re-reads Job On at that time
-> Boquilhas must not transition merely because the future Job On was created

CONTEXT_CHANGED
-> same jobon_id
-> relevant CM/MF/BQ/TP or other consumed context changes
-> consumer awareness is immediate
-> consumer re-reads Job On immediately
```

The implementation may share transport/storage infrastructure for both event kinds, but it must preserve their different timing behavior.

## Required implementation properties

The implementation must provide:

- permanent Job On change history for real same-`jobon_id` context changes;
- lightweight awareness rather than a duplicated Job On snapshot;
- an event-kind distinction equivalent to `PRODUCTION_TRANSITION` vs `CONTEXT_CHANGED`;
- immediate planning/read availability of a successfully created future Job On;
- planning-only Tool replacement may keep the same component-context ID while that context has not yet been operationally consumed;
- after operational consumption, Tool replacement creates a new component-context ID and preserves the old one;
- Controlo-specific future-`jobon_id` selection for Resumo/preparation;
- no generic downstream production-transition event solely from Job On creation;
- per-consumer pending/acknowledgement state where acknowledgement is used;
- module-owned production-activation configuration for consumers that use planned transitions;
- re-reading of Job On when the consumer must react;
- preservation of historical records on their original CM/MF/BQ/other production contexts;
- delivery of context-change awareness to the application functions that actually use the changed Job On context, without a separate routing table or consumer catalogue.

For a planned transition, any `jobon_id` stored/carried with the awareness must not be treated as the final production truth at activation time. The consumer re-reads Job On and uses the then-applicable plan.

## Acknowledgement boundary

A reviewer must distinguish:

```text
awareness acknowledged
!=
production transition applied
```

A future Job On may be visible in planning reads before the configured activation time without creating an early operational transition.

Where a consumer exposes transition acknowledgement, acknowledgement remains distinct from applying/changing the production context.

For `CONTEXT_CHANGED`, acknowledgement still means only that the immediate change awareness was seen. It does not mean corrected, recalculated, approved or operationally resolved.

## Technical choices intentionally left open

Planning/Architect may choose the technical representation for:

- change-log persistence;
- awareness/event persistence;
- per-consumer acknowledgement state;
- scheduling mechanism;
- delivery/revalidation mechanism;
- storage of each consumer module's activation time;
- UI treatment of pending/seen awareness;
- retention/query UX for the permanent change history.

The blueprint does not require an event bus, message bus, polling implementation or a particular scheduler.

## Functional delivery rule

The implementation follows the functions that consume each Job On context.

```text
function consumes context
→ context changes
→ function receives awareness
→ function re-reads Job On
```

Do not create a separate CM/MF/BQ/TP routing table or consumer catalogue. The existing application functions are the relevant delivery boundary.

## Acceptance / reviewer checks

Reject an implementation that:

- treats `PRODUCTION_TRANSITION` and `CONTEXT_CHANGED` as having the same timing;
- uses midnight, `07:00` or another global application time for every consumer;
- lets one module's activation time control another module;
- lets acknowledgement of a future production transition cancel or complete the scheduled activation;
- trusts an old transition payload instead of re-reading Job On at activation time;
- delays a same-`jobon_id` context change until a later scheduled activation time;
- mutates an already operationally consumed CM/MF/BQ context to point at a replacement Tool;
- creates a new context identity unnecessarily for a planning-only replacement before any operational record has consumed the context;
- rewrites historical Peso, Boquilhas traces or other existing records onto the new context;
- deletes the permanent same-Job-On change fact when awareness is acknowledged;
- interprets acknowledgement as correction, recalculation, approval or resolution;
- duplicates a full Job On snapshot into the awareness system without a separately justified need;
- introduces a separate notification-routing table/catalogue instead of following the application functions that use the changed context.

## Completion rule

This feature may be marked implemented only when:

1. a successfully created future Job On becomes available immediately to the planning reads that need it, including Controlo Resumo selection;
2. Job On creation does not by itself transition Boquilhas or other operational consumers;
3. the consuming modules that need planned transitions own/configure their own activation times;
4. immediate context-change consumers revalidate without waiting for those scheduled times;
5. acknowledgement and scheduled application are separate states/concerns where acknowledgement exists;
6. historical identities and records remain truthful;
7. tests cover planning availability, actual production transition and immediate same-jobon context change.
