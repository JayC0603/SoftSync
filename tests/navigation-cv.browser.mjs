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
let intercept;
ws.onmessage = event => {
    const message = JSON.parse(event.data);
    if (message.id) {
        const promise = pending.get(message.id);
        pending.delete(message.id);
        message.error ? promise.reject(Error(JSON.stringify(message.error))) : promise.resolve(message.result);
    } else if (message.method === 'Network.requestWillBeSent') requests.push(message.params);
    else if (message.method === 'Fetch.requestPaused') intercept?.(message.params);
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
            if (await evaluate(expression)) return;
            await pause(100);
        }
        throw Error(`Timed out: ${expression}`);
    };
    await send('Page.enable');
    await send('Runtime.enable');
    await send('Network.enable');
    await send('Emulation.setDeviceMetricsOverride', { width: 390, height: 844, deviceScaleFactor: 1, mobile: true });
    await send('Page.navigate', { url: `${app}/Account/Login?ReturnUrl=%2Fmore` });
    await wait(`!!document.querySelector('#login-identifier')`);
    await evaluate(`(() => {
        document.querySelector('#login-identifier').value = 'demo@softsync.local';
        document.querySelector('#login-password').value = 'Demo@12345';
        document.querySelector('.login-submit').click();
    })()`);
    await wait(`document.querySelector('main[data-page-route="/more"]') && !!document.querySelector('.ss-mobile-bottom-nav a.active')`);
    await pause(500);
    assert.equal(await evaluate(`!!document.head.querySelector('link[href*="cv-career.css"]')`), true);
    let release;
    intercept = paused => { release = () => send('Fetch.continueRequest', { requestId: paused.requestId }); };
    await send('Fetch.enable', { patterns: [{ urlPattern: `${app}/cv`, requestStage: 'Request' }] });
    await evaluate(`(() => {
        window.__routeFrames = [];
        const until = performance.now() + 2500;
        const sample = () => {
            const main = document.querySelector('#main-content');
            const active = document.querySelector('.ss-mobile-bottom-nav a.active');
            const menu = document.querySelector('.career-menu');
            window.__routeFrames.push({ rendered: main?.dataset.pageRoute, active: active?.getAttribute('href'), cvStyled: !menu || getComputedStyle(menu).display === 'grid' });
            if (performance.now() < until) requestAnimationFrame(sample);
        };
        sample();
        document.querySelector('.ss-mobile-bottom-nav a[href="/cv"]').click();
    })()`);
    for (let n = 0; n < 100 && !release; n++) await pause(50);
    assert.ok(release, 'CV navigation request should be intercepted');
    await pause(500);
    const pendingState = await evaluate(`({ route:document.querySelector('#main-content').dataset.pageRoute, active:document.querySelector('.ss-mobile-bottom-nav a.active')?.getAttribute('href'), busy:document.querySelector('#main-content').getAttribute('aria-busy'), statusHidden:document.querySelector('#ss-navigation-status').hidden })`);
    assert.deepEqual(pendingState, { route: '/more', active: '/more', busy: 'true', statusHidden: false });
    await release();
    await send('Fetch.disable');
    await wait(`document.querySelector('main[data-page-route="/cv"] .career-menu-card') && document.querySelector('.ss-mobile-bottom-nav a.active')?.getAttribute('href') === '/cv'`);
    await pause(1800);
    const frames = await evaluate('window.__routeFrames');
    const mismatches = frames.filter(frame => frame.active !== frame.rendered || !frame.cvStyled);
    assert.equal(mismatches.length, 0, JSON.stringify(mismatches.slice(0, 5)));
    assert.equal(await evaluate(`document.querySelector('#ss-navigation-status').hidden`), true);
    const sizes = [];
    for (const width of [375, 390, 430, 1440]) {
        await send('Emulation.setDeviceMetricsOverride', { width, height: 900, deviceScaleFactor: 1, mobile: width < 500 });
        await pause(200);
        for (const theme of ['light', 'dark']) {
            await evaluate(`document.documentElement.dataset.theme = '${theme}'`);
            const layout = await evaluate(`(() => {
                const rect = node => { const r=node.getBoundingClientRect(); return {x:r.x,width:r.width,height:r.height}; };
                return { cards:[...document.querySelectorAll('.career-menu-card')].map(rect), menu:rect(document.querySelector('.career-menu')), tip:rect(document.querySelector('.career-tip')), overflow:document.documentElement.scrollWidth > innerWidth };
            })()`);
            const widths = layout.cards.map(card => card.width);
            assert.ok(Math.max(...widths) - Math.min(...widths) < 1, JSON.stringify(layout));
            assert.ok(Math.abs(layout.menu.width - layout.tip.width) < 1, JSON.stringify(layout));
            assert.equal(layout.overflow, false, JSON.stringify(layout));
            sizes.push({ width, theme, cardWidths: widths });
        }
    }
    await send('Emulation.setDeviceMetricsOverride', { width:390,height:844,deviceScaleFactor:1,mobile:true });
    await evaluate(`document.documentElement.dataset.theme='light'`);
    await pause(200);
    await mkdir(new URL('../../.integration-tools/navigation-cv/', import.meta.url), { recursive:true });
    const screenshot = await send('Page.captureScreenshot', { format:'png' });
    await writeFile(new URL('../../.integration-tools/navigation-cv/cv-after.png', import.meta.url), Buffer.from(screenshot.data,'base64'));
    for (const path of ['/progress','/roadmap','/dashboard','/cv','/more']) {
        await evaluate(`document.querySelector('.ss-mobile-bottom-nav a[href="${path}"]').click()`);
        await wait(`document.querySelector('#main-content')?.dataset.pageRoute === '${path}' && document.querySelector('.ss-mobile-bottom-nav a.active')?.getAttribute('href') === '${path}'`);
    }
    assert.equal(errors.length, 0, JSON.stringify(errors));
    console.log(JSON.stringify({ passed:true, delayedNavigation:pendingState, frames:frames.length, mismatches:mismatches.length, sizes }));
} finally {
    await call('Target.disposeBrowserContext', { browserContextId });
    ws.close();
}
