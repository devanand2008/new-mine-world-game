# Graphics update verification

Local Windows/Chrome verification for art update 0.2, 11 October 2026.

- Unity 6000.3.25f1 successfully compiled C#, imported assets and built the WebGL player. No C# or shader compile errors were reported. Final output is `CubeFrontier/Builds/Web`; build log is `TestResults/Unity-Web-Build.log`.
- All 29 core checks passed: deterministic terrain, chunk boundaries, edits, inventory, recipes and validated binary saves. Generator and save codec were not changed.
- `Tests/BrowserAcceptance.mjs` passed on the upgraded runtime: native pointer lock, mouse look, mining, placement, flight, WASD, inventory, hotbar and third-person view. SHA256-valid world saves were read from real IndexedDB, then reloaded after a full Chrome restart. Test artifacts are in `TestResults/BrowserAcceptance`.
- `Tests/BrowserGraphics.mjs` visits all thirteen explorer appearances and nine creature designs in the compiled game. Screenshots were visually inspected. Helmet/hood clipping found during inspection was corrected. Selecting Eclipse Sentinel persisted after Chrome restarted and the selected model appeared in third-person gameplay. Menu art was verified after fixing the PNG import settings to use 2D textures.
- `Tests/BrowserLandscape.mjs` uses seeds 207 and 621 to inspect the Crystal Marsh terrain, grass edges, crystals, shoreline and water. Screenshots are in `TestResults/BrowserLandscape`.
- `Tests/BrowserModes.mjs` passed on the final build: Survival and Horror each started, captured the pointer and ran for 27 seconds without game errors. Survival inventory also opened. This does not verify full combat or Horror completion.
- `Start-Game.bat -CheckOnly` confirmed that the compiled local build and Node.js were ready. The final menu, all 22 gallery models, selected third-person appearance, forest and river screenshots were inspected. Final gallery contact sheets are `TestResults/BrowserGraphics/explorer-contact.png` and `creature-contact.png`.

All browser tests use isolated profiles/contexts and leave the user's saved worlds untouched. Browser console output still includes Unity's manual filesystem-sync deprecation warning, unsupported optional URP shader messages, and a harmless missing favicon response. No game exceptions were observed in the completed checks. These are functional and visual checks, not an FPS or memory certification.

Full combat balance, completion of the Horror objective, every material at every quality level, Firefox and long-session profiling remain unverified. The new pet, mount, two additional creature types and final boss are design previews; no gameplay behavior for those five models was claimed or tested.

To play, double-click `E:\GAME\Start-Game.bat`, open **Characters**, choose **Wear this appearance**, return to the menu and start/load a world. Press **F5** for third-person view. The review mapping all supplied documents is in [ART-REFERENCE-REVIEW.md](ART-REFERENCE-REVIEW.md).
