# The Horizon Update — 0.8

This is a visual redesign of the existing playable game, using original code-generated artwork. It keeps the square, pixelated voxel style and Creative flight controls.

## Visual direction

Deep blue surfaces replace the muddy green panels. Mint identifies primary actions and active selections; gold highlights survival goals and important feedback. Light text, stronger borders and shallow block shadows carry through menus, settings, character previews, crafting, building blueprints, the journal, trading, pause screens and save-slot confirmation.

The home screen has an atmospheric blueprint-grid background, a new block emblem, a larger adventure headline, a richer animated village model with a bridge, timber, planted rows and crystals, and clearer mode choices. The browser's loading and surrounding interface use the same palette and wording.

Tools, weapons, food, armour and shields have original 16-pixel illustrations instead of letter placeholders. The same images appear in the hotbar, bag, recipes and merchant trades. The hotbar also shows actual remaining durability.

The HUD adds a heading compass, in-world clock, segmented survival meters and a compact next-objective tracker. Notifications move to the top while paused and below the crosshair while playing, with a separate position when a building preview is active. Quest rows show checked progress visually, and equipment has its own darker column.

World presentation adds a square moon, discrete stars, warm sunrise/sunset haze, brighter turquoise water, softer grass/stone texture contrast and a more balanced foliage palette. The terrain structure and saved block identities are unchanged.

## Play and source

- Windows: `E:\GAME\Start-Windows.bat`.
- Browser: `E:\GAME\Start-Game.bat`, then `http://127.0.0.1:8080/`.
- E: crafting/materials; J: journal/equipment; T: nearby merchant; B: houses; H: help; Esc: pause/save.
- Creative: F normal flight; G fly through blocks; Space up; Ctrl down.

Most interface work is in `Runtime/FrontierUI.cs`, `BuilderUI.cs`, `QuestUI.cs` and the new `ItemIllustrations.cs`. Menu artwork is in `CreativeVista.cs`. Lighting changes are in `GameSession.cs`; the sky/water shaders and generated atlas supply the world treatment. The browser template is under `Assets/WebGLTemplates/Frontier`.

Source and template backups from before this update are in `E:\GAME\TestResults\DesignBackup-20261016`. Save format remains version 4. This design release does not represent completion of the larger class, farming, companion, boss or multiplayer roadmap; current gameplay status is documented in `FIRST-FRONTIER.md`.
