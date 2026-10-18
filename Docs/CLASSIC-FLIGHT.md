# Classic blocks and fly-through — 0.6

Requested direction: Minecraft-like voxel presentation with the ability to fly through blocks. Cube Frontier keeps its own name, artwork, terrain and mechanics.

## Controls

- **G**: toggle fly-through in Creative. Enabling it also enables flight.
- **WASD**: move horizontally; **Space** rises; **Left Ctrl** descends.
- **G** again in open space: restore block collision while remaining in normal flight.
- **F**: toggle normal Creative flight. From fly-through, F returns to normal movement only when the player is clear of solid blocks and terrain is loaded.
- **E**: blocks/inventory. **B**: house blueprints. **H**: controls help. **Esc**: pause/save.

The HUD identifies fly-through while it is active. G and F cannot enable flight in Survival or Horror. Fly-through does not remove or modify blocks.

## Safe return

Collision cannot be re-enabled while the player's body overlaps solid voxel data or the current terrain has not loaded. Fly into clear space and toggle again.

Fly-through is a session control, not a new save format. If a world is saved while the player is inside solid blocks, the save records a nearby safe spawn position. The live player stays where they are until they move; reloading restores normal collision at the safe position. Saves from previous versions remain supported.

## Visual changes

Original, point-filtered 16-pixel block patterns; more saturated grass and earthy dirt; a mottled stone texture; dark square beveled panels; light text and green highlights; a block-texture menu background; a dark hotbar and a thin black block outline. The browser loading page follows the classic theme.

The previous bright-design source files were copied to `TestResults/ClassicBackup-20261015` before editing. These are source backups, not browser-save backups.

## Verification

`Tests/BrowserFlight.mjs` uses an isolated browser profile and a generated wall fixture to verify normal collision, traversal without editing the wall, refusal to restore collision while embedded, safe saving/reloading, restored collision and Creative-only controls. The standard building and browser gameplay tests remain available.
