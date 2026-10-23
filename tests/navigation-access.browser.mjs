import assert from 'node:assert/strict';
import {writeFile} from 'node:fs/promises';
// Run against the ephemeral SoftSync:DemoMode fixture, with a local CDP browser.
const root=process.env.APP_URL || 'http://127.0.0.1:5208', out=process.env.CHECK_OUTPUT_DIR || '.';
assert.ok(['localhost','127.0.0.1'].includes(new URL(root).hostname), 'Use the local DemoMode fixture');
const version=await(await fetch((process.env.CDP_URL || 'http://127.0.0.1:9478')+'/json/version')).json();
const ws=new WebSocket(version.webSocketDebuggerUrl);
await new Promise((r,j)=>{ws.onopen=r;ws.onerror=j});
let id=0,sessionId; const pending=new Map(),errors=[],results=[];
ws.onmessage=e=>{const m=JSON.parse(e.data);if(m.id){const p=pending.get(m.id);pending.delete(m.id);m.error?p.reject(Error(JSON.stringify(m.error))):p.resolve(m.result)}if(m.method==='Runtime.exceptionThrown')errors.push(m.params.exceptionDetails)};
const call=(method,params={},session)=>new Promise((resolve,reject)=>{const n=++id;pending.set(n,{resolve,reject});ws.send(JSON.stringify({id:n,method,params,sessionId:session}))});
const send=(m,p)=>call(m,p,sessionId);
const evaluate=async expression=>{const r=await send('Runtime.evaluate',{expression,returnByValue:true,awaitPromise:true});if(r.exceptionDetails)throw Error(JSON.stringify(r.exceptionDetails));return r.result.value};
const pause=ms=>new Promise(r=>setTimeout(r,ms));
const wait=async expr=>{for(let n=0;n<200;n++){if(await evaluate(expr))return;await pause(100)}throw Error('Timeout '+expr+' '+await evaluate('document.body.innerText.slice(0,1200)'))};
const nav=async path=>{await send('Page.navigate',{url:root+path});await wait("document.readyState==='complete'");await pause(600)};
const dock=()=>evaluate("document.querySelectorAll('nav.ss-mobile-bottom-nav').length");
const login=async()=>{await nav('/Account/Login');await wait("!!document.querySelector('#login-identifier')");await evaluate("document.querySelector('#login-identifier').value='demo@softsync.local';document.querySelector('#login-password').value='Demo@12345';document.querySelector('.login-submit').click()");};
const {browserContextId}=await call('Target.createBrowserContext');
try{
 const {targetId}=await call('Target.createTarget',{url:'about:blank',browserContextId});({sessionId}=await call('Target.attachToTarget',{targetId,flatten:true}));
 await send('Page.enable');await send('Runtime.enable');
 for(const width of [320,390,430]){
  await send('Emulation.setDeviceMetricsOverride',{width,height:844,deviceScaleFactor:1,mobile:true});
  await nav('/mentors');assert.equal(await dock(),0);
  assert.equal(await evaluate("document.querySelectorAll('.ss-unified-public-header').length"),1);
  const overflow=await evaluate('document.documentElement.scrollWidth>innerWidth');assert.equal(overflow,false,'guest header overflow '+width);
  await evaluate("document.querySelector('.ss-public-drawer-trigger').click()");
  assert.equal(await evaluate("document.querySelector('#ss-public-drawer').open"),true);
  assert.equal(await evaluate('document.documentElement.scrollWidth>innerWidth'),false);
  await evaluate("document.querySelector('#ss-public-drawer').close()");results.push({guestWidth:width,header:true,drawer:true,noDock:true});
 }
 await evaluate("document.querySelector('.ss-header-language').open=true;document.querySelector('.ss-header-language button:last-child').click()");
 await wait("document.documentElement.lang==='en' && document.querySelector('.ss-language-flag').getAttribute('src').endsWith('/en.svg')");
 const theme=await evaluate("document.documentElement.dataset.theme");
 await evaluate("document.querySelector('.ss-public-actions button.ss-shell-control').click()");
 await wait("document.documentElement.dataset.theme!=="+JSON.stringify(theme));
 results.push({language:true,theme:true});
 await login();await wait("location.pathname.startsWith('/assessment/') && !!document.querySelector('.assessment-option')");
 assert.equal(await dock(),0);results.push({unassessedLogin:'assessment',noDock:true});
 for(const path of ['/mentors','/resources','/community','/game','/profile','/settings']){
  await nav(path);await wait("!!document.querySelector('.ss-header-user')");assert.equal(await dock(),0);assert.equal(await evaluate("document.querySelectorAll('.ss-unified-public-header').length"),0);
 }
 await nav('/roadmap');await wait("location.pathname.startsWith('/assessment/') && !!document.querySelector('.assessment-option')");
 // Save a real draft, then refresh: the existing assessment must resume.
 await evaluate("document.querySelector('.assessment-option').click()");await wait("!!document.querySelector('.assessment-option.active')");
 await evaluate("document.querySelector('.assessment-page button.ss-btn-primary').click()");
 await wait("!!document.querySelector('.assessment-option') && !document.querySelector('.assessment-option.active')");
 const badge=await evaluate("document.querySelector('.assessment-page .badge').textContent");
 await send('Page.reload');await wait("!!document.querySelector('.assessment-option')");await pause(1000);
 assert.equal(await evaluate("document.querySelector('.assessment-page .badge').textContent"),badge);assert.equal(await dock(),0);
 results.push({draftRestored:true});
 for(let n=0;n<32;n++){
  if(await evaluate("location.pathname.startsWith('/assessment-result/')"))break;
  await wait("!!document.querySelector('.assessment-option')");
  const current=await evaluate("document.querySelector('.assessment-page .badge').textContent");
  await evaluate("document.querySelector('.assessment-option').click()");await wait("!!document.querySelector('.assessment-option.active')");
  await evaluate("document.querySelector('.assessment-page button.ss-btn-primary').click()");
  await wait("location.pathname.startsWith('/assessment-result/') || (document.querySelector('.assessment-page .badge')?.textContent!=="+JSON.stringify(current)+" && !!document.querySelector('.assessment-option'))");
 }
 await wait("location.pathname.startsWith('/assessment-result/')");assert.equal(await dock(),0);
 for(const path of ['/dashboard','/roadmap','/progress','/mentors','/resources','/community','/game']){
  await nav(path);await wait("!!document.querySelector('.ss-header-user')");assert.equal(await dock(),1,'completed dock '+path);
  if(['/mentors','/resources','/community','/game'].includes(path))assert.equal(await evaluate("document.querySelector('.ss-mobile-bottom-link.active')?.getAttribute('href')"),'/more');
  results.push({completedRoute:path,dock:1});
 }
 await send('Page.reload');await wait("!!document.querySelector('.ss-mobile-bottom-nav')");results.push({refreshRestores:true});
 await evaluate("document.querySelector('.ss-header-profile form').requestSubmit()");await wait("location.pathname==='/Account/Login'");assert.equal(await dock(),0);
 await nav('/mentors');assert.equal(await dock(),0);assert.equal(await evaluate("document.querySelectorAll('.ss-unified-public-header').length"),1);
 await nav('/Account/Register');
 await evaluate(`document.querySelector('#register-fullname').value='Navigation Tester';document.querySelector('#register-email').value='navigation-${Date.now()}@example.test';document.querySelector('#register-password').value='Test@12345';document.querySelector('#register-confirmpassword').value='Test@12345';document.querySelector('input[type=checkbox]').checked=true;document.querySelector('#register-fullname').form.requestSubmit()`);
 await wait("location.pathname==='/profile-setup' && !!document.querySelector('.ss-header-user')");assert.equal(await dock(),0);
 await nav('/dashboard');await wait("location.pathname==='/profile-setup'");assert.equal(await dock(),0);
 results.push({newAccountRequiresProfile:true,noRedirectLoop:true});
 await evaluate("document.querySelector('.ss-header-profile form').requestSubmit()");await wait("location.pathname==='/Account/Login'");
 await login();await wait("location.pathname==='/dashboard' && !!document.querySelector('.ss-mobile-bottom-nav')");results.push({logoutClears:true,reloginSkipsAssessment:true});
 assert.equal(errors.length,0,JSON.stringify(errors));
 await writeFile(out+'/navigation-access-browser-results.json',JSON.stringify({passed:true,results},null,2));console.log(JSON.stringify({passed:true,results}));
}catch(e){await writeFile(out+'/navigation-access-browser-failure.txt',String(e)+JSON.stringify({errors,debug:await evaluate('({url:location.href,blazor:!!window.Blazor,started:window.ssBlazorStarted,body:document.body.innerText.slice(0,500)})')}));const shot=await send('Page.captureScreenshot',{format:'png',captureBeyondViewport:true});await writeFile(out+'/navigation-access-browser-failure.png',Buffer.from(shot.data,'base64'));throw e}
finally{await call('Target.disposeBrowserContext',{browserContextId});ws.close()}
