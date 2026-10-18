// Exercises the compiled Unity player with real browser input and persisted saves.
// The profile is isolated under TestResults; user browser data is never touched.
import { createRequire } from 'node:module';
import { fileURLToPath } from 'node:url';
import path from 'node:path';
import fs from 'node:fs';
import assert from 'node:assert/strict';
import {readWorldSave} from './ReadWorldSave.mjs';
const workspace = path.resolve(path.dirname(fileURLToPath(import.meta.url)), '..');
const require = createRequire(path.join(workspace, 'TestResults/BrowserTools/package.json'));
const { chromium } = require('playwright');
const output = path.join(workspace, 'TestResults/BrowserAcceptance');
const profile = path.join(output, `profile-${Date.now()}`);
fs.mkdirSync(output, { recursive: true });
const url = process.env.FRONTIER_URL || 'http://127.0.0.1:8080';
const logs = [], errors = [], networkFailures = [];
let context, page;
const result = { profile, checks: [] };
async function open() {
  context = await chromium.launchPersistentContext(profile, { channel: 'chrome', headless: process.env.FRONTIER_HEADED !== '1', viewport: { width: 1280, height: 908 } });
  // Chromium's initial persistent-profile tab rejects pointer lock under automation.
  // A new tab exercises the actual native API correctly, without API overrides.
  const initialPages = context.pages();
  page = await context.newPage();
  for (const initial of initialPages) await initial.close();
  page.on('pageerror', error => errors.push(error.message));
  page.on('console', message => {
    const text = message.text(); logs.push(`${message.type()}: ${text}`);
    if (message.type() === 'error' && !text.includes('404')) errors.push(text);
  });
  page.on('response', response => { if (!response.ok() && !response.url().endsWith('/favicon.ico')) networkFailures.push(`${response.status()} ${response.url()}`); });
  await page.goto(url, { waitUntil: 'domcontentloaded' });
  await page.waitForFunction(() => document.getElementById('loading')?.style.display === 'none', null, { timeout: 180000 });
  await page.waitForTimeout(1800);
}
async function shot(name) { await page.screenshot({ path: path.join(output, `${name}.png`) }); }
async function waitForLock() {
  // Unity defers a GUI mouse-up lock request to the next user gesture.
  // An unused arrow key activates capture without mining a block on entry.
  await page.waitForTimeout(250);
  if (!await page.evaluate(() => document.pointerLockElement?.id === 'unity-canvas')) await page.keyboard.press('ArrowLeft');
  await page.waitForFunction(() => document.pointerLockElement?.id === 'unity-canvas', null, { timeout: 8000 });
}
async function readSave() {
  const files = await page.evaluate(async () => {
    const dbs = await indexedDB.databases();
    const files = [];
    for (const info of dbs) {
      if (!info.name?.includes('idbfs')) continue;
      const db = await new Promise((resolve, reject) => { const request = indexedDB.open(info.name); request.onsuccess = () => resolve(request.result); request.onerror = () => reject(request.error); });
      if (db.objectStoreNames.contains('FILE_DATA')) {
        await new Promise((resolve, reject) => {
          const transaction = db.transaction('FILE_DATA', 'readonly');
          const cursor = transaction.objectStore('FILE_DATA').openCursor();
          cursor.onsuccess = () => {
            const row = cursor.result;
            if (!row) return;
            if (String(row.key).endsWith('/CubeFrontier/world-0.cfe')) files.push({ path: row.key, bytes: Array.from(row.value.contents) });
            row.continue();
          };
          transaction.oncomplete = resolve; transaction.onerror = () => reject(transaction.error);
        });
      }
      db.close();
    }
    return files;
  });
  assert.equal(files.length, 1, 'One actual world save must exist in IndexedDB.');
  return {path:files[0].path,...readWorldSave(Buffer.from(files[0].bytes))};
}
async function saveFromPause() {
  await page.mouse.click(640, 472);
  await page.waitForTimeout(2200);
  return readSave();
}
async function resume() { await page.mouse.click(640, 408); await waitForLock(); await page.waitForTimeout(300); }
try {
  await open(); await shot('01-menu'); result.checks.push('Unity menu loaded over HTTP');
  await page.mouse.click(300, 482); await page.waitForTimeout(12000); await waitForLock();
  await shot('02-creative'); result.checks.push('Creative terrain and pointer lock');
  assert.equal(errors.length, 0, errors.join('\n'));
  await page.keyboard.press('Escape'); await page.waitForTimeout(500);
  result.baseline = await saveFromPause(); await resume();
  await page.keyboard.press('f'); await page.keyboard.down('Space'); await page.waitForTimeout(180); await page.keyboard.up('Space');
  await page.keyboard.press('3');
  await page.mouse.move(640, 795, { steps: 12 }); await page.waitForTimeout(350);
  await page.mouse.down(); await page.waitForTimeout(70); await page.mouse.up(); await page.waitForTimeout(600);
  await page.keyboard.press('Escape'); await page.waitForTimeout(500);
  result.mined = await saveFromPause();
  assert.ok(result.mined.edits.some(edit => edit[3] === 0 && !result.baseline.edits.some(before => JSON.stringify(before) === JSON.stringify(edit))), 'Mining must persist a new air edit, not a pre-existing settlement edit.');
  await resume();
  await page.mouse.click(640, 795, { button: 'right' }); await page.waitForTimeout(600); await shot('03-block-edit');
  await page.keyboard.press('Escape'); await page.waitForTimeout(500);
  result.edited = await saveFromPause();
  assert.ok(result.edited.edits.some(edit => edit[3] === 18 && !result.mined.edits.some(before => JSON.stringify(before) === JSON.stringify(edit))), 'Placement must persist a new stone-brick edit from the redesigned starter hotbar.');
  result.checks.push('Mouse look, block mining and stone-brick placement confirmed in saved edits');
  await resume();
  // Flight clears terrain obstacles and lets saved coordinates prove movement occurred.
  await page.keyboard.down('Space'); await page.waitForTimeout(900); await page.keyboard.up('Space');
  await page.keyboard.down('w'); await page.waitForTimeout(900); await page.keyboard.up('w');
  await page.keyboard.press('3'); await page.keyboard.press('F5'); await page.waitForTimeout(500); await shot('03-movement-third-person');
  await page.keyboard.press('e'); await page.waitForTimeout(600); await shot('04-inventory');
  await page.keyboard.press('e'); await page.waitForTimeout(400);
  await page.keyboard.press('Escape'); await page.waitForTimeout(500);
  result.saved = await saveFromPause(); await shot('05-saved');
  assert.equal(result.saved.seed, 714); assert.equal(result.saved.mode, 1); assert.equal(result.saved.selected, 2);
  assert.ok(result.saved.y > result.baseline.y + 2, 'Flight must increase height.');
  assert.ok(Math.hypot(result.saved.x - result.baseline.x, result.saved.z - result.baseline.z) > 2, 'W must move through the world.');
  result.checks.push('Flight, WASD and hotbar selection confirmed in saved state', 'Inventory and third-person input exercised', 'Checksum-valid world persisted in IndexedDB');
  await context.close(); context = null;
  await open(); await shot('06-reopened-menu');
  await page.mouse.click(1120, 692); // Load world, logical y=628 plus header.
  await page.waitForTimeout(12000); await waitForLock();
  await shot('07-reloaded-world');
  // Pause immediately and save the freshly loaded world for state comparison.
  await page.keyboard.press('Escape'); await page.waitForTimeout(500);
  result.reloaded = await saveFromPause();
  assert.equal(result.reloaded.seed, result.saved.seed); assert.equal(result.reloaded.mode, result.saved.mode);
  assert.equal(result.reloaded.selected, result.saved.selected);
  assert.ok(Math.hypot(result.reloaded.x - result.saved.x, result.reloaded.z - result.saved.z) < .2, 'Reloaded horizontal position');
  assert.deepEqual(result.reloaded.inventory, result.saved.inventory); assert.deepEqual(result.reloaded.edits, result.saved.edits);
  result.checks.push('World, horizontal position, hotbar, inventory and edits restored after Chrome restart');
  assert.equal(errors.length, 0, errors.join('\n')); assert.equal(networkFailures.length, 0, networkFailures.join('\n'));
  result.passed = true; console.log(JSON.stringify({ passed: true, checks: result.checks, resultFile: path.join(output, 'result.json') }, null, 2));
} catch (error) {
  result.passed = false; result.failure = error.message; console.error(error.stack);
  if (page) await shot('failure').catch(() => {}); process.exitCode = 1;
} finally {
  if (context) await context.close();
  result.runtimeErrors = errors; result.networkFailures = networkFailures;
  fs.writeFileSync(path.join(output, 'result.json'), JSON.stringify(result, null, 2));
  fs.writeFileSync(path.join(output, 'browser.log'), logs.join('\n'));
}
