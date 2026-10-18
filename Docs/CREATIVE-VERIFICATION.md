# Creative update 0.5 verification

Verified 15 October 2026 on the installed Unity 6000.3.25f1 Web player.

## Build

Unity reports `Build Finished, Result: Success` and exits with code 0. The player is in `CubeFrontier/Builds/Web`. The SHA-256 of `Build/Web.wasm` is:

`02AE407DA4D68BC8ABED2D0401F9F1EA3158CD168490A6044A56020C72FF5D05`

`Start-Game.bat -CheckOnly` passes. The local server serves the player at `http://127.0.0.1:8080`.

## Completed checks

- Core harness: 87 checks passed, including save compatibility, inventory, terrain and blueprint invariants.
- Local-server suite: 12 tests passed.
- Design navigation: 13 screenshots across 1280 x 908, 1440 x 900 and 1024 x 768 browser viewports. Home, settings, explorer gallery, bestiary, Creative HUD, controls, material catalogue, blueprints, pause and replacement confirmation were exercised without browser/runtime errors. Home, settings, gallery, HUD, material and house screens were visually inspected. A clipped caption and oversized tower icon were corrected and rebuilt.
- Building acceptance: material search (including typing E), material selection, menu-to-preview input separation, rotated cottage placement, exact undo, original-generator preservation and restart persistence all passed against actual saved world data. This also verifies the search-focus fix: selecting a category or material releases the search field so E can close the catalogue.
- Mode checks: Survival and Horror loaded and captured the pointer without browser/runtime errors during the observed startup period. Survival inventory was verified in a checksum-valid save. These are startup checks, not full combat or Horror completion certification.
- Final gameplay acceptance on the build above: mouse look, mining, stone-brick placement, flight, WASD movement and hotbar selection were confirmed in actual saved state. Inventory and third-person controls were exercised. World edits, inventory, selected slot and horizontal position were restored after a Chrome restart, with valid save checksums and no reported browser errors or failed network requests.

## Evidence

The build log is `TestResults/Unity-Web-Build.log`. Browser results and screenshots are in `TestResults/BrowserDesignScreens`, `TestResults/BrowserBuildings`, `TestResults/BrowserModes` and `TestResults/BrowserAcceptance`.

All browser automation used isolated test profiles. It did not read or modify the user's normal browser saves. The source backup remains in `TestResults/DesignBackup-20261014`.

The previous `BUILDERS-VERIFICATION.md` documents a historical build. Its unverified blueprint preview issue is resolved by the successful current building acceptance test.
