# Admin — App Definições

This file defines the centralized administrative configuration surface for module-owned settings.

## 1. Purpose

Operational modules should not each consume navigation space with their own rarely used `Definições` destination.

Selected configuration is centralized under:

```text
Admin
→ App Definições
→ select module
→ edit that module's settings
```

The user selects the module whose settings are to be changed.

This is an Admin configuration surface, not a standalone Modules tab.

## 2. What belongs here

App Definições is intended primarily for module settings that are:

- changed infrequently;
- specialized or niche;
- important enough that casual changes could disrupt the workflow;
- administrative rather than part of the module's daily operational hot path.

A setting does not belong here merely because it exists.

The owning module defines which settings are administratively configurable.

## 3. Ownership remains with the module

Centralized editing does not make Admin the owner of every setting.

Example:

```text
Boquilhas production-activation time
→ owned semantically by Boquilhas
→ edited through Admin → App Definições → Boquilhas
```

Likewise, a Controlo-specific document or recipient setting remains a Controlo setting even if its editing UI is reached through App Definições.

There must be one canonical stored setting, not:

```text
Admin copy
+
module copy
```

## 4. No per-module settings destination required

A module may have a `DEFINICOES.md` blueprint file to document configuration it owns.

That filename does **not** imply a visible `Definições` tab inside the operational module.

The UI rule is:

```text
daily operational module
→ operational functions only

Admin → App Definições
→ administrative module configuration
```

This keeps the normal application navigation focused on daily work.

## 5. Relationship to Templates

Templates and App Definições both mention modules but serve different purposes.

```text
Templates
→ modules/capabilities/permissions
→ USER ACCESS

App Definições
→ module-owned settings
→ MODULE CONFIGURATION
```

Changing a module setting must not alter which Users have access to that module.

Changing a Template must not silently change the module's administrative settings.

## 6. Initial confirmed example — Boquilhas

Boquilhas owns a configurable daily production-activation time used for planned Job On production transitions.

Its configuration is exposed through:

```text
Admin
→ App Definições
→ Boquilhas
→ production activation time
```

The setting controls planned production-transition timing for Boquilhas only.

It does not delay immediate awareness when the context inside the same `jobon_id` changes.

It is not a global application production-switch time.

## 7. Historical safety

Changing a module setting affects behavior according to that setting's own module rules.

Administrative configuration must not silently rewrite historical operational facts merely because a setting was changed later.

Each owning module remains responsible for defining any historical-stability requirement relevant to its settings.
