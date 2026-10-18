# Builders update 0.4

This update adds both freehand construction and ready-made houses, as requested. It uses original block art and the existing Cube Frontier character appearances.

## Play

Start `E:\GAME\Start-Game.bat`. Start a new Creative world with **Starter settlement** checked to explore a cottage, lodge, conservatory and watchtower near the arrival area. Follow the path, or use **F** then **Space** to fly above the village. Sites with excessive slopes or water may skip a starter building. Existing worlds load their saved blocks without adding or replacing a settlement.

- **E** opens the material catalogue in Creative. Search or filter Nature, Building and Decor. Pick the hotbar slot on the right, then click a material. The catalogue directly replaces that selected Creative slot with an unlimited stack.
- **Right mouse** places a block; hold it in Creative to build repeatedly. **Left mouse** mines. **Middle mouse** copies the targeted block into the selected Creative slot. Blocks cannot be placed inside the player or a creature.
- **B** opens four house blueprints. In Creative, choose **Preview & place**, aim at nearby ground, press **R** to rotate, then left click when the preview is green. Right click cancels. Occupied or steep sites show a red preview and a reason. The building volume needs to be clear, with ground or a platform below; clear vegetation first if necessary.
- **Z** undoes the last placed house during the current play session if none of its changed cells has since been edited. It restores the prior site, including generated foundations. Manually edited houses are preserved.
- **F5** toggles the selected full character; first-person view now shows a moving hand with the current block or tool.
- In Survival, **E** shows the pack and paged crafting recipes. All thirty-two construction materials have recipes; the expansion's natural/rare materials are gathered or crafted. Instant whole-house placement is a Creative tool; Survival construction uses gathered/crafted blocks.
- Use **Esc → Save world**, wait for the saved status, and keep the same browser/address to reload your buildings.

## Materials and houses

There are **54 selectable block materials**: the original eleven placeable blocks, thirty-two construction materials and eleven further natural/rare materials. Construction materials include oak/spruce/birch planks, dark timber, cobblestone, stone/mossy/clay brick, sandstone, smooth stone, cream plaster, slate/red roof tiles, glass/dark glass, copper/iron/gold, lantern, bookshelf, crate, four wool colours, terracotta, polished basalt, marble, path, gravel, packed ice and carved stone. The integrated expansion adds clay, moss, mushroom, moonstone, ember crystal, shadow ore, ancient relic, spectral block, roots, obsidian and torch. Lava is a generated hazard, not a catalogue material.

The **Meadow cottage** has a red stepped gable, cream walls, oak beams, windows and furnishings. The **Stone & timber lodge** uses stone walls, dark beams, slate roofing and a library area. The **Crystal conservatory** has glass walls/skylight, a copper frame and planters. The **Frontier watchtower** has a block staircase and open battlements; use jump to climb the full-block steps. Doorways are open passages. Bookshelves and crates are decorative full blocks, not storage or interaction systems. Lanterns are emissive blocks, without simulated light propagation.

All buildings consist of real world blocks. You can mine walls, replace roofs and windows, extend rooms, or build entirely by hand. No static building prop replaces editable construction. Starter buildings and placed plans use the same sparse block edits as manual construction and persist in local saves.

## Compatibility and limits

Old block IDs are preserved and new IDs are appended. Save format 3 records the terrain generator, equipment state and experience; formats 1 and 2 remain readable. Worlds from the released version-1 save format retain the original terrain, including when resaved. New worlds use the expanded generator. Resaved worlds cannot be opened by older game builds. The map is still vertically bounded to 64 blocks; all block materials use full-cube geometry. Stairs/slabs, opening doors, fluid simulation, advanced redstone-like systems, functional storage, multiplayer and Java Edition feature parity are not implemented.

The atlas expands to 256 × 512 with 32-pixel material tiles and distinct faces for grass, logs and bookshelves. Terrain, held blocks and UI icons share the atlas dimensions. Glass and ice have a transparent render pass and solid collision. Opaque blocks remain visible through glass. Existing character designs, clouds, water and corner shading are retained. Blueprint placement validates bounds, all occupied cells, ground support, actor overlap, loaded chunks and edit limits before changing the world.

## Verification

The engine-independent suite passes **87 checks**, including original regressions, material validity, glass visibility, construction recipe coverage, hotbar replacement, plan bounds/rotations, rejection without partial writes, protected undo, save migration and sixteen terrain hashes from the original compiled generator. See [current browser verification](BUILDERS-VERIFICATION.md). Browser checks use disposable profiles; the user's saves are not modified by automated tests.
