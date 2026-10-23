// APP_URL points to the local demo profile; uses an existing Chromium CDP browser.
import assert from 'node:assert/strict';
const app=process.env.APP_URL || 'http://127.0.0.1:5198';
const version=await(await fetch('http://127.0.0.1:9466/json/version')).json();
const ws=new WebSocket(version.webSocketDebuggerUrl);
await new Promise((r,j)=>{ws.onopen=r;ws.onerror=j;});
let id=0, sessionId;
const pending=new Map(), paused=[], requests=[], errors=[];
ws.onmessage=event=>{
 const m=JSON.parse(event.data);
 if(m.id){const p=pending.get(m.id);pending.delete(m.id);m.error?p.reject(Error(JSON.stringify(m.error))):p.resolve(m.result);}
 if(m.method==='Fetch.requestPaused')paused.push(m.params.requestId);
 if(m.method==='Network.requestWillBeSent')requests.push(m.params.request.url);
 if(m.method==='Runtime.exceptionThrown')errors.push(m.params.exceptionDetails);
};
const call=(method,params={},session)=>new Promise((resolve,reject)=>{const n=++id;pending.set(n,{resolve,reject});ws.send(JSON.stringify({id:n,method,params,sessionId:session}));});
const send=(m,p)=>call(m,p,sessionId);
const evaluate=async expression=>{const r=await send('Runtime.evaluate',{expression,returnByValue:true,awaitPromise:true});if(r.exceptionDetails)throw Error(JSON.stringify(r.exceptionDetails));return r.result.value;};
const pause=ms=>new Promise(r=>setTimeout(r,ms));
const wait=async expression=>{for(let n=0;n<250;n++){if(await evaluate(expression))return;await pause(100);}throw Error('Timeout: '+expression+' '+JSON.stringify(await evaluate('document.body.innerText.slice(-1200)')));};
const {browserContextId}=await call('Target.createBrowserContext');
try{
 const {targetId}=await call('Target.createTarget',{url:'about:blank',browserContextId});
 ({sessionId}=await call('Target.attachToTarget',{targetId,flatten:true}));
 await send('Page.enable');await send('Runtime.enable');await send('Network.enable');
 await send('Emulation.setDeviceMetricsOverride',{width:390,height:844,deviceScaleFactor:1,mobile:true});
 await send('Page.navigate',{url:app+'/Account/Login?ReturnUrl=%2Froadmap'});
 await wait(`!!document.querySelector('#login-identifier')`);
 // Intercept the heavy module: the preview must remain visible while it is delayed.
 await send('Fetch.enable',{patterns:[{urlPattern:'*/dist/vietnam-map.js*',requestStage:'Request'}]});
 await evaluate(`(()=>{document.querySelector('#login-identifier').value='demo@softsync.local';document.querySelector('#login-password').value='Demo@12345';document.querySelector('.login-submit').click();})()`);
 await wait(`!!document.querySelector('.vn-map-preview') && document.querySelector('.vn-map-preview').complete && document.querySelector('.vn-map-preview').naturalWidth>0`);
 const preview=await evaluate(`(()=>{const img=document.querySelector('.vn-map-preview'), stage=img.parentElement;const state=stage.querySelector('.vn-map-loading');return {source:img.getAttribute('src'),width:img.naturalWidth,visible:img.getBoundingClientRect().height>0,overlayHeight:state?.getBoundingClientRect().height||0,stageHeight:stage.getBoundingClientRect().height,interactive:!!document.querySelector('.vn-journey[data-map-ready="true"]')};})()`);
 assert.equal(preview.visible,true);
 assert.ok(preview.source.endsWith('.webp'));
 assert.equal(preview.interactive,false);
 assert.ok(preview.overlayHeight<preview.stageHeight/2,'Loading message must not cover the illustration');
 assert.ok(paused.length>0,'Heavy module must be withheld for this test');
 const started=performance.now();
 await send('Fetch.disable');
 await wait(`!!document.querySelector('.vn-journey[data-map-ready="true"] .vn-adventure-stop')`);
 const readyMs=Math.round(performance.now()-started);
 assert.equal(requests.some(url=>url.includes('vietnam-adventure-coast-v1.png')),false,'Map must not fetch the old 2.84 MB PNG');
 assert.equal(await evaluate(`getComputedStyle(document.querySelector('.vn-controls')).visibility`),'visible');
 await evaluate(`document.querySelector('.vn-controls button').click()`);
 await wait(`document.querySelector('.vn-journey').dataset.mapAction==='zoomIn'`);
 await evaluate(`document.querySelector('.vn-adventure-stop').click()`);
 await wait(`!!document.querySelector('.vn-sheet')`);
 assert.equal(errors.length,0,JSON.stringify(errors));
 console.log(JSON.stringify({passed:true,previewWhileModuleDelayed:preview,readyAfterReleaseMs:readyMs,zoom:true,checkpoint:true}));
}finally{await call('Target.disposeBrowserContext',{browserContextId});ws.close();}
