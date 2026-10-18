# Horizon 0.8 verification — 16 October 2026

## Builds

The final Windows and WebGL builds both completed successfully with Unity 6000.3.25f1. Their logs record `Build Finished, Result: Success.` and exit code 0.

- Windows: `E:\GAME\CubeFrontier\Builds\Windows\CubeFrontier.exe`
- Browser: `E:\GAME\CubeFrontier\Builds\Web\index.html`
- Logs: `E:\GAME\TestResults\Unity-Windows-Build.log` and `Unity-Web-Build.log`

The final Windows HUD was inspected in the running application. Browser panels use a platform-specific colour conversion to keep their intended dark-blue appearance; label colours are set directly for readable text.

## Interface verification

`node E:\GAME\Tests\BrowserDesignScreens.mjs` completed successfully against the final browser build, with no captured browser/runtime errors. It produced 14 screenshots and exercised menus, settings, character and creature galleries, Creative mode, the G fly-through toggle, help, materials, blueprints, pause, saving, and cancellation of save-slot replacement.

The screen run used 1280 × 908, 1440 × 900 and 1024 × 768 browser viewports. Visual review confirmed readable menus and item screens, the smaller-window home layout, and unobstructed pause/save confirmation actions. Results and screenshots are under `E:\GAME\TestResults\BrowserDesignScreens`.

## Gameplay and persistence

`node E:\GAME\Tests\BrowserHorizonQuest.mjs` passed all eight checks against the final build, with no captured browser/runtime errors:

- Fresh Survival starts with an uncompleted quest.
- The nearby merchant exchanges four wood for two rations.
- Crafting an axe consumes materials and advances the commission.
- An incomplete commission cannot claim its reward.
- Mouse mining collects timber and advances quest progress.
- A completed-quest fixture grants eight crystals only once.
- Buying and equipping armour preserves item counts.
- Character, quest, reward claim, armour and durability survive browser restart.

The suite uses controlled save fixtures for repeatable trade, reward and equipment states; it is not an end-to-end playthrough of every quest objective. Results are under `E:\GAME\TestResults\BrowserHorizonQuest`. The final night screenshot was visually inspected and shows the square moon, stars, and active Creative fly-through indicator.

These checks use isolated test browser profiles. Save format remains version 4. This release updates presentation of the existing local prototype; it does not complete the larger RPG roadmap. Earlier core and PlayMode test results remain documented separately; they were not rerun for this visual release.
