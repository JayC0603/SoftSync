import { mkdir, writeFile } from 'node:fs/promises';
const tabs = await (await fetch('http://127.0.0.1:9333/json/list')).json();
const tab = tabs.find(x => x.type === 'page');
if (!tab) throw new Error('No review browser page');
const ws = new WebSocket(tab.webSocketDebuggerUrl);
await new Promise((resolve,reject) => { ws.onopen=resolve;ws.onerror=reject; });
let next=0; const pending=new Map();
ws.onmessage=e=>{const msg=JSON.parse(e.data);if(msg.id){const p=pending.get(msg.id);pending.delete(msg.id);msg.error?p.reject(new Error(JSON.stringify(msg.error))):p.resolve(msg.result);}};
const send=(method,params={})=>new Promise((resolve,reject)=>{const id=++next;pending.set(id,{resolve,reject});ws.send(JSON.stringify({id,method,params}));});
const pause=ms=>new Promise(r=>setTimeout(r,ms));
const evaluate=async(expression)=>{const r=await send('Runtime.evaluate',{expression,returnByValue:true,awaitPromise:true});if(r.exceptionDetails)throw new Error(JSON.stringify(r.exceptionDetails));return r.result?.value;};
const ready=async()=>{for(let i=0;i<150;i++){try{if(await evaluate("!!document.querySelector('.learn-v3')"))return;}catch{}await pause(100);}throw new Error('Learning page did not render');};
await send('Page.enable');await send('Runtime.enable');
const mode=process.argv[2]??'inspect', width=Number(process.argv[3]??390);
await send('Emulation.setDeviceMetricsOverride',{width,height:844,deviceScaleFactor:1,mobile:true});
try {
if(mode==='login'){
 await send('Page.navigate',{url:'http://localhost:5104/Account/Login'});await pause(1800);
 console.log(await evaluate(`(()=>{const form=document.querySelector('form');const email=form?.querySelector('#login-identifier');const password=form?.querySelector('input[type=password]');if(!email||!password)return document.body.innerText;email.value='demo@softsync.local';password.value='Demo@12345';form.requestSubmit();return 'submitted';})()`));await pause(1800);
} else if(mode==='navigate'){
 await send('Page.navigate',{url:'http://localhost:5104'+(process.argv[4]??'/roadmap')});await pause(2200);
} else if(mode==='assess'){
 for(let i=0;i<60;i++){
  const result=await evaluate(`(()=>{const option=document.querySelector('.assessment-option');if(option){option.click();return 'option';}return location.pathname;})()`);
  if(result!=='option'){console.log(result);break;}
  await pause(100);
  await evaluate(`document.querySelector('.assessment-page .ss-btn-primary:not(:disabled)')?.click()`);
  await pause(180);
 }
 await pause(2500);
} else if(mode==='click'){
 console.log(await evaluate(`(()=>{const el=document.querySelector(${JSON.stringify(process.argv[4])});if(!el)return 'missing';el.click();return 'clicked';})()`));await pause(1300);
} else if(mode==='verify'){
 const results=[];
 for(const route of ['/roadmap','/roadmap?course=time']){
  await send('Page.navigate',{url:'http://localhost:5104'+route});await ready();await pause(1200);
  for(const reviewWidth of [360,390,430,1280]){
   await send('Emulation.setDeviceMetricsOverride',{width:reviewWidth,height:844,deviceScaleFactor:1,mobile:reviewWidth<901});await pause(200);
   const result=await evaluate(`({width:innerWidth,scrollWidth:document.documentElement.scrollWidth,heading:document.querySelector('.lm-detail-heading h1')?.textContent,desktopVisible:document.querySelector('.learn-desktop')?.getBoundingClientRect().height>0,brokenImages:[...document.querySelectorAll('.learn-v3 img')].filter(x=>x.complete&&x.naturalWidth===0).map(x=>x.src)})`);
   if(result.scrollWidth>result.width)throw new Error('Horizontal overflow: '+JSON.stringify(result));
   if(result.brokenImages.length)throw new Error('Broken images: '+JSON.stringify(result));
   if(route.includes('course')&&!result.heading?.includes('Quản lý thời gian'))throw new Error('Course query was not applied');
   if(reviewWidth===1280&&!result.desktopVisible)throw new Error('Desktop journey hidden');
   results.push({route,...result});
  }
 }
 await send('Emulation.setDeviceMetricsOverride',{width:390,height:844,deviceScaleFactor:1,mobile:true});
 for(const id of [1,2,0]){
  await evaluate(`document.querySelector('#lm-tab-${id}').click()`);await pause(300);
  const selected=await evaluate(`document.querySelector('#lm-tab-${id}').getAttribute('aria-selected')`);
  if(selected!=='true')throw new Error('Tab did not activate: '+id);
 }
 await send('Page.navigate',{url:'http://localhost:5104/roadmap'});await pause(1200);
 await evaluate("document.querySelector('.lm-featured .lm-primary').click()");await pause(1000);
 const lesson=await evaluate('location.pathname');
 if(!lesson.includes('/roadmap/lecture/'))throw new Error('Continue did not open lesson');
 console.log(JSON.stringify({checks:results,tabs:'passed',continueRoute:lesson}));
} else if(mode==='shot'){
 await pause(800);await evaluate('document.fonts.ready');
 const initial=await send('Page.getLayoutMetrics');
 await send('Emulation.setDeviceMetricsOverride',{width,height:Math.min(4000,Math.ceil(initial.cssContentSize.height)),deviceScaleFactor:1,mobile:true});
 await pause(150);
 const metrics=await send('Page.getLayoutMetrics');
 const shot=await send('Page.captureScreenshot',{format:'png',captureBeyondViewport:true,clip:{x:0,y:0,width,height:Math.min(4000,Math.ceil(metrics.cssContentSize.height)),scale:1}});
 await mkdir('docs/screenshots',{recursive:true});
 const path='docs/screenshots/'+(process.argv[4]??'learn-reference-'+width)+'.png';
 await writeFile(path,Buffer.from(shot.data,'base64'));console.log(path);
}
console.log(await evaluate(`JSON.stringify({url:location.href,width:innerWidth,scrollWidth:document.documentElement.scrollWidth,text:document.body.innerText.slice(0,6500),brokenImages:[...document.querySelectorAll('.learn-v3 img')].filter(x=>!x.complete||x.naturalWidth===0).map(x=>x.getAttribute('src')),tabs:[...document.querySelectorAll('.lm-detail-tabs button')].map(x=>({text:x.innerText,selected:x.getAttribute('aria-selected')}))})`));
} finally {ws.close();}
