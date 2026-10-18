# Cube Frontier: Eclipse

Playable Unity/C# local-game prototype with a compiled browser build. **Double-click `E:\GAME\Start-Game.bat`** to play at `http://127.0.0.1:8080`. Choose a mode, then press a movement key or click inside the game to capture the mouse. Esc pauses; use **Save world** and wait for **Saved to this device** before closing.

Unity **6000.3.25f1** and Web Build Support are installed at `E:\Unity\6000.3.25f1`, and the Unity license is active. The engine-independent core passes 105 checks. See [current build verification](Docs/FIRST-FRONTIER-VERIFICATION.md) for browser results. Live multiplayer and the backend remain future work.

## Horizon design update (0.8)

A complete interface refresh with deep-blue panels, mint/gold actions, illustrated item icons, a compass and quest tracker, plus a new night sky, warm dawn/dusk lighting and updated water/terrain colours. The Windows and browser versions share the design. See [design notes](Docs/HORIZON-DESIGN.md).

## First Frontier survival update (0.7)

Windows build: double-click `E:\GAME\Start-Windows.bat`. Survival now includes Mara's physical merchant, real trades, a persistent commission, equipment management, saved character selection, and dodgeable night attacks. **J** opens quest/equipment; **T** trades nearby. See [current features and phase status](Docs/FIRST-FRONTIER.md). The full master brief remains in development.

## Classic blocks + fly-through (0.6)

Classic pixel textures, square beveled menus, a dark hotbar and Creative **G** fly-through controls. **F** retains normal flight. Fly into open space before restoring collision. See [controls and safe saving](Docs/CLASSIC-FLIGHT.md).

## Creative design update (0.5)

Bright, playful creative building: a new village-preview home screen, cream-and-teal interface, redesigned HUD and building cards, matching loading screen, and sunnier world lighting. Press **H** during play for controls. See [design changes and recovery notes](Docs/CREATIVE-DESIGN.md).

## Builders features

**54 building materials**, a searchable catalogue, illustrated hotbar, first-person held items, four editable house blueprints and a starter settlement for new worlds. Press **E** for blocks, **B** for houses, **R** to rotate a house preview, **Z** to undo an unchanged placed house, and middle mouse to pick a block in Creative. See [building controls and limits](Docs/BUILDERS-UPDATE.md).

The integrated expansion also includes additional biomes/materials, tool and weapon recipes, basic weather and dropped items. Older worlds retain their original terrain generator when loaded and saved. New saves use format 4; older builds cannot read them.

## Character and map art

The **Characters** button opens a rotating 3D gallery with 13 explorer appearances and 9 creature designs, based on all 18 supplied sheets and the 49-page PDF. Select an explorer, click **Wear this appearance**, then create a world and press **F5**. Five gallery creature designs appear during play; Ash Hound and Shadow Archer use additional simple models. Class abilities and the equipment shown on character appearances remain cosmetic.

World graphics have grass edges, wood end grain, patterned block textures, corner shading, transparent animated water, glass windows, small plants, brighter daylight, a gradient sky and block clouds. These are original low-poly adaptations, not exact reproductions of the concept renders or Minecraft shader packs. See the [full reference review](Docs/ART-REFERENCE-REVIEW.md).

## Open the project

1. Use the installed **Unity 6000.3.25f1 (6.3 LTS)** at `E:\Unity\6000.3.25f1\Editor\Unity.exe`. On another machine, install this version with **Web Build Support** and activate an appropriate Unity license.
2. In Unity Hub, choose **Add project from disk** and select `E:\GAME\CubeFrontier`.
3. Let package import and script compilation finish. On first import, `ProjectSetup` generates the original block atlas, materials, and URP assets. If necessary, run **Cube Frontier > Prepare Project**.
4. Open `Assets/CubeFrontier/Scenes/Frontier.unity` and press Play.
5. Start Survival or Creative, wait for terrain colliders to load, and run the checks in [TESTING.md](Docs/TESTING.md). Fix all Console errors before accepting the milestone.

Do not create a new Unity project over this folder. Other settings that are not committed are initialized by Unity on first import. No aircraft project or existing user files were present or modified.

## Implemented source

- Seeded 16 Ã— 16 Ã— 64 chunks; original and expanded biome generators, rivers, simple caves, trees, crystal outcrops, and ore.
- Exposed-face meshes and separate solid collision meshes; incremental mesh extraction, bounded render distance, unloading, and sparse edit saves.
- CharacterController movement, jump/sprint/swim, Creative flight, first/third-person view, block outline, mining and placement.
- 27-slot inventory, nine-slot hotbar, transactional field crafting, durability, rations, health/hunger/stamina, fall damage and respawn.
- Thirteen selectable layered explorer appearances, seven gameplay creature types and additional gallery previews; local creature behavior, melee/projectile code, rewards, and a timed three-beacon Horror run.
- Searchable Creative block catalogue, manual construction, four editable house blueprints, placement preview/rotation, protected undo and a starter settlement.
- Main menu, settings, credits, inventory, crafting, pause, three local world slots, and explicit multiplayer development status.
- Versioned binary saves with validation, corruption checks, previous snapshot recovery, and a browser filesystem sync bridge.
- Original generated atlas/audio, supplied concept references, Unity Web loading template, and build/serve scripts.

**Not implemented:** live multiplayer, administrator authentication/panel, room access, backend API/database/migrations, cloud saves, functional chests, full voxel pathfinding, advanced art/animation, performance certification, or production deployment. Horror progress is not saved. See [implementation report](Docs/IMPLEMENTATION.md).

## Controls

| Action | Control |
|---|---|
| Move / look | WASD / mouse |
| Jump / sprint | Space / Left Shift |
| Mine / attack | Left mouse |
| Place / eat selected ration | Right mouse |
| Inventory / field crafting | E |
| House blueprints / rotate preview | B / R |
| Undo last unchanged house (Creative) | Z |
| Pick targeted block (Creative) | Middle mouse |
| Drop one held item | Q |
| Select hotbar | 1â€“9 or mouse wheel |
| Third-person camera | F5 |
| Creative flight / descend | F / Left Ctrl |
| Creative fly-through blocks | G (toggle in open space) |
| Restore nearby haunted beacon | R |
| Pause / release mouse | Esc |

Creative mode does not consume block stacks and ignores ordinary damage. Inventory slots can be swapped with two clicks. Craft a pickaxe from wood and stone, a blade from wood and ore, and trail rations from leaves. These recipes are prototype balance, not final game content.

## Tests and Web build

### Double-click launcher

Double-click **`E:\GAME\Start-Game.bat`**. It checks for a generated Web build; if one is missing, it locates Unity and runs the build script. After a build is available, it starts the local server and opens `http://127.0.0.1:8080` in your browser. Keep the launcher window open while playing; press Ctrl+C to stop the server.

**Local installation:** Unity and Web Build Support are installed on E:, and the first browser build completed successfully. The launcher reuses `CubeFrontier/Builds/Web`. Multiplayer/admin features are still not implemented.

For an Editor installed in a custom folder, run from Command Prompt:

```bat
Start-Game.bat "E:\your-installation\Editor\Unity.exe"
```

Optional arguments: `-Rebuild` to compile source changes, `-NoBrowser` to serve without opening a tab, and `-CheckOnly` to inspect prerequisites. Alternatively set `UNITY_EDITOR_PATH` to the full Editor executable path. Existing builds are reused unless `-Rebuild` is given. The default port stays at 8080 to preserve the browser origin used by local saves.

### Command-line tools

From PowerShell in `E:\GAME`:

```powershell
.\Tools\Test-Core.ps1
node --test .\Tests\LocalServer.test.mjs
# Installed Editor path on this laptop.
.\Tools\Build-Web.ps1 -UnityPath 'E:\Unity\6000.3.25f1\Editor\Unity.exe'
node .\Tools\Serve-Web.mjs
```

The core harness uses the installed Windows .NET Framework C# compiler; it needs no SDK, Unity license, or third-party test packages. Outputs are written to ignored `TestResults/`.

Build output, **when Unity successfully runs**, is `CubeFrontier/Builds/Web/`. The server refuses to start if no generated build exists. Open `http://127.0.0.1:8080` after a build. Never open a Unity build through `file://`.

See [deployment and environment instructions](Docs/DEPLOYMENT.md), [architecture and audit](Docs/ARCHITECTURE.md), and [known issues and milestone status](Docs/IMPLEMENTATION.md).




