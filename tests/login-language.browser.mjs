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
    const results = [];
    for (const width of [390, 1440]) {
        await send('Emulation.setDeviceMetricsOverride', { width, height: 900, deviceScaleFactor: 1, mobile: width < 500 });
        await send('Page.navigate', { url: `${app}/Account/Login?ReturnUrl=%2Fdashboard` });
        await wait(`!!document.querySelector('#login-identifier') && document.readyState === 'complete'`);
        await evaluate(`window.ssDesign.setAccountLanguage('vi')`);
        await evaluate(`(() => {
            const input = document.querySelector('#login-identifier');
            input.value = 'draft@example.com'; input.focus(); input.setSelectionRange(2, 5);
            document.querySelector('#login-password').value = 'draft-password';
            document.querySelector('#rememberMe').checked = false;
            window.ssDesign.togglePassword(document.querySelector('.ss-password-visibility'));
            window.loginForm = document.querySelector('.login-card form');
            window.loginInput = input;
            window.loginToken = loginForm.querySelector('[name="__RequestVerificationToken"]').value;
        })()`);
        const positions = await evaluate(`['#login-identifier','#login-password','.login-submit'].map(s => { const r = document.querySelector(s).getBoundingClientRect(); return [r.x,r.y,r.width,r.height]; })`);
        const before = requests.length;
        await send('Network.emulateNetworkConditions', { offline: true, latency: 0, downloadThroughput: -1, uploadThroughput: -1 });
        for (const code of ['en', 'en', 'vi', 'en', 'vi', 'en']) {
            const result = await evaluate(`(() => {
                const start = performance.now();
                document.querySelector('[data-account-language="${code}"]').click();
                return {
                    ms: performance.now() - start,
                    lang: document.documentElement.lang,
                    title: document.title,
                    heading: document.querySelector('.login-heading h2').textContent,
                    sameForm: window.loginForm === document.querySelector('.login-card form'),
                    sameInput: window.loginInput === document.querySelector('#login-identifier'),
                    identifier: loginInput.value, password: document.querySelector('#login-password').value,
                    visible: document.querySelector('#login-password').type === 'text',
                    remembered: document.querySelector('#rememberMe').checked,
                    selection: [loginInput.selectionStart,loginInput.selectionEnd],
                    sameToken: loginToken === loginForm.querySelector('[name="__RequestVerificationToken"]').value,
                    pressed: document.querySelector('[data-account-language="${code}"]').getAttribute('aria-pressed'),
                    placeholder: document.querySelector('#login-password').placeholder,
                    footer: document.querySelector('.sh-footer-brand small').textContent
                };
            })()`);
            assert.equal(result.lang, code);
            assert.equal(result.heading, code === 'vi' ? 'Đăng nhập' : 'Sign in');
            assert.equal(result.title, result.heading);
            assert.equal(result.sameForm && result.sameInput && result.sameToken, true);
            assert.equal(result.identifier, 'draft@example.com');
            assert.equal(result.password, 'draft-password');
            assert.equal(result.visible, true);
            assert.equal(result.remembered, false);
            assert.deepEqual(result.selection, [2, 5]);
            assert.equal(result.pressed, 'true');
            assert.equal(result.placeholder, code === 'vi' ? 'Nhập mật khẩu của bạn' : 'Enter your password');
            assert.equal(result.footer, code === 'vi' ? 'Học kỹ năng mềm cùng AI.' : 'Learn soft skills with AI.');
            results.push({ width, code, ms: result.ms });
        }
        await pause(200);
        assert.equal(requests.length, before, 'language switching must not make network requests');
        const afterPositions = await evaluate(`['#login-identifier','#login-password','.login-submit'].map(s => { const r = document.querySelector(s).getBoundingClientRect(); return [r.x,r.y,r.width,r.height]; })`);
        assert.deepEqual(afterPositions, positions, 'switching language must not move form controls');
        await send('Network.emulateNetworkConditions', { offline: false, latency: 0, downloadThroughput: -1, uploadThroughput: -1 });
        assert.equal(await evaluate(`localStorage.getItem('ss-lang') === 'en' && document.cookie.includes('ss-lang=en')`), true);
    }
    await evaluate(`(() => {
        document.querySelector('#login-identifier').value = 'demo@softsync.local';
        document.querySelector('#login-password').value = 'Demo@12345';
        document.querySelector('.login-submit').click();
    })()`);
    await wait(`location.pathname === '/dashboard' && !!document.querySelector('.ss-mobile-bottom-nav')`);
    assert.equal(await evaluate(`document.documentElement.lang`), 'en', 'server must respect the selected language after login');
    assert.equal(errors.length, 0, JSON.stringify(errors));
    console.log(JSON.stringify({ passed: true, switches: results, loginAfterSwitch: true }));
} finally {
    await call('Target.disposeBrowserContext', { browserContextId });
    ws.close();
}
