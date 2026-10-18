// Inspects the actual compiled player; isolated browser profile, no user save changes.
import { createRequire } from 'node:module';
import { fileURLToPath } from 'node:url';
import fs from 'node:fs';
import path from 'node:path';
import assert from 'node:assert/strict';
const root=path.resolve(path.dirname(fileURLToPath(import.meta.url)),'..');
const require=createRequire(path.join(root,'TestResults/BrowserTools/package.json'));
const {chromium}=require('playwright');
const out=path.join(root,'TestResults/BrowserGraphics');fs.mkdirSync(out,{recursive:true});
const profile=path.join(out,`profile-${Date.now()}`);
const errors=[],logs=[],checks=[];
let context,page;
async function open(){
 context=await chromium.launchPersistentContext(profile,{channel:'chrome',headless:true,viewport:{width:1280,height:908}});
 const initial=context.pages();page=await context.newPage();for(const p of initial)await p.close();
 page.on('pageerror',e=>errors.push(e.message));page.on('console',m=>{logs.push(`${m.type()}: ${m.text()}`);if(m.type()==='error'&&!m.text().includes('404'))errors.push(m.text());});
 page.on('response',r=>{if(!r.ok()&&!r.url().endsWith('favicon.ico'))errors.push(`${r.status()} ${r.url()}`);});
 await page.goto('http://127.0.0.1:8080');await page.waitForFunction(()=>document.getElementById('loading')?.style.display==='none',null,{timeout:180000});await page.waitForTimeout(4200);
}
async function click(x,y){await page.mouse.click(x,y+64);await page.waitForTimeout(450);}
async function shot(name){await page.screenshot({path:path.join(out,`${name}.png`)});}
try{
 await open();await shot('00-menu');await click(300,681);
 await click(550,678); // Stop automatic rotation for consistent turnarounds.
 for(let i=0;i<13;i++){
  await click(160+(i%2)*266,299+Math.floor(i/2)*51);await shot(`explorer-${String(i).padStart(2,'0')}`);
 }
 checks.push('Opened and captured all 13 selectable explorer appearances');
 await click(180,678); // Wear Eclipse Sentinel.
 await click(460,235);
 for(let i=0;i<9;i++){
  await click(160+(i%2)*266,299+Math.floor(i/2)*51);await shot(`creature-${String(i).padStart(2,'0')}`);
 }
 checks.push('Opened and captured all 9 creature designs');
 await click(180,775);await shot('01-selected-menu');
 await context.close();context=null;await open();await shot('02-selection-after-restart');
 await click(300,418);await page.waitForTimeout(16000);await page.keyboard.press('ArrowLeft');
 await page.waitForFunction(()=>document.pointerLockElement?.id==='unity-canvas',null,{timeout:8000});
 await shot('03-map-ground');
 await page.keyboard.press('F5');await page.waitForTimeout(700);await shot('04-selected-character-in-world');
 await page.keyboard.press('f');await page.keyboard.down('Space');await page.waitForTimeout(1100);await page.keyboard.up('Space');
 await page.mouse.move(640,600,{steps:10});await page.waitForTimeout(1500);await shot('05-map-overview');
 await page.keyboard.press('Escape');await page.waitForTimeout(350);await shot('06-pause');
 checks.push('Creative mode, third-person selected appearance, flight, map overview and pause exercised');
 assert.equal(errors.length,0,errors.join('\n'));
 checks.push('No browser exceptions, console errors or failed resource responses');
 console.log(JSON.stringify({passed:true,profile,checks},null,2));
 fs.writeFileSync(path.join(out,'result.json'),JSON.stringify({passed:true,profile,checks},null,2));
}catch(e){await shot('failure').catch(()=>{});fs.writeFileSync(path.join(out,'result.json'),JSON.stringify({passed:false,error:e.stack,errors,checks},null,2));throw e;}
finally{fs.writeFileSync(path.join(out,'console.log'),logs.join('\n'));if(context)await context.close();}
