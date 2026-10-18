# First Frontier 0.7 verification

Completed checks reviewed 16 October 2026. These results verify the local survival increment, not the full master brief.

| Check | Result | Evidence |
|---|---|---|
| Core C# | 105 assertions passed | `Tools/Test-Core.ps1` |
| Unity Play Mode survival sequence | 44 assertions passed | `TestResults/PlayModeMilestone/result.json` and `checks.txt` |
| Browser survival UI and persistence | 8 scenarios passed, zero captured browser errors | `TestResults/BrowserQuest/result.json` |
| Browser Creative flight regression | 7 scenarios passed, zero captured browser errors | `TestResults/BrowserFlight/result.json` |
| Local HTTP server | 12 tests passed | `Tests/LocalServer.test.mjs` |
| Windows build | Succeeded | `TestResults/Unity-Windows-Build.log` |
| Web build | Succeeded | `TestResults/Unity-Web-Build.log` |
| Native Windows UI | Main menu, Survival terrain, journal and complete window layout inspected at 1024 × 640; native v4 save decoded successfully | Windows game run and local save inspection |

The Play Mode sequence drives the production mining and placement raycasts through `IPlayerInput`. It mines timber and stone, crafts tools, places an actual enclosed shelter, verifies it, trades, crafts a blade, starts night watch, dodges a wind-up, takes real damage, defeats a creature through the production melee interaction, claims the commission, and saves/reloads the shelter, character and reward. Its saves are redirected to `TestResults/PlayModeMilestone`, outside the player's normal save store.

The browser suite uses actual keyboard/mouse input and reads checksummed IndexedDB saves. Controlled fixtures make merchant access and reward-boundary scenarios repeatable; the browser completion screenshot is a test world, not evidence of manually earning every objective there. The separate Play Mode test earns those objectives through the runtime interactions. Native Windows inspection also verified a new Survival world's zeroed quest state.

Unity's editor emitted a SearchDatabase indexing exception during startup before the Play Mode driver began. The runtime sequence and final player builds completed successfully; this editor indexing issue is not counted as a gameplay pass or hidden as a clean editor log.

Web player SHA-256 (`Builds/Web/Build/Web.wasm`):
`40817033476A7185D47155516DCF1CABC1286C0FEE5754501BD1738FD7D04498`

No measured FPS certification, multiplayer test, controller test, Android build, full boss encounter or class-ability acceptance is claimed. Remaining requirements are tracked in `FIRST-FRONTIER.md`.
