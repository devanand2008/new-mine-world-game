// Runs against the real compiled Unity build; never substitutes a fixture for gameplay.
// Install Playwright under TestResults/BrowserTools, then start the local server first.
import { createRequire } from 'node:module';
import { fileURLToPath } from 'node:url';
import path from 'node:path';
import fs from 'node:fs';
const workspace = path.resolve(path.dirname(fileURLToPath(import.meta.url)), '..');
const require = createRequire(path.join(workspace, 'TestResults/BrowserTools/package.json'));
const { chromium } = require('playwright');
const output = path.join(workspace, 'TestResults/BrowserSmoke');
fs.mkdirSync(output, { recursive: true });
const browser = await chromium.launch({ channel: 'chrome', headless: true });
const context = await browser.newContext({ viewport: { width: 1280, height: 908 } });
const page = await context.newPage();
const errors = [], logs = [];
page.on('pageerror', error => errors.push(error.message));
page.on('console', message => {
  logs.push(`${message.type()}: ${message.text()}`);
  if (message.type() === 'error' && !message.text().includes('favicon.ico') && !message.text().includes('404')) errors.push(message.text());
});
try {
  await page.goto(process.env.FRONTIER_URL || 'http://127.0.0.1:8080', { waitUntil: 'domcontentloaded' });
  await page.waitForFunction(() => document.getElementById('loading')?.style.display === 'none', null, { timeout: 180000 });
  await page.waitForTimeout(2000);
  await page.screenshot({ path: path.join(output, '01-menu.png') });
  await page.mouse.click(300, 482); // Creative menu button, canvas begins at y=64.
  await page.waitForTimeout(12000);
  // Unity applies its deferred cursor lock on the next user gesture.
  await page.keyboard.press('ArrowLeft');
  await page.screenshot({ path: path.join(output, '02-creative.png') });
  const locked = await page.evaluate(() => document.pointerLockElement?.id === 'unity-canvas');
  if (!locked) throw new Error('Creative start did not lock the pointer. Inspect screenshot and runtime logs.');
  await page.keyboard.down('w'); await page.waitForTimeout(1800); await page.keyboard.up('w');
  await page.keyboard.press('Space');
  await page.screenshot({ path: path.join(output, '03-after-input.png') });
  await page.keyboard.press('e'); await page.waitForTimeout(600);
  await page.screenshot({ path: path.join(output, '04-inventory.png') });
  await page.keyboard.press('e'); await page.waitForTimeout(400);
  await page.keyboard.press('Escape'); await page.waitForTimeout(500);
  await page.mouse.click(640, 472); // Save world in pause menu.
  await page.waitForTimeout(2500);
  await page.screenshot({ path: path.join(output, '05-save.png') });
  if (errors.length) throw new Error(`${errors.length} browser/Unity errors: ${errors.slice(0, 4).join('\n')}`);
  fs.writeFileSync(path.join(output, 'result.json'), JSON.stringify({ loaded: true, creativePointerLocked: locked, inputSequenceSent: true, errors, note: 'Screenshots require visual inspection. This does not certify all gameplay or save reload.' }, null, 2));
  console.log('Real Web build loaded, Creative locked the pointer, and keyboard/menu inputs completed without browser errors. Inspect screenshots.');
} catch (error) {
  await page.screenshot({ path: path.join(output, 'failure.png') }).catch(() => {});
  fs.writeFileSync(path.join(output, 'result.json'), JSON.stringify({ error: error.message, runtimeErrors: errors }, null, 2));
  console.error(error.message); process.exitCode = 1;
} finally {
  fs.writeFileSync(path.join(output, 'browser.log'), logs.join('\n'));
  await browser.close();
}
