import {createRequire} from 'node:module';
import {fileURLToPath} from 'node:url';
import path from 'node:path';
import fs from 'node:fs';
import assert from 'node:assert/strict';
const root=path.resolve(path.dirname(fileURLToPath(import.meta.url)),'..');
const require=createRequire(path.join(root,'TestResults/BrowserTools/package.json'));
const {chromium}=require('playwright');
const out=path.join(root,'TestResults/BrowserLandscape');fs.mkdirSync(out,{recursive:true});
const browser=await chromium.launch({channel:'chrome',headless:true});
const results=[];
try {
 for(const seed of [207,621]) {
  const context=await browser.newContext({viewport:{width:1280,height:908}}),page=await context.newPage(),errors=[];
  page.on('pageerror',e=>errors.push(e.message));
  page.on('console',m=>{if(m.type()==='error'&&!m.text().includes('404'))errors.push(m.text());});
  try {
   await page.goto('http://127.0.0.1:8080');
   await page.waitForFunction(()=>document.getElementById('loading')?.style.display==='none',null,{timeout:180000});await page.waitForTimeout(4200);
   await page.mouse.click(765,691);await page.keyboard.press('Control+a');await page.keyboard.type(String(seed));
   await page.mouse.click(300,482);await page.waitForTimeout(15000);await page.keyboard.press('ArrowLeft');
   await page.waitForFunction(()=>document.pointerLockElement?.id==='unity-canvas',null,{timeout:8000});
   await page.keyboard.press('f');await page.keyboard.down('Space');await page.waitForTimeout(550);await page.keyboard.up('Space');
   await page.keyboard.down('w');await page.waitForTimeout(1100);await page.keyboard.up('w');
   await page.mouse.move(300,552,{steps:12});await page.waitForTimeout(1800);
   await page.evaluate(()=>window.getSelection()?.removeAllRanges());
   await page.screenshot({path:path.join(out,`water-${seed}.png`)});
   assert.equal(errors.length,0,errors.join('\n'));results.push({seed,errors,loaded:true});
  } finally {await context.close();}
 }
 console.log(JSON.stringify(results,null,2));
} finally {fs.writeFileSync(path.join(out,'result.json'),JSON.stringify(results,null,2));await browser.close();}
