# Job On — Editar

Editing belongs to Job On Create.

An existing Job On opens in a safe View state and requires an explicit user action to enter Edit state before editable values are changed.

## Identity

Editing mutates the same `jobon_id`.

It does not create a replacement production merely because one field changed.

## Context safety

Changes must preserve the distinction between:

- canonical Tool identity in Ferramentas;
- Tool selection/context for this Job On;
- downstream records already created from the production context.

Editing must not silently rewrite historical downstream facts.

Tool replacement follows the operational-use boundary.

### Planning-only replacement

If the CM/MF/BQ context has not yet been operationally consumed, replacing the selected Tool may update that same component-context identity in place:

```text
jobon_id = unchanged
context_id = unchanged
old tool selection -> new tool selection
no downstream operational history on that context
```

This is a planning edit, not a rewrite of operational history.

### Replacement after operational use

Once operational work has consumed the context, replacing the Tool creates a new context identity:

```text
jobon_id = unchanged

<old_context_id> -> <old_tool_id>
<new_context_id> -> <new_tool_id>
```

The previous `cm_id`, `mf_id` or `bq_id` remains attached to the Tool it originally represented. It is never retargeted to the replacement `tool_id`.

A lot change follows the same rule because another lot means another canonical Tool.

### Context-change awareness

A relevant **operational** production-context edit also follows the lightweight awareness rule. A planning-only edit to a future/not-yet-consumed context updates planning truth but does not create an urgent operational context-change ping merely because the plan changed:

```text
change
-> permanent Job On change log
-> lightweight awareness for consumers of that context
-> module acknowledgement when seen
```

Acknowledgement means only that the change was seen. It does not resolve, correct, recalculate, approve or rewrite downstream work.

The awareness mechanism is defined in `CONTEXT_CHANGE_AWARENESS.md`.

Where a requested change would otherwise invalidate or conflict with already-created dependent records, the implementation must preserve those historical facts rather than pretending the change is harmless.

The exact allowed edit boundary beyond this awareness mechanism is determined by the applicable dependency rules; it must not be inferred from UI convenience.
