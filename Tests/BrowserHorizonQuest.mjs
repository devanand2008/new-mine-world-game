import {createRequire} from 'node:module';
import {fileURLToPath} from 'node:url';
import {createHash} from 'node:crypto';
import fs from 'node:fs';import path from 'node:path';import assert from 'node:assert/strict';
import {readWorldSave} from './ReadWorldSave.mjs';
const root=path.resolve(path.dirname(fileURLToPath(import.meta.url)),'..');
const require=createRequire(path.join(root,'TestResults/BrowserTools/package.json'));
const {chromium}=require('playwright');
const out=path.join(root,'TestResults/BrowserHorizonQuest');fs.mkdirSync(out,{recursive:true});
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
const fixture=name=>fs.readFileSync(path.join(root,'Tests/Fixtures',name+'.cfe'));
const count=(save,id)=>save.inventory.filter(s=>s[0]===id).reduce((sum,s)=>sum+s[1],0);
try {
 await open();await click(300,509);await page.waitForTimeout(16000);await lock();
 await key('j');await shot('01-new-survival-journal');await key('j');await lock();const fresh=await save();
 assert.deepEqual(fresh.quest,[0,0,0,0],'Starter supplies and village must not grant quest progress');checks.push('Fresh Survival begins with an uncompleted quest');
 await install(fixture('quest-start-v4'));
 await key('t');await shot('02-merchant');await click(600,280);await key('Escape');await lock();const traded=await save();
 assert.equal(count(traded,8),8);assert.equal(count(traded,103),6);checks.push('Physical merchant T interaction exchanges four wood for two rations');await resume();
 await key('e');await click(400,341);await key('e');await lock();const crafted=await save();
 assert.equal(count(crafted,101),1);assert.equal(count(crafted,8),5);assert.equal(count(crafted,3),9);assert.equal(crafted.quest[3]&1,1);
 checks.push('Crafting a real axe consumes materials and records quest progress');assert.equal(crafted.character,4);await resume();
 await key('t');await click(600,558);await key('Escape');await lock();const rejected=await save();assert.equal(count(rejected,10),0);assert.equal(rejected.quest[3]&8,0);checks.push('Incomplete commission refuses its reward');
 await install(fixture('quest-mine-v4'));await page.mouse.down();await page.waitForTimeout(5500);await page.mouse.up();const mined=await save();
 assert.equal(mined.quest[0],1,'Mining a timber voxel updates the commission');assert.equal(count(mined,8),13);checks.push('Real mouse mining collects timber and advances the quest');
 await install(fixture('quest-ready-v4'));await key('t');await click(600,558);await click(600,558);await key('Escape');await lock();const claimed=await save();
 assert.equal(count(claimed,10),8);assert.equal(claimed.quest[3],15);checks.push('Completed-quest fixture awards eight crystals once, including repeated-click protection');await resume();
 await key('t');await click(600,411);await key('Escape');await lock();await key('6');await key('j');await click(950,607);await shot('03-equipment-and-completed-quest');await key('j');await lock();const equipped=await save();
 assert.equal(equipped.equipment[1][0],121);assert.equal(equipped.equipment[1][2],120);assert.equal(count(equipped,121),0);assert.equal(count(equipped,10),4);checks.push('Merchant armour purchase and equipment transfer preserve real item counts');
 const bytes=Buffer.from(await file());await install(bytes);await key('j');await shot('04-reloaded-journal');await key('j');await lock();const loaded=await save();
 assert.equal(loaded.character,4);assert.deepEqual(loaded.quest,[4,8,12,15]);assert.deepEqual(loaded.equipment,equipped.equipment);checks.push('Character, quest, reward claim, purchased armour and durability survive browser restart');
 const night=Buffer.from(fs.readFileSync(path.join(root,'Tests/Fixtures/flight-wall-v3.cfe')));
 night.writeFloatLE(.92,45);night.writeFloatLE(145,25);night.writeFloatLE(-60,29);
 createHash('sha256').update(night.subarray(0,-32)).digest().copy(night,night.length-32);
 await install(night);await key('g');await hold('Space',3500);await page.waitForTimeout(6500);await shot('05-moon-and-stars');
 assert.equal(errors.length,0,errors.join('\n'));
 fs.writeFileSync(path.join(out,'result.json'),JSON.stringify({passed:true,checks,errors},null,2));console.log(JSON.stringify({passed:true,checks},null,2));
} catch(e) {await shot('failure').catch(()=>{});fs.writeFileSync(path.join(out,'result.json'),JSON.stringify({passed:false,error:e.stack,checks,errors},null,2));throw e;}
finally {fs.writeFileSync(path.join(out,'console.log'),logs.join('\n'));if(context)await context.close();}
