import assert from 'node:assert/strict';
import {mkdir, writeFile} from 'node:fs/promises';
const root=process.env.APP_URL||'http://127.0.0.1:5298';
assert.ok(['localhost','127.0.0.1'].includes(new URL(root).hostname),'Use the ephemeral DemoMode fixture');
const output=process.env.CHECK_OUTPUT_DIR||'SoftSync-main/docs/screenshots/settings';
await mkdir(output,{recursive:true});
const version=await(await fetch((process.env.CDP_URL||'http://127.0.0.1:9498')+'/json/version')).json();
const ws=new WebSocket(version.webSocketDebuggerUrl);
await new Promise((resolve,reject)=>{ws.onopen=resolve;ws.onerror=reject;});
let id=0,sessionId;
const pending=new Map(),errors=[],results=[];
ws.onmessage=e=>{const m=JSON.parse(e.data);if(m.id){const p=pending.get(m.id);pending.delete(m.id);m.error?p.reject(Error(JSON.stringify(m.error))):p.resolve(m.result);}if(m.method==='Runtime.exceptionThrown')errors.push(m.params.exceptionDetails);};
const call=(method,params={},sid)=>new Promise((resolve,reject)=>{const n=++id;pending.set(n,{resolve,reject});ws.send(JSON.stringify({id:n,method,params,sessionId:sid}));});
const send=(m,p)=>call(m,p,sessionId);
const evaluate=async expression=>{const r=await send('Runtime.evaluate',{expression,returnByValue:true,awaitPromise:true});assert.ok(!r.exceptionDetails,JSON.stringify(r.exceptionDetails));return r.result.value;};
const pause=ms=>new Promise(r=>setTimeout(r,ms));
const wait=async expression=>{for(let n=0;n<160;n++){if(await evaluate(expression))return;await pause(100);}throw Error('Timeout '+expression+' '+JSON.stringify(await evaluate('({url:location.href,text:document.body.innerText.slice(-600)})')));};
const nav=async path=>{await send('Page.navigate',{url:root+path});await wait("document.readyState==='complete'");};
const fill=async(id,value)=>evaluate(`(()=>{const el=document.getElementById(${JSON.stringify(id)});el.value=${JSON.stringify(value)};el.dispatchEvent(new Event('input',{bubbles:true}));})()`);
const section=async key=>{await evaluate(`document.querySelector('.st-nav-item[data-section="${key}"]').click()`);await wait(`document.querySelector('.st-nav-item[data-section="${key}"]').getAttribute('aria-current')==='page'`);await pause(100);};
const screenshot=async name=>{await evaluate('window.scrollTo(0,0)');await pause(100);const {cssContentSize}=await send('Page.getLayoutMetrics');const result=await send('Page.captureScreenshot',{format:'png',captureBeyondViewport:true,clip:{x:0,y:0,width:cssContentSize.width,height:cssContentSize.height,scale:1}});await writeFile(output+'/'+name+'.png',Buffer.from(result.data,'base64'));};
const {browserContextId}=await call('Target.createBrowserContext');
try{
 const {targetId}=await call('Target.createTarget',{url:'about:blank',browserContextId});({sessionId}=await call('Target.attachToTarget',{targetId,flatten:true}));await send('Page.enable');await send('Runtime.enable');
 await send('Emulation.setDeviceMetricsOverride',{width:1440,height:1000,deviceScaleFactor:1,mobile:false});
 await nav('/Account/Login');await wait("!!document.querySelector('#login-identifier')");
 await evaluate("ssLang.set('vi');document.querySelector('#login-identifier').value='demo@softsync.local';document.querySelector('#login-password').value='Demo@12345';document.querySelector('.login-submit').click()");
 await wait("!location.pathname.includes('/Account/Login')");
 await nav('/settings');await wait("!!document.querySelector('.st-fieldset:not([disabled])')");
 assert.equal(await evaluate('document.querySelectorAll(".st-nav-item").length'),7);
 assert.equal(await evaluate('document.querySelectorAll("#st-panel .st-card").length'),2,'Only the active section renders');
 assert.equal(await evaluate('document.querySelector(".st-save-bar button").disabled'),true);
 const displayName=await evaluate('document.querySelector("#displayName").value==="Minh Anh"?"Ngọc Anh":"Minh Anh"');
 await fill('fullName','Nguyễn '+displayName);await fill('displayName',displayName);await wait('!document.querySelector(".st-save-bar button").disabled');
 await evaluate('document.querySelector(".st-save-bar button").click()');await wait('!!document.querySelector(".st-feedback-success") && document.querySelector(".st-save-bar button").disabled');
 await nav('/settings');await wait("!!document.querySelector('.st-fieldset:not([disabled])')");
 assert.equal(await evaluate('document.querySelector("#displayName").value'),displayName);
 results.push({profileSavedAndReloaded:true});
 await section('Learning');const minutes=await evaluate('document.querySelector("#dailyMinutes").value==="30"?"45":"30"');await evaluate(`document.querySelectorAll(".st-goal")[3].click();document.querySelector("#dailyMinutes").value="${minutes}";document.querySelector("#dailyMinutes").dispatchEvent(new Event("change",{bubbles:true}));document.querySelector("#studyDays").value="5";document.querySelector("#studyDays").dispatchEvent(new Event("change",{bubbles:true}))`);
 await wait('!document.querySelector(".st-save-bar button").disabled');await evaluate('document.querySelector(".st-save-bar button").click()');await wait('!!document.querySelector(".st-feedback-success")');
 await nav('/settings?section=Learning');await wait("!!document.querySelector('.st-fieldset:not([disabled])')");assert.equal(await evaluate('document.querySelector("#dailyMinutes").value'),minutes);assert.equal(await evaluate('document.querySelector("#studyDays").value'),'5');assert.equal(await evaluate('document.querySelectorAll(".st-goal")[3].getAttribute("aria-pressed")'),'true');results.push({learningSavedAndReloaded:true});
 await section('Security');await fill('settings-new-password','Example123');await fill('settings-confirm-password','Mismatch123');await wait('!!document.querySelector(".st-error")');assert.equal(await evaluate('document.querySelector(".st-password-form button").disabled'),true);
 await fill('settings-confirm-password','Example123');await wait('!document.querySelector(".st-password-form button").disabled');
 await evaluate('document.querySelector(".st-check-label input").click()');await wait('document.querySelector("#settings-new-password").type==="text"');
 await evaluate('document.querySelector(".st-danger").open=true;document.querySelector(".st-danger .st-button-danger").focus();document.querySelector(".st-danger .st-button-danger").click()');await wait('!!document.querySelector(".st-dialog") && document.activeElement.classList.contains("st-dialog")');assert.equal(await evaluate('document.querySelector(".st-dialog button[type=submit]").disabled'),true);
 await send('Input.dispatchKeyEvent',{type:'keyDown',key:'Tab',code:'Tab',windowsVirtualKeyCode:9});assert.equal(await evaluate('document.activeElement.className'),'st-dialog-close');
 await send('Input.dispatchKeyEvent',{type:'keyDown',key:'Escape',code:'Escape',windowsVirtualKeyCode:27});await wait('!document.querySelector(".st-dialog") && document.activeElement.classList.contains("st-button-danger")');results.push({passwordValidation:true,dialogKeyboardAndFocus:true});
 for(const width of [320,390,768,1440]){
  await send('Emulation.setDeviceMetricsOverride',{width,height:900,deviceScaleFactor:1,mobile:width<901});
  for(const key of ['Account','Learning','Security','Notifications','Appearance','Privacy','Danger']){
   await section(key);assert.equal(await evaluate('document.documentElement.scrollWidth>innerWidth'),false,`Overflow ${width} ${key}`);
   if(width<901)assert.equal(await evaluate('(()=>{const nav=document.querySelector(".st-navigation").getBoundingClientRect(),active=document.querySelector(".st-nav-item.is-active").getBoundingClientRect();return active.left>=nav.left-1&&active.right<=nav.right+1})()'),true,`Active section visible ${width} ${key}`);
  }
  results.push({width,allSectionsNoOverflow:true});
 }
 await section('Account');await screenshot('desktop-profile-light');
 await section('Appearance');await screenshot('desktop-appearance-light');
 await evaluate('document.querySelectorAll(".st-theme")[1].click()');await wait("document.documentElement.dataset.theme==='dark'");await screenshot('desktop-appearance-dark');
 await evaluate('document.querySelectorAll(".st-segment")[1].querySelectorAll("button")[2].click();document.querySelector("#highContrast").click();document.querySelector("#reduceMotion").click()');await wait("document.documentElement.dataset.fontSize==='large' && document.documentElement.dataset.highContrast==='1' && document.documentElement.dataset.reduceMotion==='1'");
 assert.equal(await evaluate('document.documentElement.scrollWidth>innerWidth'),false,'Large text with contrast');
 await evaluate('document.querySelectorAll(".st-segment")[1].querySelectorAll("button")[1].click();document.querySelector("#highContrast").click()');
 await send('Emulation.setDeviceMetricsOverride',{width:390,height:844,deviceScaleFactor:1,mobile:true});await screenshot('mobile-appearance-dark');
 await evaluate('document.querySelectorAll(".st-theme")[0].click()');await wait("document.documentElement.dataset.theme==='light'");await section('Account');await screenshot('mobile-profile-light');
 await section('Learning');await screenshot('mobile-learning-light');await section('Security');await screenshot('mobile-security-light');
 await section('Appearance');await evaluate('document.querySelector(".st-segment button:nth-child(2)").click()');await wait('document.querySelector("#st-panel-heading").textContent==="Appearance"');assert.equal(await evaluate('document.querySelector(".st-nav-item.is-active strong").textContent'),'Appearance');results.push({themes:true,largeText:true,contrast:true,reduceMotion:true,english:true});
 assert.deepEqual(errors,[],'No browser runtime exceptions');await writeFile(output+'/checks.json',JSON.stringify(results,null,2));console.log(JSON.stringify(results));
}catch(error){await screenshot('failure');throw error;}finally{await call('Target.disposeBrowserContext',{browserContextId});ws.close();}
