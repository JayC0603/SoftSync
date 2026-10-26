// Run against the demo profile and a Chromium browser started with
// --remote-debugging-port=9466. No third-party test dependencies are required.
import assert from 'node:assert/strict';
import { writeFile, mkdir } from 'node:fs/promises';

const app = process.env.APP_URL || 'http://127.0.0.1:5104';
const version = await (await fetch(process.env.BROWSER_DEBUG_URL || 'http://127.0.0.1:9466/json/version')).json();
const ws = new WebSocket(version.webSocketDebuggerUrl);
await new Promise((resolve, reject) => { ws.onopen = resolve; ws.onerror = reject; });
let id = 0;
const pending = new Map();
const requests = [];
const errors = [];
ws.onmessage = event => {
    const message = JSON.parse(event.data);
    if (message.id) {
        const promise = pending.get(message.id);
        pending.delete(message.id);
        message.error ? promise.reject(Error(JSON.stringify(message.error))) : promise.resolve(message.result);
    } else if (message.method === 'Network.requestWillBeSent') requests.push(message.params);
    else if (message.method === 'Runtime.exceptionThrown') errors.push(message.params.exceptionDetails);
};
const call = (method, params = {}, sessionId) => new Promise((resolve, reject) => {
    const next = ++id;
    pending.set(next, { resolve, reject });
    ws.send(JSON.stringify({ id: next, method, params, ...(sessionId ? { sessionId } : {}) }));
});
const pause = ms => new Promise(resolve => setTimeout(resolve, ms));
const { browserContextId } = await call('Target.createBrowserContext');
try {
    const { targetId } = await call('Target.createTarget', { url: 'about:blank', browserContextId });
    const { sessionId } = await call('Target.attachToTarget', { targetId, flatten: true });
    const send = (method, params) => call(method, params, sessionId);
    const evaluate = async expression => {
        const result = await send('Runtime.evaluate', { expression, returnByValue: true, awaitPromise: true });
        if (result.exceptionDetails) throw Error(JSON.stringify(result.exceptionDetails));
        return result.result.value;
    };
    const wait = async expression => {
        for (let n = 0; n < 150; n++) {
            if (await evaluate(`Boolean(${expression})`)) return;
            await pause(100);
        }
        throw Error(`Timed out: ${expression}`);
    };
    await send('Page.enable');
    await send('Runtime.enable');
    await send('Network.enable');
    await send('Emulation.setDeviceMetricsOverride', { width: 390, height: 844, deviceScaleFactor: 1, mobile: true });
    const results = [];
    const output = new URL('../../.integration-tools/landing-reference/', import.meta.url);
    await mkdir(output, {recursive:true});
    await send('Page.navigate', { url: app });
    await wait(`!!document.querySelector('.lp-hero') && !!document.querySelector('.ss-public-language-switch button:last-child')`);
    await pause(800);
    for (const width of [360, 390, 640, 840, 1440]) {
        await send('Emulation.setDeviceMetricsOverride', { width, height:1000, deviceScaleFactor:1, mobile:width < 500 });
        await evaluate(`document.documentElement.dataset.theme='light'`);
        await pause(200);
        await evaluate(`(async()=>{for(let top=0;top<document.body.scrollHeight;top+=700){scrollTo(0,top);await new Promise(r=>setTimeout(r,50));}await Promise.all([...document.images].map(image=>image.decode().catch(()=>{})));scrollTo(0,0);})()`);
        const layout = await evaluate(`(() => {
            const header=document.querySelector('.ss-public-header').getBoundingClientRect();
            const footer=document.querySelector('footer');
            return { width:innerWidth, scrollWidth:document.documentElement.scrollWidth, sections:document.querySelectorAll('.lp-landing section').length,
                footerCount:document.querySelectorAll('footer').length, footer:footer.textContent.includes('SoftSync'),
                brokenImages:[...document.images].filter(image=>!image.complete || image.naturalWidth===0).map(image=>image.src),
                clippedControls:[...document.querySelectorAll('.ss-public-header a,.ss-public-header button,.ss-public-header summary')].filter(element=>{
                    if(element.closest('details:not([open])') && !element.matches('summary')) return false; const r=element.getBoundingClientRect();return r.width>0 && r.height>0 && (r.x<0 || r.right>innerWidth);
                }).map(element=>element.outerHTML),
                columns:['.lp-hero','.lp-benefits','.lp-journey','.lp-feature-grid','.lp-mentor-grid'].map(selector=>getComputedStyle(document.querySelector(selector)).gridTemplateColumns.split(' ').length), statistics:[...document.querySelectorAll('.lp-highlights strong')].map(element=>element.textContent), header:{left:header.left,right:header.right}, title:document.querySelector('h1').textContent };
        })()`);
        assert.ok(layout.scrollWidth <= width, JSON.stringify(layout));
        assert.deepEqual(layout.columns,[2,2,2,4,3],'Reference column layout must not change');
        assert.deepEqual(layout.statistics,['10.000+','500+','95%']);
        assert.equal(layout.footerCount,1);
        assert.equal(layout.footer,true);
        assert.equal(layout.brokenImages.length,0,JSON.stringify(layout));
        assert.equal(layout.clippedControls.length,0,JSON.stringify(layout));
        const metrics = await send('Page.getLayoutMetrics');
        const screenshot = await send('Page.captureScreenshot',{format:'png',captureBeyondViewport:true,clip:{x:0,y:0,width,height:Math.ceil(metrics.cssContentSize.height),scale:1}});
        await writeFile(new URL(`landing-${width}.png`,output),Buffer.from(screenshot.data,'base64'));
        results.push(layout);
    }
    await evaluate(`document.querySelector('.ss-public-language-switch button:last-child').click()`);
    await wait(`document.querySelector('h1').textContent.includes('Grow your')`);
    assert.equal(await evaluate(`document.querySelectorAll('footer').length`),1);
    await send('Emulation.setDeviceMetricsOverride',{width:390,height:844,deviceScaleFactor:1,mobile:true});
    await evaluate(`document.querySelector('.ss-public-menu-placeholder').click()`);
    assert.equal(await evaluate(`document.querySelector('.ss-public-menu-placeholder').getAttribute('aria-disabled')`),'true');
    assert.equal(await evaluate(`document.querySelector('.ss-public-mobile') === null`),true,'Hamburger content is reserved for a later design');
    await evaluate(`document.querySelector('.ss-public-actions button[onclick]').click()`);
    assert.equal(await evaluate(`document.documentElement.dataset.theme`),'dark');
    const dark = await send('Page.captureScreenshot',{format:'png'});
    await writeFile(new URL('landing-dark-390.png',output),Buffer.from(dark.data,'base64'));
    await evaluate(`document.querySelector('.lp-feature-card[href="/progress"]').click()`);
    await wait(`!!document.querySelector('#login-identifier')`);
    await evaluate(`(() => {
        document.querySelector('#login-identifier').value='demo@softsync.local';
        document.querySelector('#login-password').value='Demo@12345';
        document.querySelector('.login-submit').click();
    })()`);
    await wait(`location.pathname === '/progress' && !!document.querySelector('.ss-mobile-bottom-nav')`);
    const taskbarLinks=await evaluate(`[...document.querySelectorAll('.ss-mobile-bottom-nav a')].map(link=>link.getAttribute('href'))`);
    await send('Page.navigate',{url:app});
    await wait(`!!document.querySelector('.ss-authenticated-landing .lp-hero') && !!document.querySelector('.ss-header-language button:not(:disabled)')`);
    assert.deepEqual(await evaluate(`[...document.querySelectorAll('.ss-mobile-bottom-nav a')].map(link=>link.getAttribute('href'))`),taskbarLinks);
    assert.equal(await evaluate(`document.querySelectorAll('footer').length`),1);
    assert.equal(await evaluate(`!!document.querySelector('.ss-public-header')`),false,'Signed-in landing must use the project taskbar');
    const authenticated=await send('Page.captureScreenshot',{format:'png'});
    await writeFile(new URL('landing-authenticated-390.png',output),Buffer.from(authenticated.data,'base64'));
    assert.equal(errors.length,0,JSON.stringify(errors));
    console.log(JSON.stringify({passed:true, results, languageSwitch:true, reservedMenu:true, authenticatedTaskbar:true, darkMode:true, protectedFeatureRedirect:true}));
} finally {
    await call('Target.disposeBrowserContext',{browserContextId});
    ws.close();
}
