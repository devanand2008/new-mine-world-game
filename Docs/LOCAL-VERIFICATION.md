# Local play verification — 2026-10-11

**Story:** Start-Game.bat opens the compiled Unity game over localhost; the player starts a world, moves, mines/builds, saves to browser storage, and loads that edited world after restarting Chrome.

| Boundary | Status | Evidence |
|---|---|---|
| Editor and license | Passed | Unity 6000.3.25f1 plus Web Build Support installed on E:, active license verified |
| Source to Web player | Passed | `TestResults/Unity-Web-Build.log`: `Build Finished, Result: Success.`; process log ends with return code 0 |
| Launcher prerequisites | Passed | `Start-Game.bat -CheckOnly` reports `Ready to launch` |
| HTTP asset delivery | Passed | Project health endpoint matches the build path; WASM MIME is `application/wasm`; no failed game asset requests in Chrome |
| Menu to gameplay | Passed | Native pointer lock, visible terrain and playable Creative world; screenshots inspected |
| Input to world state | Passed | Mouse mining produces an air edit; placement produces a stone edit; WASD/flight change saved coordinates; hotbar changes selected slot |
| Save to storage | Passed | Real IndexedDB world file has a valid SHA256 checksum; UI displays `Saved to this device` |
| Storage to reload | Passed | Chrome closed and reopened with the same isolated profile; world menu load restores seed, mode, horizontal position, inventory, hotbar and all three edits |
| Other local modes | Startup passed | `Tests/BrowserModes.mjs`: Survival and Horror each ran for 27 seconds with native pointer lock and zero browser exceptions; screenshots inspected |

Executable verification: `node Tests/BrowserAcceptance.mjs`. Evidence: `TestResults/BrowserAcceptance/result.json`, `browser.log`, and screenshots.

The first browser build failed primitive model creation because its collider class was stripped. Referencing `BoxCollider` explicitly fixes it. The persistent-browser test initially used Chrome's startup tab, which rejected pointer lock; creating a fresh tab fixes that automation issue without replacing or simulating the pointer-lock API.

Creative flight/third-person toggles are not saved in format 1. Vertical position settles onto terrain after reloading a flying world. Full Survival/Horror completion, Firefox, private/denied storage, all materials/save slots, performance profiling and multiplayer remain separate acceptance work.
