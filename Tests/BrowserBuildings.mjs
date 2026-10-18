import {createRequire} from 'node:module';
import {fileURLToPath} from 'node:url';
import fs from 'node:fs';import path from 'node:path';import assert from 'node:assert/strict';import {readWorldSave} from './ReadWorldSave.mjs';
const root=path.resolve(path.dirname(fileURLToPath(import.meta.url)),'..');
const require=createRequire(path.join(root,'TestResults/BrowserTools/package.json'));const {chromium}=require('playwright');
const out=path.join(root,'TestResults/BrowserBuildings');fs.mkdirSync(out,{recursive:true});
const profile=path.join(out,`profile-${Date.now()}`),errors=[],logs=[],checks=[];let context,page;
async function open(){context=await chromium.launchPersistentContext(profile,{channel:'chrome',headless:true,viewport:{width:1280,height:908}});const old=context.pages();page=await context.newPage();for(const p of old)await p.close();
 page.on('pageerror',e=>errors.push(e.message));page.on('console',m=>{logs.push(m.text());if(m.type()==='error'&&!m.text().includes('404'))errors.push(m.text());});
 await page.goto('http://127.0.0.1:8080');await page.waitForFunction(()=>document.getElementById('loading')?.style.display==='none',null,{timeout:180000});await page.waitForTimeout(4200);
}
async function shot(n){await page.screenshot({path:path.join(out,`${n}.png`)});}
async function click(x,y){await page.mouse.click(x,y+64);await page.waitForTimeout(450);}
async function lock(){await page.waitForTimeout(300);if(!await page.evaluate(()=>!!document.pointerLockElement))await page.keyboard.press('ArrowLeft');await page.waitForFunction(()=>document.pointerLockElement?.id==='unity-canvas',null,{timeout:8000});}
async function file(write){return page.evaluate(async write=>{
 const dbs=await indexedDB.databases();let found;
 for(const info of dbs){if(!info.name.includes('idbfs'))continue;const db=await new Promise((resolve,reject)=>{const r=indexedDB.open(info.name);r.onsuccess=()=>resolve(r.result);r.onerror=()=>reject(r.error);});
  if(db.objectStoreNames.contains('FILE_DATA'))await new Promise((resolve,reject)=>{const tx=db.transaction('FILE_DATA',write?'readwrite':'readonly'),store=tx.objectStore('FILE_DATA'),cursor=store.openCursor();cursor.onsuccess=()=>{const row=cursor.result;if(!row)return;if(String(row.key).endsWith('/CubeFrontier/world-0.cfe')){found=Array.from(row.value.contents);if(write){const value=row.value;value.contents=new Uint8Array(write);value.timestamp=new Date();row.update(value);}}row.continue();};tx.oncomplete=resolve;tx.onerror=()=>reject(tx.error);});db.close();
 }return found;
},write);}
async function save(){await page.keyboard.press('Escape');await page.waitForTimeout(400);await click(640,408);await page.waitForTimeout(1900);const bytes=Buffer.from(await file());
 return readWorldSave(bytes);}
async function resume(){await click(640,344);await lock();}
try{
 await open();await shot('01-menu');await click(300,418);await page.waitForTimeout(16000);await lock();await shot('02-starter-settlement');
 await page.keyboard.press('e');await page.waitForTimeout(500);await shot('03-block-library');
 await click(260,276);await page.keyboard.type('stone bricks',{delay:70});await page.waitForTimeout(400);
 assert.equal(await page.evaluate(()=>!!document.pointerLockElement),false,'Typing e in material search must keep the catalogue open');await shot('03-search-results');
 await click(260,276);await page.keyboard.press('Control+a');await page.keyboard.press('Backspace');
 await click(474,324);await click(120,390);await shot('04-building-materials');
 await page.keyboard.press('e');await lock();let initial=await save();
 assert.equal(initial.inventory[0][0],14,'Choosing Oak Planks replaces selected hotbar slot');
 assert.ok(initial.edits.some(e=>e[3]===26)&&initial.edits.some(e=>e[3]===25),'New world contains saved glass windows and red roofs');
 checks.push('Starter settlement saved as editable blocks; catalogue replaces selected hotbar material');
 await resume();await page.keyboard.press('f');await page.keyboard.down('Space');await page.waitForTimeout(1200);await page.keyboard.up('Space');await shot('05-village-overview');
 await page.keyboard.press('b');await page.waitForTimeout(500);await shot('06-house-catalogue');
 // Install the prepared level site in this disposable profile, then use only real game input.
 await context.close();context=null;await open();await file(Array.from(fs.readFileSync(path.join(root,'Tests/Fixtures/platform-v1.cfe'))));await context.close();context=null;
 await open();await click(1120,628);await page.waitForTimeout(14000);await lock();await shot('07-platform');
 const baseline=await save();assert.equal(baseline.generatorVersion,1,'Original save keeps its terrain generator');fs.writeFileSync(path.join(out,'baseline.json'),JSON.stringify(baseline));await resume();
 await page.keyboard.press('b');await page.waitForTimeout(300);await click(210,635);await lock();await page.waitForTimeout(800);await shot('08-house-preview');
 const preview=await save();assert.deepEqual(preview.edits,baseline.edits,'Selecting a blueprint must only preview it, without placing any blocks');await resume();
 await page.keyboard.press('r');await page.waitForTimeout(550);await shot('09-rotated-preview');
 await page.mouse.down();await page.waitForTimeout(60);await page.mouse.up();await page.waitForTimeout(4500);await shot('10-placed-house');
 const built=await save();fs.writeFileSync(path.join(out,'built.json'),JSON.stringify(built));assert.ok(built.edits.filter(e=>e[3]===23).length>30,'Real blueprint click writes plaster walls');assert.ok(built.edits.some(e=>e[3]===26),'House has saved windows');
 const roof=built.edits.filter(e=>e[3]===25);
 assert.equal(Math.max(...roof.map(e=>e[0]))-Math.min(...roof.map(e=>e[0]))+1,11,'Rotated cottage spans eleven blocks along X');
 assert.equal(Math.max(...roof.map(e=>e[2]))-Math.min(...roof.map(e=>e[2]))+1,9,'Rotated cottage spans nine blocks along Z');
 checks.push('Menu selection only previews; rotation and a separate click create an editable house in actual saved world state');
 await resume();await page.keyboard.press('z');await page.waitForTimeout(3000);const undone=await save();assert.deepEqual(undone.edits,baseline.edits,'Undo restores original platform exactly');checks.push('Undo restores the original construction site');
 await resume();await page.keyboard.press('b');await page.waitForTimeout(350);await click(210,635);await lock();await page.waitForTimeout(600);await page.mouse.down();await page.waitForTimeout(60);await page.mouse.up();await page.waitForTimeout(3500);
 const persisted=await save();await context.close();context=null;await open();await click(1120,628);await page.waitForTimeout(14000);await lock();await shot('11-reloaded-house');
 const restored=await save();assert.deepEqual(restored.edits,persisted.edits);checks.push('Placed building persists unchanged after closing and reopening Chrome');assert.equal(errors.length,0,errors.join('\n'));
 fs.writeFileSync(path.join(out,'result.json'),JSON.stringify({passed:true,profile,checks,errors},null,2));console.log(JSON.stringify({passed:true,checks},null,2));
}catch(e){await shot('failure').catch(()=>{});fs.writeFileSync(path.join(out,'result.json'),JSON.stringify({passed:false,error:e.stack,checks,errors},null,2));throw e;}
finally{fs.writeFileSync(path.join(out,'console.log'),logs.join('\n'));if(context)await context.close();}
