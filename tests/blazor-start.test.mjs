import test from 'node:test';
import assert from 'node:assert/strict';
import { readFileSync } from 'node:fs';
import vm from 'node:vm';

const source = readFileSync(new URL('../SoftSync-main/SoftSync.Presentation/wwwroot/js/blazor-start.js', import.meta.url), 'utf8');

function harness({ reconnect = async () => true, healthy = true, fields = [], lastReload } = {}) {
    const handlers = {};
    const message = { textContent: '' };
    const button = { textContent: '', hidden: true };
    const notice = { hidden: true, querySelector: selector => selector === 'p' ? message : button };
    const errorNotice = { style: {} };
    const storage = new Map(lastReload ? [['ss-connection-reload', String(lastReload)]] : []);
    let configuration;
    const counts = { starts: 0, reloads: 0, reconnects: 0, health: 0, errors: 0 };
    const context = vm.createContext({
        window: { addEventListener(name, handler) { handlers[name] = handler; } },
        document: {
            documentElement: { lang: 'en' }, visibilityState: 'visible',
            addEventListener(name, handler) { handlers[name] = handler; },
            querySelectorAll: () => fields,
            getElementById: id => id === 'ss-connection-notice' ? notice : errorNotice
        },
        sessionStorage: { getItem: key => storage.get(key), setItem: (key, value) => storage.set(key, value) },
        location: { reload() { counts.reloads++; } },
        Blazor: {
            start(options) { counts.starts++; configuration = options; return Promise.resolve(); },
            reconnect() { counts.reconnects++; return reconnect(); }
        },
        fetch: async () => { counts.health++; return { ok: healthy, json: async () => ({ status: healthy ? 'ok' : 'unavailable' }) }; },
        AbortSignal: { timeout: () => undefined },
        setTimeout: callback => setImmediate(callback),
        console: { error() { counts.errors++; } }
    });
    vm.runInContext(source, context);
    const settle = async () => { for (let n = 0; n < 12; n++) await new Promise(resolve => setImmediate(resolve)); };
    return { context, configuration, counts, handlers, notice, message, button, errorNotice, settle };
}

test('starts once and opens a direct WebSocket with aligned keep-alive settings', () => {
    const h = harness();
    vm.runInContext(source, h.context);
    assert.equal(h.counts.starts, 1);
    const calls = {};
    h.configuration.circuit.configureSignalR({
        withUrl(url, options) { calls.url = url; calls.transport = options.transport; calls.skipNegotiation = options.skipNegotiation; return this; },
        withServerTimeout(value) { calls.timeout = value; return this; },
        withKeepAliveInterval(value) { calls.keepAlive = value; return this; }
    });
    assert.deepEqual(calls, { url: '/_blazor', transport: 1, skipNegotiation: true, timeout: 60000, keepAlive: 15000 });
});

test('a brief disconnect resumes the circuit without reloading', async () => {
    const h = harness();
    h.configuration.circuit.reconnectionHandler.onConnectionDown();
    await h.settle();
    assert.equal(h.counts.reconnects, 1);
    assert.equal(h.counts.reloads, 0);
    assert.equal(h.counts.health, 0);
    assert.equal(h.notice.hidden, true);
});

test('a discarded circuit opens a fresh session once the server is healthy', async () => {
    const h = harness({ reconnect: async () => false });
    h.configuration.circuit.reconnectionHandler.onConnectionDown();
    h.configuration.circuit.reconnectionHandler.onConnectionDown();
    await h.settle();
    assert.equal(h.counts.reconnects, 1, 'duplicate disconnect notifications must share one recovery');
    assert.equal(h.counts.health, 1);
    assert.equal(h.counts.reloads, 1);
});

test('unsaved input prevents automatic reload after the server discarded a circuit', async () => {
    const h = harness({ reconnect: async () => false });
    h.handlers.input({ target: { matches: () => true, isConnected: true } });
    h.configuration.circuit.reconnectionHandler.onConnectionDown();
    await h.settle();
    assert.equal(h.counts.reloads, 0);
    assert.equal(h.counts.health, 0);
    assert.match(h.message.textContent, /Your unsaved input/);
    assert.equal(h.button.hidden, false);
});

test('password-manager values are protected even without an input event', async () => {
    const h = harness({ reconnect: async () => false, fields: [{ type: 'password', value: 'autofilled', defaultValue: '' }] });
    h.configuration.circuit.reconnectionHandler.onConnectionDown();
    await h.settle();
    assert.equal(h.counts.reloads, 0);
    assert.match(h.message.textContent, /Your unsaved input/);
});

test('an offline server ends retries and offers a working native reload action', async () => {
    const h = harness({ reconnect: async () => { throw Error('offline'); } });
    h.configuration.circuit.reconnectionHandler.onConnectionDown();
    await h.settle();
    assert.equal(h.counts.reconnects, 3);
    assert.equal(h.counts.reloads, 0);
    assert.equal(h.notice.hidden, false);
    assert.equal(h.button.hidden, false);
    assert.match(h.message.textContent, /Check your network/);
});

test('a failing health check never triggers an automatic reload', async () => {
    const h = harness({ reconnect: async () => false, healthy: false });
    h.configuration.circuit.reconnectionHandler.onConnectionDown();
    await h.settle();
    assert.equal(h.counts.health, 1);
    assert.equal(h.counts.reloads, 0);
    assert.equal(h.button.hidden, false);
});

test('a recent automatic reload prevents a reload loop', async () => {
    const h = harness({ reconnect: async () => false, lastReload: Date.now() });
    h.configuration.circuit.reconnectionHandler.onConnectionDown();
    await h.settle();
    assert.equal(h.counts.reloads, 0);
    assert.equal(h.counts.health, 0);
    assert.equal(h.button.hidden, false);
});

test('a background tab waits until visible before opening a fresh session', async () => {
    const h = harness({ reconnect: async () => false });
    h.context.document.visibilityState = 'hidden';
    h.configuration.circuit.reconnectionHandler.onConnectionDown();
    await h.settle();
    assert.equal(h.counts.reloads, 0);
    h.context.document.visibilityState = 'visible';
    h.handlers.visibilitychange();
    await h.settle();
    assert.equal(h.counts.reloads, 1);
});

test('connection recovery cancels an in-flight reconnect attempt', async () => {
    let resolve;
    const h = harness({ reconnect: () => new Promise(r => { resolve = r; }) });
    h.configuration.circuit.reconnectionHandler.onConnectionDown();
    h.configuration.circuit.reconnectionHandler.onConnectionUp();
    resolve(false);
    await h.settle();
    assert.equal(h.counts.reloads, 0);
    assert.equal(h.notice.hidden, true);
});

test('startup errors expose the existing reload action without discarding forms', async () => {
    const h = harness();
    // Use a new window to simulate a fresh startup that fails.
    h.context.window.ssBlazorStarted = false;
    h.context.Blazor.start = () => Promise.reject(Error('offline'));
    vm.runInContext(source, h.context);
    await h.settle();
    assert.equal(h.errorNotice.style.display, 'block');
    assert.equal(h.counts.reloads, 0);
    assert.equal(h.counts.errors, 1);
});
