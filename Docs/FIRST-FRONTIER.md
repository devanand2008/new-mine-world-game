# First Frontier — version 0.7

This increment implements the first survival commission from the supplied master brief. It is not the completed twelve-class multiplayer RPG.

## Play

Windows: run `E:\GAME\Start-Windows.bat`. Browser: run `E:\GAME\Start-Game.bat`, then open `http://127.0.0.1:8080`. The Windows and browser versions have separate local save stores. Save before closing or refreshing.

Choose an appearance before creating a Survival world. Each world now saves its character independently. Older saves use the starter explorer because their format did not store a character.

- **J:** commission journal and equipment. It also shows Mara's distance and X/Z coordinates.
- **T:** trade when within four metres of Mara, with a clear line of sight.
- **E:** pack and field crafting. Select armour in a hotbar slot, then use J to equip it. Unequipping requires an empty pack slot; swapping equipment preserves durability.
- **Esc:** pause, save and return to the menu.
- Creative retains **F** normal flight and **G** flight through blocks; Space rises and Ctrl descends.

## Mara's commission

Mine at least four timber and eight stone blocks, craft a tool, place twelve blocks, verify a shelter, and defeat a creature at night. Starter supplies and generated houses do not grant gathering or placement counters. Progress can be made in any order.

Shelter verification is deliberately simple: stand below a solid roof two to four blocks above your feet, with solid two-block-high walls in at least three cardinal directions within three blocks. At least twelve manual placements are required. It is an introductory enclosure check, not a flood-fill or structural integrity simulation.

After verifying the shelter, Mara offers **Begin night watch** to advance to night. Creatures spawn on nearby loaded terrain. Keep food ready and craft a blade. Melee creatures wind up for 0.55 seconds before checking current reach and intervening terrain. Moving away or behind a wall can avoid the strike. Night attacks deal 25% more damage.

Return to Mara after completing the commission to claim eight crystals. A full pack leaves the reward unclaimed; claiming twice cannot duplicate it. Mara exchanges wood for food, stone for ore, and crystals for a vest or iron sword. Exchanges use atomic inventory transactions: failed trades do not charge resources.

## Save compatibility

Format 4 appends the character and bounded quest progress after existing world edits. It retains checksum validation and reads versions 1–3. Earlier game builds cannot read format 4. Source backup before this increment: `TestResults/MilestoneBackup-20261015`.

## Master brief status

| Phase | Current status | Remaining gate |
|---|---|---|
| Foundation | Unity 6.3 LTS, URP, streamed chunks, movement, mining, placement, inventory, durability and local saves work | New Input System/controller support, ScriptableObject definition migration, greedy meshing and large-coordinate strategy |
| First survival | Terrain, caves, trees, field crafting, shelter construction, enemies, merchant and persistent commission implemented | Broader seed/device playtesting and balance |
| Full sandbox | Construction palettes and blueprints exist; armour management now works | Functional storage, stations, doors/stairs/slabs, beds, farming/cooking progression, companion and mount |
| Characters | Thirteen original procedural appearances selectable and saved per world | Twelve gameplay classes, active/passive abilities, cooldowns, detailed animation/VFX and validation |
| Enemies and bosses | Seven basic creature variants; melee/ranged combat, telegraph and night scaling | Full enemy specifications, robust navigation, two multi-phase bosses and their encounters |
| Multiplayer | Not implemented; menu clearly states its status | NGO/Transport, authority, replication, lobbies, two-player verification, 2–8 co-op, optional PvP |
| Polish/optimization | Original atlas, procedural models/audio, weather, UI and bounded chunk streaming | Profiling and FPS evidence, broader graphics/animation work, accessibility and controller/mobile support |

The world currently uses 16 × 16 × 64 chunks and a coordinate bound of ±100,000; it is not floating-origin or truly endless. Chests and many decorative blocks remain decorative. Character weapons shown in appearance meshes do not imply class abilities. Horror runs remain session-only. No multiplayer, Android, performance, or complete-master-brief claim is made by this release.

## Build and tests

`Tools/Build-Windows.ps1` and `Tools/Build-Web.ps1` invoke the installed Unity editor. `Tools/Test-Milestone.ps1` runs the isolated Play Mode sequence. `Tools/Test-Core.ps1` covers inventory, saves, terrain, construction, quest validation, trades, equipment and shelter rules. `Tests/BrowserQuest.mjs` uses disposable browser profiles and explicitly identified fixture scenarios for trading/reward tests. `Editor/MilestoneVerification.cs` is an editor-only Play Mode integration test; it redirects saves to TestResults and drives production raycasts through the input abstraction. It is excluded from player builds.
