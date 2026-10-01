# Shared Razor shell proof in Alpha

All application source, assets, contracts, fixtures, verification dependencies and
runtime outputs belong to `D:\AI Dev\DMO\Alpha`. No external reference-root
configuration or filesystem bridge is used. Only Boquilhas is migrated.

## Current route-to-layout ownership

| Route | Layout in `src/DMO.Alpha.Web/Pages/Shared` | Role |
| --- | --- | --- |
| `/` | `_DmoShellLayout.cshtml` | Operational shared Shell |
| `/Modules/Home` | `_DmoShellLayout.cshtml` | Operational shared Shell |
| `/31_BOQUILHAS_01_VISUAL_AUTHORITY_boquilhas.html` | `_DmoShellLayout.cshtml` | Operational shared Shell |
| `/login` | `_Layout.cshtml` | Auth/session special case |
| `/change-password` | `_Layout.cshtml` | Auth/session special case |
| `/logout` GET | `_Layout.cshtml` | Auth/session special case |

The operational pages explicitly select `_DmoShellLayout`; the auth/session
pages inherit `_Layout` from `Pages/_ViewStart.cshtml`. These layouts serve
distinct page sets. There is no evidence of two operational Shells competing
on the same page.

`tests/ShellProof/fixtures/accepted-boquilhas.html` is **PROOF_ONLY**: the test
harness reads it from disk; it is not an application route.

The legacy alternate-shell hooks in `wwwroot/beta-adapter.js` do not render an
alternate Shell on pages with `data-razor-shell="true"`: legacy navigation and
user-navigation branches are guarded off, and the `.dmo-app-header` relocation
target is absent. Other adapter behavior remains active on Boquilhas.

Boquilhas explicitly mounts `Shared/_DmoSidepanelBoquilhas.cshtml` with its
visible Boquilhas sidepanel state. `Shared/_DmoSidepanelPlaneamento.cshtml`
exists but has no current mounting reference; it is unused, not declared obsolete.

### Planeamento sidepanel readiness

`Shared/_DmoSidepanelPlaneamento.cshtml` exists but is currently **UNMOUNTED**
and is not integration-ready.

- **Presentation:** no shared Shell mounting, open/close handler, or partial
  visibility guard exists. `_DmoShellLayout` only emits sidepanel state as a body
  attribute. The rail styles in `beta-adapter.css` and
  `beta-production-overview.css` are currently loaded by the Boquilhas page.
- **Data:** no producer supplies `PlaneamentoSidepanelPresentation`, and no
  native source supplies its current day, following productions, or calendar/ping
  state. `IProductionContextQuery.GetAsync(jobOnId)` retrieves a single Job On's
  context; it does not supply planning data.
- **Navigation:** no native Razor Job On/Planeamento destination exists.
  Production `Href` values are caller-supplied; the partial emits them as
  `data-jobon-href`, which has no navigation handler. Null values disable buttons.
- **Demo evidence:** the related `beta-production-overview.js` uses hard-coded
  production rows, fixed fixture dates/machines, and sessionStorage keys
  `betaJobOnCalendarDetached` and `betaJobOnSummaries`. Its prototype planning
  behavior is evidence only, not implementation authority for native production
  ordering or calendar semantics; it is not wired to the Planeamento partial.

### Boquilhas sidepanel readiness

`Shared/_DmoSidepanelBoquilhas.cshtml` is **ACTIVE_RUNTIME**, mounted by the
Boquilhas page. Its active rail remains demo-driven, not native integration.

- **Presentation:** `beta-adapter.js` calls `betaProductionOverview.renderRail`,
  replacing the initial hard-coded Razor rail contents. Rail styles remain
  page-owned through `beta-adapter.css` and `beta-production-overview.css`.
  No shared Shell open/close controller exists.
- **Data:** machine labels and default production rows are fixtures; runtime
  rows merge sessionStorage data, and the active-card highlight is browser state.
  No visible rail field comes from a native server query. The current-production
  selection per machine (descending demo date) is prototype logic, not native
  product authority.
- **Navigation:** clicking a populated card switches to the existing in-page
  Registo and prefills descriptive context only. It does not resolve canonical
  Tool/BQ/trace identity. Double-clicking reloads Boquilhas with `?view=registo`
  and passes no contextual identifiers. The rail does not navigate to Histórico
  or Job On. This is partial UI behavior, not native contextual routing.
- **Native replacement:** no safe end-to-end replacement is currently available
  within sidepanel scope. Missing dependencies are a native current-production-
  per-machine source and established selection rules, verified mapping to
  canonical Boquilhas context, native Boquilhas authorization, and connected
  native read/write services. Existing registered write handlers are not called
  by this page and do not supply its rail data or context mapping.

## Run from Alpha

```powershell
dotnet build --no-restore
dotnet run --project src/DMO.Alpha.Web --no-build --no-launch-profile -- --urls http://127.0.0.1:5190 --environment Development
```

Alpha's native `/login` remains available. The Boquilhas proof route is
`/31_BOQUILHAS_01_VISUAL_AUTHORITY_boquilhas.html?view=registo`.
Its accepted demo-session compatibility scripts are Alpha-local. The automated
proof harness seeds existing demo accounts in sessionStorage; it does not need a
legacy login page. For manual proof inspection, set `betaDemoUser` to `1003` or
`1001` in sessionStorage on this origin and reload the Boquilhas route.

Links to unconverted legacy pages retain their original URLs. Those destinations
are not hosted by Alpha and return 404; serving them would require a separate
migration. Legacy demo login/logout destinations are likewise preserved, not
replaced with new authentication workflows. No external fallback is provided.
Native session/capability integration remains outside this technical proof.

Data Protection keys stay in the Web project's ignored `temp/data-protection`
directory, protected by Windows DPAPI on Windows.

## Verify

```powershell
node tests/ShellProof/verify-shell.cjs http://127.0.0.1:5190
dotnet test --no-restore
```

Verification always uses `tests/ShellProof/fixtures/accepted-boquilhas.html`,
local asset hashes and local `node_modules`. No external baseline argument or
reference-root environment variable is read. `package.json` pins jsdom;
`npm install --prefix tests/ShellProof` restores dependencies if needed.

See `ACCEPTANCE.md` for gate results and the deferred Module 1 auth assertion.
`proof-desktop.png` is the carried-forward accepted visual baseline; Alpha was
separately inspected at 1366 x 768 during the workspace correction.
