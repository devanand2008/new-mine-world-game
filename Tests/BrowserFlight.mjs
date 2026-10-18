import {createRequire} from 'node:module';
import {fileURLToPath} from 'node:url';
import {createHash} from 'node:crypto';
import fs from 'node:fs';import path from 'node:path';import assert from 'node:assert/strict';
import {readWorldSave} from './ReadWorldSave.mjs';
const root=path.resolve(path.dirname(fileURLToPath(import.meta.url)),'..');
const require=createRequire(path.join(root,'TestResults/BrowserTools/package.json'));
const {chromium}=require('playwright');
const out=path.join(root,'TestResults/BrowserFlight');fs.mkdirSync(out,{recursive:true});
const profile=path.join(out,`profile-${Date.now()}`),errors=[],logs=[],checks=[];
let context,page;
async function open(){
 context=await chromium.launchPersistentContext(profile,{channel:'chrome',headless:true,viewport:{width:1280,height:908}});
 page=await context.newPage();for(const old of context.pages())if(old!==page)await old.close();
 page.on('pageerror',e=>errors.push(e.message));page.on('console',m=>{logs.push(m.text());if(m.type()==='error'&&!m.text().includes('404'))errors.push(m.text());});
 await page.goto('http://127.0.0.1:8080');await page.waitForFunction(()=>document.getElementById('loading')?.style.display==='none',null,{timeout:180000});await page.waitForTimeout(2000);
}
async function click(x,y){await page.mouse.click(x,y+64);await page.waitForTimeout(450);}
async function key(k){await page.keyboard.press(k,{delay:70});await page.waitForTimeout(300);}
async function hold(k,ms){await page.keyboard.down(k);await page.waitForTimeout(ms);await page.keyboard.up(k);await page.waitForTimeout(150);}
async function lock(){if(!await page.evaluate(()=>!!document.pointerLockElement))await key('ArrowLeft');await page.waitForFunction(()=>!!document.pointerLockElement,null,{timeout:12000});}
async function shot(name){await page.screenshot({path:path.join(out,name+'.png')});}
async function file(write){return page.evaluate(async write=>{
 let found;for(const info of await indexedDB.databases()){
  if(!info.name.includes('idbfs'))continue;
  const db=await new Promise((resolve,reject)=>{const r=indexedDB.open(info.name);r.onsuccess=()=>resolve(r.result);r.onerror=()=>reject(r.error);});
  if(db.objectStoreNames.contains('FILE_DATA'))await new Promise((resolve,reject)=>{
   const tx=db.transaction('FILE_DATA',write?'readwrite':'readonly'),r=tx.objectStore('FILE_DATA').openCursor();
   r.onsuccess=()=>{const row=r.result;if(!row)return;if(String(row.key).endsWith('/CubeFrontier/world-0.cfe')){found=Array.from(row.value.contents);if(write){const value=row.value;value.contents=new Uint8Array(write);value.timestamp=new Date();row.update(value);}}row.continue();};tx.oncomplete=resolve;tx.onerror=()=>reject(tx.error);
  });db.close();
 }return found;
},write);}
async function save(){await key('Escape');await click(640,408);await page.waitForTimeout(2000);return readWorldSave(Buffer.from(await file()));}
async function resume(){await click(640,344);await lock();}
async function install(bytes){
 if(context){await context.close();context=null;}await open();await file(Array.from(bytes));await context.close();context=null;await open();await click(1120,628);await page.waitForTimeout(14000);await lock();
}
const fixture=fs.readFileSync(path.join(root,'Tests/Fixtures/flight-wall-v3.cfe'));
try{
 await open();await shot('01-classic-menu');await click(300,418);await page.waitForTimeout(16000);await lock();await save();
 await install(fixture);await shot('02-solid-wall');
 await key('f');await hold('w',1000);const blocked=await save();
 assert.ok(blocked.z>3.5&&blocked.z<4.9,`Normal flight must stop before wall: z=${blocked.z}`);checks.push('Normal Creative flight collides with a three-block-thick wall');await resume();
 await key('g');await hold('w',140);await key('g');await key('f');await shot('03-inside-wall-exit-refused');
 const interior=await save(),safeBytes=Buffer.from(await file());
 assert.ok(!(interior.z>=5&&interior.z<8&&interior.y>=45&&interior.y<50),'Saving while inside a wall must use an unobstructed return point');checks.push('Saving inside solid blocks records a safe return position');await resume();
 await hold('w',650);await shot('04-through-wall');const through=await save();
 assert.ok(through.z>8.5,`Fly-through must cross the wall: z=${through.z}`);
 assert.ok(Math.abs(through.y-blocked.y)<.25,'G and F must refuse to restore collision inside the wall');
 assert.deepEqual(through.edits,blocked.edits,'Flying through the wall must not mine or change any blocks');
 checks.push('G crosses solid blocks; G/F cannot trap the player inside a wall; world blocks remain unchanged');await resume();
 await key('g');await hold('s',1300);const restored=await save();
 assert.ok(restored.z>8.1&&restored.z<9,`Restored collision must stop at the far wall: z=${restored.z}`);checks.push('G restores normal collision in open space');await resume();
 await key('f');await page.waitForTimeout(1000);const landed=await save();assert.ok(landed.y>=44.9&&landed.y<45.3,'F resumes normal grounded movement');checks.push('Normal walking and floor collision work after leaving fly-through');
 await install(safeBytes);await shot('05-safe-reload');const reloaded=await save();
 assert.ok(!(reloaded.z>=5&&reloaded.z<8&&reloaded.y>=45&&reloaded.y<50),'Reload must not place the player inside the wall');checks.push('Save made inside a wall reloads safely');
 const survival=Buffer.from(fixture);survival[12]=0;createHash('sha256').update(survival.subarray(0,-32)).digest().copy(survival,survival.length-32);
 await install(survival);await key('g');await key('f');await hold('w',1800);const normal=await save();
 assert.ok(normal.z>3.5&&normal.z<4.9,'Survival ignores G/F and retains wall collision');assert.ok(normal.y<45.3,'Survival cannot enable flight');checks.push('Fly-through is Creative-only; Survival remains collidable');
 assert.equal(errors.length,0,errors.join('\n'));
 fs.writeFileSync(path.join(out,'result.json'),JSON.stringify({passed:true,checks,errors,positions:{blocked,through,restored,landed}},null,2));console.log(JSON.stringify({passed:true,checks},null,2));
}catch(e){await shot('failure').catch(()=>{});fs.writeFileSync(path.join(out,'result.json'),JSON.stringify({passed:false,error:e.stack,checks,errors},null,2));throw e;}
finally{fs.writeFileSync(path.join(out,'console.log'),logs.join('\n'));if(context)await context.close();}
