// Run against the demo profile and a Chromium browser started with
// --remote-debugging-port=9466. No third-party test dependencies are required.
import assert from 'node:assert/strict';

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
            if (await evaluate(expression)) return;
            await pause(100);
        }
        throw Error(`Timed out: ${expression}`);
    };
    await send('Page.enable');
    await send('Runtime.enable');
    await send('Network.enable');
    await send('Page.addScriptToEvaluateOnNewDocument', { source: `
        window.__loadId = Math.random();
        const NativeSocket = window.WebSocket;
        window.WebSocket = class extends NativeSocket {
            constructor(...args) {
                super(...args);
                if (String(args[0]).includes('/_blazor')) window.__testSocket = this;
            }
        };
    ` });
    await send('Network.setCookie', { name: 'ss-lang', value: 'en', url: app });
    await send('Emulation.setDeviceMetricsOverride', { width: 390, height: 844, deviceScaleFactor: 1, mobile: true });
    await send('Page.navigate', { url: `${app}/Account/Login?ReturnUrl=%2Fmore` });
    await wait(`!!document.querySelector('#login-identifier')`);
    await evaluate(`(() => {
        document.querySelector('#login-identifier').value = 'demo@softsync.local';
        document.querySelector('#login-password').value = 'Demo@12345';
        document.querySelector('.login-submit').click();
    })()`);
    await wait(`location.pathname === '/more' && window.__testSocket?.readyState === 1 && !!document.querySelector('.ss-header-language button:not(:disabled)')`);
    const initialLoad = await evaluate('window.__loadId');
    await evaluate(`window.__oldSocket = window.__testSocket; window.__testSocket.close()`);
    await wait(`window.__testSocket !== window.__oldSocket && window.__testSocket.readyState === 1 && !document.querySelector('#ss-connection-notice')`);
    assert.equal(await evaluate('window.__loadId'), initialLoad, 'a brief socket drop should resume without a page reload');
    assert.equal(await evaluate(`!!document.querySelector('#blazor-error-ui')`), false);

    await evaluate(`Blazor.reconnect = async () => false; window.__testSocket.close()`);
    await wait(`window.__loadId !== ${initialLoad} && location.pathname === '/more' && window.__testSocket?.readyState === 1 && !!document.querySelector('.ss-header-language button:not(:disabled)')`);
    const recoveredLoad = await evaluate('window.__loadId');
    assert.equal(await evaluate(`!document.querySelector('#ss-connection-notice')`), true);
    assert.equal(await evaluate(`!!document.querySelector('#blazor-error-ui')`), false);
    assert.equal(await evaluate(`!!document.querySelector('#components-reconnect-modal') && getComputedStyle(document.querySelector('#components-reconnect-modal')).display !== 'none'`), false);

    await evaluate(`(() => {
        const draft = document.createElement('textarea');
        draft.id = 'recovery-test-draft';
        document.querySelector('main').appendChild(draft);
        draft.value = 'Keep my unsaved learning notes';
        draft.dispatchEvent(new Event('input', { bubbles: true }));
        window.__draftReconnectAttempted = false;
        Blazor.reconnect = async () => { window.__draftReconnectAttempted = true; return false; };
        window.__testSocket.close();
    })()`);
    await wait(`window.__draftReconnectAttempted === true`);
    await pause(500);
    assert.equal(await evaluate('window.__loadId'), recoveredLoad);
    assert.equal(await evaluate(`document.querySelector('#recovery-test-draft').value`), 'Keep my unsaved learning notes');
    assert.equal(await evaluate(`!!document.querySelector('#ss-connection-notice')`), false);
    assert.equal(errors.length, 0, JSON.stringify(errors));
    console.log(JSON.stringify({ passed: true, transientResume: true, expiredSessionReload: true, draftPreserved: true }));
} finally {
    await call('Target.disposeBrowserContext', { browserContextId });
    ws.close();
}
