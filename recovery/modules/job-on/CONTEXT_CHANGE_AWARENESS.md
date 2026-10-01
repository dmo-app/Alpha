# Job On — Context Change Awareness

## Purpose

This document defines the functional v1 awareness mechanism used when Job On information relevant to another module changes.

There are two distinct event kinds:

```text
PRODUCTION_TRANSITION
= a consumer module moves from one planned jobon_id to another for a machine

CONTEXT_CHANGED
= the same jobon_id remains, but a relevant production fact/context changes
```

The goal is awareness and revalidation, not automatic operational decision-making.

No additional resolution workflow is implied.

---

## 1. Two different events must remain distinct

### Planned production transition

A machine has a different planned production for the new production date:

```text
machine = B1

previous jobon_id
-> next jobon_id
```

Job On remains the source of the planned production information.

There is **no single global application hour** at which every module must adopt the next production.

Each consuming module owns its own configurable production-activation time.

Saving a future Job On makes that production available as planning information immediately, but **planning availability is not a generic production-transition awareness event**.

Controlo may explicitly select a future saved `jobon_id` from its Resumo/context selection in order to prepare work in advance.

Operational modules that should change only when production actually changes do not adopt or receive the future production as their current operational context at Job On creation.

For a consumer such as Boquilhas:

```text
future Job On exists
-> remains planning information only

consumer's configured production-activation time arrives
-> consumer re-reads Job On
-> resolves the production/context that now applies
-> operational context changes
```

The production transition must not be inferred from midnight and must not depend on a hardcoded example hour such as `07:00`.

### Job On context changed

This event applies when the same production occurrence remains identified by the same `jobon_id` **and the changed context is already operationally relevant to a consumer**.

A future/planning-only Tool selection may be edited before operational use without creating an urgent `CONTEXT_CHANGED` event. Planning readers simply see the updated Job On truth.

Once the context has been operationally consumed, a relevant replacement is represented with a new component-context identity and produces immediate awareness:

```text
jobon_id = unchanged

<old_cm_id> -> <new_cm_id>
<old_mf_id> -> <new_mf_id>
<old_bq_id> -> <new_bq_id>
old TP/Calote value -> new TP/Calote value
```

This event is **immediate** for consumers of the changed context.

A consumer must not wait for its configured production-transition activation time before re-reading the Job On after a same-`jobon_id` context change.

Therefore:

```text
PRODUCTION_TRANSITION
-> consumer reacts at its configured module time

CONTEXT_CHANGED
-> consumer reacts immediately
```

These events may share notification infrastructure, but they must not be modeled as having the same timing behavior.

---

## 2. Production identity and replacement contexts

A substitution inside the same production does not create a replacement `jobon_id`.

The component-context identity rule is temporal:

```text
before operational use
→ planning-only Tool replacement may keep the same cm_id / mf_id / bq_id

after operational use
→ replacement Tool gets a new cm_id / mf_id / bq_id
→ old context remains historical
```

The awareness mechanism must not turn a planning-only edit into a fake operational-history event.

For Tool-backed production contexts, the old context identity is never mutated so that it points to another physical Tool.

Forbidden:

```text
existing bq_id
tool_id X -> tool_id Y
```

Required model **after the BQ context has already been operationally consumed**:

```text
<old_bq_id> -> tool_id X
<new_bq_id> -> tool_id Y
```

The same principle applies to `cm_id` and `mf_id`.

Therefore:

- the `jobon_id` may remain the same;
- after operational use, the replacement Tool receives a new production-context identity;
- the previous context remains referencable;
- downstream records that already use the previous context remain historically truthful.

Notation rule for documentation:

```text
cm_id -> <old_cm_id> / <new_cm_id>
mf_id -> <old_mf_id> / <new_mf_id>
bq_id -> <old_bq_id> / <new_bq_id>

machine -> B1 / B2 / B3 / C1 / C2 / C3
```

Machine labels such as B1/B2/C1 must not be reused as fictional component-context IDs in examples.

---

## 3. The mechanism has three pieces

```text
JOB ON CHANGE LOG
= permanent memory of a real change inside an existing Job On

PING / AWARENESS
= draws attention to a relevant event

MODULE ACKNOWLEDGEMENT
= "seen / taken notice of"
```

These concepts must remain separate.

The awareness signal also preserves which event kind occurred:

```text
event_kind = production_transition
or
event_kind = context_changed
```

### Acknowledgement means only "seen"

A module acknowledgement does **not** mean:

- resolved;
- corrected;
- recalculated;
- approved;
- operationally treated;
- that a planned production transition has already been applied.

It means only that the awareness was seen/taken notice of.

Acknowledgement never deletes or rewrites the Job On change log.

Acknowledgement is scoped to the consuming module. If the same event matters to more than one module, acknowledgement by one module must not clear another module's pending awareness.

If nobody has acknowledged a relevant event for a module, the awareness remains available when that module is opened later. The mechanism must not depend on a transient on-screen event being witnessed at the exact time of delivery.

### Acknowledgement does not cancel a scheduled production transition

For `PRODUCTION_TRANSITION`, seeing or acknowledging the awareness before the configured activation time does not complete the transition.

Conceptually:

```text
future Job On may already be visible as planning information
-> no operational transition has happened yet

configured activation time arrives
-> consumer re-reads Job On
-> consumer resolves and applies the production/context that is applicable then
-> transition awareness/acknowledgement applies only if that consumer workflow exposes it
```

Therefore:

```text
acknowledged
!=
production transition applied
```

An implementation must keep the scheduled activation obligation distinct from the lightweight acknowledgement state.

---

## 4. Permanent Job On change log

When a relevant Job On fact changes inside the same production, Job On records the change permanently.

For a BQ replacement the conceptual fact is:

```text
jobon_id
changed_context = BQ
<old_bq_id> -> <new_bq_id>
actor
timestamp
```

Equivalent context identifiers apply to CM and MF changes.

The log is the durable operational memory. It remains available after acknowledgement.

The user-facing presentation does not need to expose UUIDs. It may show the human information required to understand the change.

A planned production transition does not require a duplicated Job On snapshot in the awareness system. At its configured activation time, the consumer re-reads Job On and resolves the applicable production context.

---

## 5. Lightweight awareness signal

The awareness signal is deliberately small.

For a same-Job-On context change:

```text
event_kind = context_changed
jobon_id
context_changed = BQ
```

or the corresponding CM, MF, TP/Calote or other relevant production context.

For a planned production transition, the lightweight transition fact belongs to the point where that consumer actually transitions to the next production context.

A saved future Job On may already be discoverable through planning/selection reads before this event exists.

```text
event_kind = production_transition
machine
jobon_id
```

The signal does not carry a complete Job On snapshot and does not become a second source of production truth.

For `PRODUCTION_TRANSITION`, any `jobon_id` carried by the awareness identifies the planned production known when that awareness was produced. It is not the final source of truth at activation time.

When the configured activation time arrives, the consumer must re-read Job On and use the production/context that is applicable at that moment.

Therefore a later planning edit may change what the consumer ultimately adopts without requiring the awareness payload itself to become a mutable production snapshot.

The normal module/backend relationship already knows how to read the Job On context it needs.

The signal tells the consumer **when to revalidate**, according to the event kind:

```text
production_transition
-> at the consumer module's configured activation time

context_changed
-> immediately
```

---

## 6. Pings follow the functions that use the context

A Job On context-change ping follows the actual application functions that consume that context.

```text
function uses a Job On context
→ that context changes
→ that function receives the ping
→ the function re-reads Job On
```

There is no separate product concept, table or manually maintained routing map for CM, MF, BQ, TP/Calote or other Job On facts.

The functions themselves define where the awareness is relevant. The ping mechanism follows those existing functional relationships; it does not create another domain relationship alongside them.

Each function/module that uses a planned production transition still follows its own transition timing where such timing exists. One function's transition rule must not silently control another.

---

## 7. Role of the application

This is an awareness feature, not an automation engine.

The transverse mechanism does not automatically:

- decide the operational impact of the change;
- create work;
- decide that a new Peso is required;
- recalculate Peso;
- resolve Boquilhas;
- create resolution workflows;
- mark operational work as treated;
- rewrite historical records.

For an immediate context change the minimal flow is:

```text
change
-> immediate awareness
-> consumer re-reads Job On
-> human verifies where required
-> acknowledgement when seen
```

For a planned production transition:

```text
future Job On
-> may already be visible in planning/Controlo preparation reads

consumer activation time arrives
-> consumer re-reads Job On
-> consumer uses the production/context applicable at that moment
-> any transition awareness is scoped to the consumer's actual transition behavior
```

A module may still have its own independent domain behavior. For example, Boquilhas may use its own canonical identity rules to associate a pre-production trace to a later BQ production context. That behavior is not performed by the generic Job On awareness mechanism.

---

## 8. Historical truth is preserved

A context change never rewrites a downstream record that already belongs to the previous context.

For example:

```text
existing record
-> <old_cm_id>

Job On later changes CM
-> <new_cm_id>
```

The existing record keeps `<old_cm_id>`.

The awareness mechanism tells relevant consumers that the Job On context changed. It does not move the existing record to the new context.

Likewise, moving a module to the next planned production does not rewrite records created under the previous production.

The same principle applies to BQ repair traces, movement history and other context-bound operational records.

---

## 9. Machine changes

A machine reassignment inside planning is not automatically equivalent to an urgent CM/MF/BQ context-change alert.

This is separate from the normal **planned production transition for a machine**, which uses the consumer module's configured activation time.

If a real workflow requires a special immediate awareness rule for machine reassignment itself, that behavior must be defined explicitly rather than inferred from the production-transition mechanism.

---

## 10. Functional v1 boundary

The mechanism is conceptually closed at this level:

```text
1. Job On creation makes the future production immediately discoverable as planning data
2. Controlo may select that future jobon_id in Resumo for advance preparation
3. planning availability is not a generic production_transition awareness event
4. each operational consumer adopts the next production only when its own transition rule says the change actually applies
5. at transition time the consumer re-reads Job On and uses the then-applicable production/context
6. a context_changed event inside the same jobon_id is handled immediately
7. no global midnight/hardcoded-hour rule controls all modules
8. real same-jobon context changes are permanently logged
9. awareness remains lightweight and does not duplicate the Job On snapshot
10. context-change pings follow the application functions that already use the changed Job On context
11. no separate CM/MF/BQ/TP consumer-routing catalogue is required
12. acknowledgement means only "seen" where that consumer exposes acknowledgement
13. historical operational records are never rewritten by awareness
```

## Core rules

> **A future Job On becoming available for planning is not the same thing as an operational production transition. Controlo may select the future `jobon_id` for preparation as soon as it is saved; a module such as Boquilhas changes operational context only when its own production-transition rule says the production has actually changed. Same-`jobon_id` context changes are surfaced immediately.**

> **Acknowledging a planned production-transition awareness means only that it was seen. The transition remains operationally pending until the consumer reaches its configured activation time, re-reads Job On and applies the production/context that is applicable then.**

> **Awareness remains lightweight. The consuming module re-reads Job On instead of receiving a duplicated production snapshot.**

> **A context-change ping follows the application functions that use the changed Job On context. No separate routing model or consumer catalogue is required.**
