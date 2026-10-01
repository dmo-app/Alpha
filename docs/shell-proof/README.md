# Shared Razor shell proof in Alpha

All application source, assets, contracts, fixtures, verification dependencies and
runtime outputs belong to `D:\AI Dev\DMO\Alpha`. No external reference-root
configuration or filesystem bridge is used. Only Boquilhas is migrated.

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
