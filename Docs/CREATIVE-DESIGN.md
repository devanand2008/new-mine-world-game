# Creative design update 0.5

Design direction: bright, playful creative building. Updated 15 October 2026.

## Presentation

- Cream surfaces, dark blue-green text, teal controls, coral and golden accents.
- Creative-first home screen with an original animated 3D village diorama. The preview is built from meshes, uses an isolated camera and shader, and stops rendering during gameplay or other menu screens.
- Separate seed, save-slot and starter-village controls. Existing-world replacement uses a modal confirmation, with the underlying controls disabled.
- Matching browser loading screen, progress indicator and fullscreen button.
- Rounded, illustrated material and house cards; matching character studio, settings and pause screens.
- Compact gameplay HUD, larger hotbar icons, block-name labels and optional controls help with **H**. Survival retains health, hunger and stamina indicators.
- Brighter daylight fill and a softer blue horizon. New Creative worlds start with grass, plaster, stone bricks, wood, leaves, glass, roof tiles, dark timber and torches.

## Playing

Launch `E:\GAME\Start-Game.bat`. Select **Creative playground** to build with unlimited materials, **Survival adventure** to gather and craft, or **Horror night** for the existing timed challenge.

Press **E** for materials, **B** for house blueprints, **F** for Creative flight, **R** to rotate a blueprint, and **Z** to undo an unchanged placed house. Press **Esc** to pause and save. Saves remain on the same browser and device at the existing localhost origin.

## Scope and recovery

The redesign preserves the existing game modes, 54 materials, four house blueprints, character appearances and save format. The main-menu village is a decorative original model; playable houses are selected from the in-game blueprint menu.

Original edited source files, the browser template and README were copied to `TestResults/DesignBackup-20261014` before editing. Browser checks use disposable profiles, not the user's saved worlds. The source backup is not a backup of browser saves or a complete pre-update binary build.

Live multiplayer, cloud saves, new character abilities and the other production features listed in `IMPLEMENTATION.md` remain outside this visual update.

## Verification

See [current compiled-player verification](CREATIVE-VERIFICATION.md).

Core and server checks: `Tools/Test-Core.ps1` and `node --test Tests/LocalServer.test.mjs`.

Browser checks after rebuilding: `BrowserDesignScreens.mjs` covers navigation and screenshots at three desktop sizes; `BrowserBuildings.mjs` covers material search, preview, rotation, placement, undo and restart persistence; `BrowserModes.mjs` covers Survival and Horror startup. Test outputs are under `TestResults`.
