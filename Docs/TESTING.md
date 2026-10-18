# Verification

## Executed

Run `Tools/Test-Core.ps1` from the workspace. It compiles the production Core files together with `Tests/CoreTests.cs` and executes 29 checks. All 29 passed on 2026-10-11. This is not Unity compilation.

`node --check Tools/Serve-Web.mjs` passed. The preview server checks for a real Unity build and reports its absence instead of serving a mock game.

Additional checks passed: preview paths resolve the index and WASM files, reject encoded Windows traversal and malformed URL encoding; the PowerShell build script parses without errors; the scene's bootstrap GUID matches `GameSession.cs.meta`. Launching the preview server without a build correctly reports that no Unity Web build exists. These checks do not exercise gameplay or an actual browser.

## Initial launcher verification — before Unity installation, 2026-10-11

- `Start-Game.bat -NoBrowser` was executed with `FRONTIER_NO_PAUSE=1`. It reported the missing Unity Editor/build and returned exit code 1. No playable game was launched.
- Editor searches were expanded to C:, D:, E: and installed-program records; no Unity installation was found.
- `node --test Tests/LocalServer.test.mjs` passed all 11 subtests (12 test-runner entries including the parent). Checks use temporary, explicitly synthetic HTTP fixtures under `TestResults/`, then remove only those fixture directories. They cover missing/incomplete builds, file presence checks, malformed/traversal paths, real loopback HTTP responses, WASM MIME/encoding, HEAD, 404/405 errors, and project identity for server reuse.
- Automatic Unity compilation, browser launch, and real game execution remain untested because the required Editor and game build are absent.

## Successful local build and Chrome verification — 2026-10-11

- Unity Hub, Unity 6000.3.25f1 and Web Build Support were installed on E:, and an active license was verified. The Editor path is `E:\Unity\6000.3.25f1\Editor\Unity.exe`.
- The corrected Unity build log reports `Build Finished, Result: Success.` and exit code 0. The WebAssembly, framework, loader and data files exist in `CubeFrontier/Builds/Web/Build`.
- `Start-Game.bat -CheckOnly` returns success. The real local server reports this project's identity and serves `.wasm` as `application/wasm`.
- `node Tests/BrowserAcceptance.mjs` passed against the compiled Unity player in Chrome. It checks native pointer lock, actual mouse look, mining, placement, flight, WASD, hotbar selection and IndexedDB persistence. Saved horizontal travel exceeded 10 blocks and height increased from about 36.08 to 48.51 during flight.
- The test completely closes Chrome, relaunches the same isolated profile, loads the saved world via its menu, and confirms matching seed, mode, horizontal position, inventory, selected slot and all three block edits. Flying is not persisted; vertical settling after reload is expected.
- Screenshots of terrain, third-person view, the inventory, the saved status and the reloaded world were inspected. Final browser checks reported zero runtime exceptions and zero failed game asset requests. Optional URP shader-support messages and a filesystem-sync deprecation warning remain in the log.
- `node Tests/BrowserModes.mjs` passed startup checks for Survival and Horror, each observed for 27 seconds after choosing the mode. Terrain, native mouse capture, Survival's pack/crafting interface, and Horror's objective/countdown were visible; no browser exceptions occurred. Completion and full combat/crafting behavior were not certified by these short checks.
- Artifacts are in ignored `TestResults/BrowserAcceptance/`: `result.json`, `browser.log` and screenshots. The test profile is isolated from the user's normal browser and is retained there.
- Test dependency setup: `npm install --prefix TestResults/BrowserTools playwright`. The test uses installed Chrome, so no separate browser download is required. Start the actual local game server first.

These checks establish a playable local prototype. They do not certify every item in the full acceptance checklist below.

## Full Unity acceptance — partially executed

### Phase A

1. Import using Unity 6000.3.25f1; Console must have zero compilation/shader errors. Run Prepare Project twice and confirm it does not create duplicate assets.
2. Open Frontier scene and start seed 714 in Survival. Wait for nearby collision meshes. Confirm terrain is visible, grounded spawn is safe, WASD/Space/Shift work, and mouse locking works from the menu's click.
3. Walk across x/z = 0, ±16, ±32. Check for cracks, missing surfaces, stuck movement and falling through chunks. Walk past render distance and confirm old meshes/data are released in the Profiler.
4. Mine every material, build on a chunk boundary, and check neighboring faces update. Bedrock must resist mining; placement inside the player must fail; water must not become a solid collision floor.
5. Save to each of three slots. Restart the Editor and load each. Confirm seed, position, inventory, durability, needs, hotbar and edits restore. Change a cell back to its procedural block and verify the delta count shrinks.
6. Corrupt a copied save, preserving the originals. Confirm the backup loads and subsequent saves recover. Test full/denied storage and verify the UI reports failure.
7. Creative: flight up/down, instant mining, unlimited blocks, palette selection and third-person camera wall collision. Menus must pause controls and Esc must release the pointer.

### Phase B

1. Fill inventory, craft with missing materials and with no output space. No ingredients should disappear on failure. Check swapping slots and durability after mining/attacks.
2. Test health, hunger, stamina, food consumption, falling, enemy hits, death and respawn. Creative must not take ordinary damage.
3. Exercise patrol, pursuit, investigation, retreat and death. Check obstacles and cliffs; record stuck paths. Verify every defeat cleans up controllers/meshes.
4. Complete/fail Horror runs, toggle horror effects off, pause mid-run and switch back to the menu. No old creatures, lights or audio sources should remain in the next world.

## Full browser acceptance — Chrome local flow passed; remaining checks pending

- Build Web, serve over HTTP locally and HTTPS on the staging host; no `file://` testing.
- Test separate normal browser profiles in Chrome and Firefox. Confirm loading errors are actionable, fullscreen works, and returning from Esc/tab suspension pauses safely.
- Save, await the "Saved to this device" message, close/reopen the browser, and verify state. Repeat with private mode and storage blocked; do not claim persistence if unavailable.
- Inspect network responses: `.wasm` is `application/wasm`; encoded variants match Content-Encoding; no missing data/framework files.
- Profile generation, collider cooking, edits at borders, seven creatures, and distance changes. Record median/95th/99th frame times, heap size, draw calls and triangles. Target ~60 FPS on capable desktop hardware, with measured Low/Medium/High settings.

## Multiplayer/security acceptance — future, not run

Real separate-browser join/leave, WSS failures, full/offline/expired rooms, reconnect and authority migration, player movement/health/selected state, nearby chunk edit replication, late joins, and simultaneous edits must pass before calling multiplayer functional.

API tests must cover unauthenticated/non-admin access; password setup/rotation; expired/revoked/replayed invitation tokens; attempt rate limits; collision-free concurrent room allocation; membership-bound saves; forbidden cross-user/world writes; transactional crafting; damage/movement limits; audit writes; snapshot recovery and schema migrations. No such API exists in this prototype.
