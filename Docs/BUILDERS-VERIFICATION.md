# Builders update 0.3: local verification

Verification date: 12 October 2026.

## Compiled player

The Unity 6000.3.25f1 build in `CubeFrontier/Builds/Web` completed successfully. The tested `Build/Web.wasm` SHA-256 is:

`4CCE75A1FE83A8F756002C379F91AFF07CF4A1FDCD4D4505275CA678DCA6E24B`

This compiled player contains the 0.3 Builders update: 43 selectable materials, a searchable catalogue, illustrated hotbar, held items, four building plans and a starter settlement. Newer gameplay changes currently being made by another editor are not included in this build.

`Start-Game.bat -CheckOnly` passed. The launcher can serve this player at `http://127.0.0.1:8080`.

## Confirmed in Chrome

`node Tests/BrowserAcceptance.mjs` passed with an isolated browser profile:

- Terrain and native mouse capture.
- Mouse look, mining and manual block placement, confirmed by changes in the actual saved world.
- Flight, WASD and selected hotbar slot, confirmed in saved player state.
- Inventory and third-person controls.
- SHA-256-valid saves in IndexedDB.
- World edits, inventory, hotbar selection and horizontal position restored after closing and reopening Chrome.

The building-specific browser run confirmed that the starter village renders and saves its windows/roofs and that choosing Oak Planks replaces the selected hotbar slot. Screenshots are in `TestResults/BrowserBuildings`.

## House preview regression and source fix

The building run did **not** pass its complete acceptance test. Clicking **Preview & place** was also handled as a world placement click. This built the house before the player could rotate it; the later test click mined the house and correctly prevented undo of a manually edited building.

`Runtime/BuildingTool.cs` now waits for the menu mouse buttons to be released and a short input delay to pass before accepting placement input. It also revalidates the construction site on the placement click. `Tests/BrowserBuildings.mjs` now explicitly checks that selecting a plan does not change the world, then verifies a rotated cottage's dimensions, placement, exact undo and browser-restart persistence.

**This source fix has not yet been compiled or verified in the player.** A rebuild is deferred while another editor is changing the project. The existing compiled player's preview issue therefore remains. The broader source additions also require their own integration checks before publishing a new build.

## Next rebuild

Once source edits are stable, build with `Tools/Build-Web.ps1`, then rerun `Tests/BrowserBuildings.mjs`, `Tests/BrowserAcceptance.mjs` and the relevant core/mode checks. The browser save readers currently parse the version-1 layout; adapt them if the newer save-format changes are included, preserving state and checksum assertions. The expansion's atlas tile indices also need to fit the actual atlas dimensions.

Do not treat the historical 64 passing core checks from 0.3 as verification of the newer source additions. Automated browser fixtures use disposable profiles and do not modify the user's browser saves.
