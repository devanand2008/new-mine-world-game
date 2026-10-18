// Short startup checks for Survival and Horror; completion and combat are separate acceptance gates.
import { createRequire } from 'node:module';
import { fileURLToPath } from 'node:url';
import path from 'node:path';
import fs from 'node:fs';
import assert from 'node:assert/strict';
import {readWorldSave} from './ReadWorldSave.mjs';
const workspace = path.resolve(path.dirname(fileURLToPath(import.meta.url)), '..');
const require = createRequire(path.join(workspace, 'TestResults/BrowserTools/package.json'));
const { chromium } = require('playwright');
const output = path.join(workspace, 'TestResults/BrowserModes'); fs.mkdirSync(output, { recursive: true });
const browser = await chromium.launch({ channel: 'chrome', headless: true });
const result = [];
try {
  for (const [mode, y] of [['Survival', 573], ['Horror', 664]]) {
    const context = await browser.newContext({ viewport: { width: 1280, height: 908 } });
    const page = await context.newPage(), errors = [], logs = [];
    page.on('pageerror', error => errors.push(error.message));
    page.on('console', message => { logs.push(`${message.type()}: ${message.text()}`); if (message.type() === 'error' && !message.text().includes('404')) errors.push(message.text()); });
    try {
      await page.goto('http://127.0.0.1:8080', { waitUntil: 'domcontentloaded' });
      await page.waitForFunction(() => document.getElementById('loading')?.style.display === 'none', null, { timeout: 180000 });
      await page.waitForTimeout(1800); await page.mouse.click(300, y); await page.waitForTimeout(12000);
      await page.keyboard.press('ArrowLeft');
      await page.waitForFunction(() => document.pointerLockElement?.id === 'unity-canvas', null, { timeout: 8000 });
      await page.waitForTimeout(15000); await page.screenshot({ path: path.join(output, `${mode}.png`) });
      if (mode === 'Survival') {
        await page.keyboard.press('e'); await page.waitForTimeout(500); await page.screenshot({ path: path.join(output, 'Survival-pack.png') });
        await page.keyboard.press('e');await page.waitForTimeout(600);await page.keyboard.press('Escape');await page.waitForTimeout(500);await page.mouse.click(640,472);await page.waitForTimeout(2200);
        const contents=await page.evaluate(async()=>{
          for(const info of await indexedDB.databases()){
            if(!info.name.includes('idbfs'))continue;
            const db=await new Promise((resolve,reject)=>{const r=indexedDB.open(info.name);r.onsuccess=()=>resolve(r.result);r.onerror=()=>reject(r.error);});
            let found;
            if(db.objectStoreNames.contains('FILE_DATA'))await new Promise((resolve,reject)=>{
              const tx=db.transaction('FILE_DATA','readonly'),r=tx.objectStore('FILE_DATA').openCursor();
              r.onsuccess=()=>{const row=r.result;if(!row)return;if(String(row.key).endsWith('/CubeFrontier/world-0.cfe'))found=Array.from(row.value.contents);row.continue();};tx.oncomplete=resolve;tx.onerror=()=>reject(tx.error);
            });db.close();if(found)return found;
          }
        });
        assert.ok(contents,'Survival save exists');const saved=readWorldSave(Buffer.from(contents));
        assert.equal(saved.mode,0);assert.ok(saved.inventory.some(s=>s[0]===103&&s[1]===4),'Ration stack persists');
        fs.writeFileSync(path.join(output,'Survival-save.json'),JSON.stringify(saved,null,2));
      }
      assert.equal(errors.length, 0, `${mode}: ${errors.join('\n')}`);
      result.push({ mode, loaded: true, pointerLocked: true, observedSeconds: 27, saveVerified:mode==='Survival',errors });
    } finally { fs.writeFileSync(path.join(output, `${mode}.log`), logs.join('\n')); await context.close(); }
  }
  console.log(JSON.stringify(result, null, 2));
} finally { fs.writeFileSync(path.join(output, 'result.json'), JSON.stringify(result, null, 2)); await browser.close(); }
