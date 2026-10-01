# Boquilhas — Definições

Boquilhas Definições documents configuration owned by the Boquilhas operational workflow.

These settings are edited through `Admin → App Definições → Boquilhas`. This file defines their Boquilhas meaning and behavior; it does not imply a visible Definições tab inside the operational Boquilhas module.

## Production activation time

Boquilhas owns its own configurable daily production-activation time.

This setting determines when Boquilhas adopts the next planned Job On production context for each machine.

Conceptually:

```text
future Job On
→ may already exist as planning information
→ does not change Boquilhas operational context

Boquilhas production-activation time arrives
→ Boquilhas reads Job On
→ resolves the applicable production/BQ context
→ machine card switches to that context
```

This is a Boquilhas setting, not a global application time.

It does **not** delay awareness of a change inside the same operational `jobon_id`. If the BQ context of the production Boquilhas is already using changes, Boquilhas receives that context-change awareness immediately and revalidates Job On.

Changing the configured activation time must not rewrite historical movements or reassign records that already belong to an earlier production context.

## Repairers

The repairer register belongs to Boquilhas Definições.

For the current Beta, repair routing is configured by production machine:

- B1;
- B2;
- B3;
- C1;
- C2;
- C3.

Each machine resolves its current BQ repairer from this configuration.

The movement workflow does **not** ask the operator to choose a repairer from a list.

Conceptually:

```text
BQ tool_id
→ associated machine
→ Boquilhas Definições
→ configured repairer for that machine
→ Saída uses that repairer automatically
```

If a BQ is already in production, the current production context confirms the machine. If it is still pre-production, it may have no current production machine, but its Tool already carries the associated machine used for this repair routing.

The BQ Tool does not own or duplicate the repairer configuration.

## Historical rule

Changing a later machine→repairer configuration must not rewrite historical movements.

The repairer resolved for a movement remains part of that movement's historical truth. The exact persistence representation is an implementation decision.

Definições is current configuration; it must not retroactively change which repairer applied to an earlier real movement.
