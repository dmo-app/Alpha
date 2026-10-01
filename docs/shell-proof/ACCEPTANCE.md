# Alpha workspace correction acceptance

Validated on 2026-10-01. The former prototype implementation was reproduced
inside Alpha. Existing Alpha foundation/authentication work was preserved.
No other module was migrated or developed by this correction.

| Gate | Status / evidence |
|---|---|
| Alpha is the only writable application workspace | PASS. All new source, assets, contracts, fixtures, scripts, dependencies, docs and outputs are in Alpha. Reference source hashes are unchanged. |
| One shared Razor shell, presentation only | PASS. Shared partials and contract were ported without business logic or permission resolution. No shell markup remains duplicated in the migrated Boquilhas content. |
| Branding/title/session presentation | PASS. Portal DMO, module-only title area, user/logout preserved; operator/chief and logout differential checks pass. |
| Planeamento destination and module functions | PASS. Planeamento is not a module identity. Functions remain Registo, Boquilhas, Histórico, Definições in the original order. |
| Boquilhas behavior preserved | PASS. Accepted-proof comparison passes for markup, inline CSS/JS, script ordering, rail selection, movement persistence, history, repairer settings, query semantics, DOM and sessionStorage. |
| Compatibility belongs to Alpha | PASS. Alpha owns beta-adapter.js and beta-session.js; no outside modifications are required. The legacy header renderer remains omitted on the proof. |
| No external runtime/test dependency | PASS. External static-file bridge removed. Only Alpha-owned fixtures/assets/dependencies are used; no reference-root configuration is read. |
| Unconverted destinations | Original URLs/query parameters preserved. Unmigrated destinations return 404; no external fallback and no extra module migration. |
| Fully resolved native session/capability handoff | PARTIAL. The accepted browser demo session remains. Alpha now has an authentication foundation, but integration of that foundation into the shell contract is not performed by this workspace-only task. No permissions changed or fabricated. |
| Visual/keyboard proof | PASS. Browser inspection at 1366 x 768; keyboard activation of Histórico/Definições; all four entries and production rail preserved; no JavaScript console errors observed. Existing CSS and prior mobile limitations are carried unchanged. No redesign undertaken. |
| Development order / migration scope | PASS. Boquilhas remains only a technical proof. No development order changed. |

## Build and tests

Final cleanup verification on 2026-10-01:

- Solution `dotnet build --no-restore --nologo`: PASS, zero warnings/errors.
- Shell verification: PASS, including local fixture comparisons, assets served
  from Alpha, movements/settings/session persistence, all four functions,
  unchanged Boquilhas routes/query behavior and no external fallback.
- Solution `dotnet test --no-restore --nologo --verbosity minimal`: PASS,
  15 passed, 0 failed, 0 skipped. One test assembly.
- No current test failures were observed. The previously observed auth mismatch
  is recorded below; this cleanup did not modify its test or auth behavior.
- **SHELL PROOF DONE** within the accepted technical-proof scope. Native session
  integration remains outside this scope.

## Files modified in Alpha by this correction

- `src/DMO.Alpha.Web/Program.cs`: Alpha-local Data Protection key directory;
  the temporary external reference bridge has been removed. Existing native
  auth/services/middleware are preserved.
- `src/DMO.Alpha.Web/Pages/_ViewImports.cshtml`: add the presentation-contract using;
  preserve the existing namespace and tag helpers.

The solution, project references, native login/logout pages, native auth tests,
Core/Infrastructure and recovery documentation were not modified by this correction.

## Earlier changes in the prototype workspace

These occurred in the preceding implementation, before this correction:

- `beta-adapter.js`: proof-only guards for duplicated/reordered shell UI.
- `beta-session.js`: bind resolved demo account to the rendered header.
- `31_BOQUILHAS_01_VISUAL_AUTHORITY_boquilhas.html`: removed/replaced by the old Razor endpoint.
- `shell-proof.css`: created for the old proof.
- `RazorShellProof/`: old project, contracts, pages/partials, tests and documentation created there.

They have not been restored, edited or deleted during this correction: the user
made the prototype read-only. The old proof host has been stopped. Historical
artifacts may remain as reference, but are not a development target or build/test
dependency. No further writes are required outside Alpha.

## Files created in Alpha

- `docs/shell-proof/ACCEPTANCE.md`
- `docs/shell-proof/README.md`
- `docs/shell-proof/proof-desktop.png`
- `src/DMO.Alpha.Web/Pages/Boquilhas/Index.cshtml`
- `src/DMO.Alpha.Web/Pages/Boquilhas/Index.cshtml.cs`
- `src/DMO.Alpha.Web/Pages/Shared/_DmoBranding.cshtml`
- `src/DMO.Alpha.Web/Pages/Shared/_DmoModuleTitle.cshtml`
- `src/DMO.Alpha.Web/Pages/Shared/_DmoNavigation.cshtml`
- `src/DMO.Alpha.Web/Pages/Shared/_DmoSessionUtilities.cshtml`
- `src/DMO.Alpha.Web/Pages/Shared/_DmoShellLayout.cshtml`
- `src/DMO.Alpha.Web/Pages/Shared/_ModuleFunctionEntry.cshtml`
- `src/DMO.Alpha.Web/Pages/Shared/_ModuleFunctionsContainer.cshtml`
- `src/DMO.Alpha.Web/Pages/Shared/_PlaneamentoTab.cshtml`
- `src/DMO.Alpha.Web/Presentation/ShellPresentation.cs`
- `src/DMO.Alpha.Web/wwwroot/0_ASSET_CANONICAL_DESIGN_SYSTEM.css`
- `src/DMO.Alpha.Web/wwwroot/0_ASSET_LOGO.png`
- `src/DMO.Alpha.Web/wwwroot/0_ASSET_SHELL.css`
- `src/DMO.Alpha.Web/wwwroot/beta-adapter.css`
- `src/DMO.Alpha.Web/wwwroot/beta-adapter.js`
- `src/DMO.Alpha.Web/wwwroot/beta-boquilhas-flow.js`
- `src/DMO.Alpha.Web/wwwroot/beta-calendar.css`
- `src/DMO.Alpha.Web/wwwroot/beta-production-overview.css`
- `src/DMO.Alpha.Web/wwwroot/beta-production-overview.js`
- `src/DMO.Alpha.Web/wwwroot/beta-session.js`
- `src/DMO.Alpha.Web/wwwroot/beta-settings.js`
- `src/DMO.Alpha.Web/wwwroot/beta-shell.css`
- `src/DMO.Alpha.Web/wwwroot/boquilhas-boundary.js`
- `src/DMO.Alpha.Web/wwwroot/shell-proof.css`
- `src/DMO.Alpha.Web/wwwroot/tool-boundary.js`
- `tests/ShellProof/baseline-assets.json`
- `tests/ShellProof/fixtures/accepted-boquilhas.html`
- `tests/ShellProof/package.json`
- `tests/ShellProof/verify-shell.cjs`

Third-party test dependencies were copied as the jsdom dependency closure only
into ignored `tests/ShellProof/node_modules`; no other prototype application pages
or unrelated files were copied. Build/runtime outputs stay in ignored bin/obj/temp.

## Previously observed auth failure — Module 1 — Login/Auth

- Test: `AuthenticationTests.Logout_ClearsSession`.
- Observed mismatch: expected `/login`; actual challenge URL
  `http://localhost/login?ReturnUrl=%2F`.
- Owner: **Module 1 — Login/Auth**. Historical result: 14 passed, 1 failed of 15.
- Final rerun: 15/15 passed, including `Logout_ClearsSession`. The current
  workspace test uses `Assert.Contains("/login", loginRedirect)` for the
  post-logout challenge, whereas the prior run expected exact `/login`.
  This cleanup did not change that test. The prior mismatch is retained for
  Module 1 review; it is not a currently reproduced failure.
- No auth test, challenge handling, logout workflow or permissions were changed.
  This cleanup does not start Module 1 development.

## Final cleanup

Removed the external runtime provider, external baseline argument, optional
external source-hash reads and test expectations that required reference pages.
Owned asset hashes and differential behavior checks remain. No temporary runtime
bridge is needed. Alpha can build and serve its own pages without the prototype
folder. Legacy demo scripts remain only as the accepted local proof compatibility;
they do not read any external filesystem path.
