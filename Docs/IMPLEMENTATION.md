# Implementation report and known issues

**Current release: 0.7.** See [First Frontier](FIRST-FRONTIER.md) for the current feature list and master-brief phase status. Merchant trading, quest persistence, equipment controls and a Windows build are now implemented. The sections below record earlier milestones and are historical; their older counts and limitations do not describe every current feature.

## Builders update 0.4

The current source integrates 54 selectable block materials, the searchable Creative catalogue, held items, manual repeat placement and block picking, four editable building plans with placement preview/rotation and protected undo, and a saved starter settlement. The integrated expansion also adds biome variation, natural/rare materials, recipes, basic weather, dropped items and projectile code. See [controls and limits](BUILDERS-UPDATE.md) and [current build verification](BUILDERS-VERIFICATION.md).

Integration fixes unify the 256 × 512 texture atlas across terrain/held blocks/icons, prevent menu clicks from building or mining, restore food/ammunition stacking, and validate equipment saves. Save format 3 records the generator version. Version-1 worlds keep their original terrain, verified against sixteen hashes from the original compiled generator; the engine-independent suite passes 87 checks. New gameplay systems have not all received full combat or balance acceptance testing.

## Graphics update 0.2

All 18 concept sheets and the 49-page character guide in the supplied folder were reviewed. The [reference audit](ART-REFERENCE-REVIEW.md) maps every sheet to its visual implementation. Thirteen cosmetic player appearances and nine creature/animal models now have a 3D turnaround gallery; the existing four enemy types use their new models during play. Meshes combine layered pieces per articulated limb and share a vertex-colour shader. Simple limb and bat-wing motion is implemented; authored skeletal clips, exact cinematic artwork reconstruction and the PDF's proposed class abilities are not.

Terrain visuals add 32-pixel patterned block tiles, grass/wood face variants, corner occlusion, transparent animated water, decorative meadow plants, daylight adjustments, a gradient sky, square sun and drifting block clouds. Generator and save format remain unchanged. Browser validation for this update is recorded separately in [GRAPHICS-VERIFICATION.md](GRAPHICS-VERIFICATION.md).

## Milestone status

| Milestone | Status | Evidence / gate |
|---|---|---|
| Audit | Complete | Empty workspace and available tooling inspected; supplied artwork preserved |
| A: terrain, movement, mining, building, saves | Local Chrome flow verified; full acceptance remains | See Builders verification for the current compiled player; core 87 checks passed |
| B: survival, crafting, enemies, Horror | Partial implementation; mode startup verified | Survival and Horror Chrome startup checks passed; complete survival/combat/Horror acceptance remains |
| C: actual multiplayer and room admin | Not implemented | Requires Phase A/B verification, UGS configuration, backend and real two-browser tests |
| D: backend persistence, security, optimization, build and deployment | Web build and local serving verified; remaining work pending | Compiled Web build served at localhost; no backend, performance certification or public deployment |

The local single-player prototype is playable. The broader Phase A acceptance checklist is still incomplete, and multiplayer/backend milestones remain unimplemented.

## Verified here

- Actual compilation of the engine-independent C# files using the installed .NET Framework compiler.
- 87 executable assertions: original world/inventory/save regressions plus construction materials, glass faces, house plans and rotations, atomic rejection, protected undo, ration/ammunition stacking, equipment persistence/validation, save migration and legacy terrain hashes.
- Node syntax check of the Web preview server. Other checks, if run, are recorded in `TESTING.md`.
- Added `Start-Game.bat` with Editor discovery, build-on-first-run, local serving and browser opening. Unity 6000.3.25f1 and Web Build Support are installed on E:, the license is active, and the corrected Web build completed successfully. A concrete BoxCollider reference fixes primitive creation in the stripped browser player. Eleven HTTP/prerequisite subtests pass using isolated test fixtures.
- Real Chrome testing passed for menus, terrain, native pointer lock, mouse look, mining, stone placement, flight, WASD, hotbar selection, inventory and third-person view. The saved coordinates changed with input; SHA256-valid saves appeared in IndexedDB; the edited world, inventory and hotbar selection restored after completely closing and reopening Chrome with the same isolated profile. See `TESTING.md` and `LOCAL-VERIFICATION.md`.

## Unverified engine integration

Unity installation, license, package resolution, engine C# compilation, scene import, a Web build and the Chrome local-play flow have succeeded. The full material/chunk-boundary checklist, all save slots, blocked/private storage, Firefox, actual FPS and memory profiling remain unverified. Source review does not substitute for these checks. Chrome also logs unsupported optional URP debug/copy shaders and a filesystem-sync deprecation warning; the tested terrain, models and UI render successfully without browser exceptions.

## Functional limitations

- Water has an animated transparent surface and swimming movement, without fluid simulation, real reflections/refraction or underwater effects. The expansion adds basic precipitation and static lava hazards. No greedy meshing, jobs/Burst, spatial occlusion culling, LOD or object pool yet. Per-vertex ambient occlusion is implemented.
- One chunk's terrain generation and final mesh/collider upload still occur on the main thread. Mesh extraction yields every eight levels. Profile spikes before increasing render distance; 60 FPS has not been demonstrated.
- Finite vertical range is 64 blocks; horizontal coordinates are bounded to ±100,000. Not an infinite-height world.
- Base terrain has surface biomes, simple noise caves and rivers; no complete chamber/ruin generation outside the three Horror locations.
- Inventory/recipes are centralized C# data definitions, not authored ScriptableObject assets. Equipment data and armor recipes exist, but there is no equip/unequip interface. Bow/projectile and dropped-item code is integrated; full combat acceptance remains. No crafting station requirement, functional chest or trading. Crates and bookshelves are decorative blocks.
- Creature behavior uses a small local obstacle-steering rule with cliff checks, not full voxel pathfinding. Creatures may stick in complex terrain. Brutes and bats can spawn near the surface. Models have simple procedural joint motion; detailed combat clips and exact art-sheet reproduction remain absent. Four creature/animal models are gallery previews only; Ash Hound and Shadow Archer have simple additional models.
- Horror is a four-minute, three-beacon challenge with a Warden and configurable flicker. Relic investigation/restoration is a single proximity interaction. No persisted Horror progression, whisper recordings or full escape sequence.
- Three local save slots only. Binary format version 3 records the generator version and reads formats 1 and 2. Older released saves use the preserved original generator; new worlds use the expanded generator. Older game builds cannot read new saves. Save checksum detects corruption, not cheating. Maximum 250,000 edited cells per save.
- Creative flight and third-person toggles are not stored in save format 1. A world saved while flying reloads in ordinary movement and the player settles onto the terrain; horizontal position, inventory and edits are retained.
- Browser storage is local to the origin/profile and may be cleared/denied by browser policy. Private-window persistence and quota failure need real tests. Unload cannot guarantee completion of an asynchronous browser save; use the explicit save status before closing.
- Single-player pauses on focus loss. No reconnect path exists because multiplayer is absent.
- IMGUI interfaces are a prototype: keyboard/mouse priority, no complete accessible controller/touch navigation. Game rendering uses a 1280×800 logical UI scaled to the viewport.
- No executable backend, database migrations, administrator panel, authentication or deployment. Their absence is exposed in the menu rather than presenting fake working controls.

## Next concrete steps

1. Complete the remaining Phase A acceptance checks in `TESTING.md`, including all materials, chunk boundaries, save slots and storage failures.
2. Extend the verified Chrome flow to Firefox and staging HTTPS; measure performance and memory on this laptop.
3. Complete and verify missing Phase B systems and performance bottlenecks before expanding content.
4. Implement the separate API/database and isolated UGS session integration, including permission/expiry/replay tests. Then verify actual state exchange in separate browser sessions.
5. Harden persistence and hosting, profile the real browser build, and deploy only a build that passes HTTPS smoke tests.
