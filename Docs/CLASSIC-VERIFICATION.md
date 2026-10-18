# Classic blocks / fly-through verification — 0.6

Verified on 15 October 2026 with Unity 6000.3.25f1 and the compiled Web player.

Unity build succeeded. `Build/Web.wasm` SHA-256:

`9B5DE76F3BCCBC7BE2A4DC38F4251CDD99E01D2B6513FAF48138E9D6C40C0A4F`

## Checks passed

- 87 core checks, including save compatibility and building invariants.
- `BrowserFlight.mjs`: normal flight stops at a three-block-thick wall; G crosses it without changing any saved block; G/F refuse to restore collision while embedded; G restores collision in open space; normal walking/floor collision return; saving inside blocks records a safe return point; that save reloads safely; Survival ignores the Creative flight controls.
- `BrowserDesignScreens.mjs`: navigation at three desktop sizes, 14 screenshots, no browser/runtime errors. The classic menu, pixel-texture world, active fly-through HUD and material catalogue were visually inspected.

Browser checks used isolated test profiles, not the user's browser saves. The current results are under `TestResults/BrowserFlight` and `TestResults/BrowserDesignScreens`. Earlier Creative update verification documents are historical and refer to the previous build.

See `CLASSIC-FLIGHT.md` for controls and save behavior. Original edited source files are backed up in `TestResults/ClassicBackup-20261015`.
