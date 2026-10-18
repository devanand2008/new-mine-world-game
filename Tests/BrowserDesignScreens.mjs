import {createRequire} from 'node:module';
import {fileURLToPath} from 'node:url';
import path from 'node:path';
import fs from 'node:fs';
import assert from 'node:assert/strict';
const root=path.resolve(path.dirname(fileURLToPath(import.meta.url)),'..');
const require=createRequire(path.join(root,'TestResults/BrowserTools/package.json'));
const {chromium}=require('playwright');
const out=path.join(root,'TestResults/BrowserDesignScreens');fs.mkdirSync(out,{recursive:true});
const browser=await chromium.launch({channel:'chrome',headless:true});
const context=await browser.newContext({viewport:{width:1280,height:908}});
const page=await context.newPage(),errors=[];
page.on('pageerror',e=>errors.push(e.message));
page.on('console',m=>{if(m.type()==='error'&&!m.text().includes('404'))errors.push(m.text());});
async function shot(name){await page.waitForTimeout(650);await page.screenshot({path:path.join(out,name+'.png')});}
async function click(x,y){await page.mouse.click(x,y+64);await page.waitForTimeout(300);}
try{
 await page.goto('http://127.0.0.1:8080');
 await page.waitForFunction(()=>document.getElementById('loading')?.style.display==='none',null,{timeout:180000});
 await page.waitForTimeout(1500);await shot('01-home');
 await click(130,761);await shot('02-settings');await click(470,620);
 await click(300,681);await shot('03-explorers');await click(460,235);await shot('04-creatures');await click(180,775);
 await page.setViewportSize({width:1440,height:900});await shot('05-wide-home');
 await page.setViewportSize({width:1024,height:768});await shot('06-small-home');
 await page.setViewportSize({width:1280,height:908});
 await click(300,418);await page.waitForTimeout(16000);await page.keyboard.press('ArrowLeft');
 await page.waitForFunction(()=>document.pointerLockElement?.id==='unity-canvas');
 await shot('07-creative');
 await page.keyboard.press('g');await page.waitForTimeout(6500);await shot('07-fly-through');await page.keyboard.press('g');await page.waitForTimeout(6500);
 await page.keyboard.press('h');await shot('08-controls');await page.keyboard.press('h');
 await page.keyboard.press('e');await shot('09-materials');await page.keyboard.press('e');await page.waitForTimeout(450);await page.keyboard.press('b');await shot('10-blueprints');
 await page.keyboard.press('Escape');await page.waitForTimeout(450);await page.keyboard.press('Escape');await shot('11-pause');
 await click(640,408);await page.waitForTimeout(1800);await click(640,535);await page.waitForTimeout(2000);
 await click(300,418);await shot('12-replace-confirmation');await click(790,470);await shot('13-kept-world');
 assert.equal(errors.length,0,errors.join('\n'));
 fs.writeFileSync(path.join(out,'result.json'),JSON.stringify({passed:true,errors,screenshots:14,note:'Screenshots need visual review. Functional building and persistence checks live in BrowserBuildings.mjs and BrowserFlight.mjs.'},null,2));
 console.log('Design navigation completed at three desktop sizes without browser/runtime errors.');
}finally{await browser.close();}
