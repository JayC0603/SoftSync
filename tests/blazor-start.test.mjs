import test from 'node:test';
import assert from 'node:assert/strict';
import { readFileSync } from 'node:fs';
import vm from 'node:vm';

const source = readFileSync(new URL('../SoftSync-main/SoftSync.Presentation/wwwroot/js/blazor-start.js', import.meta.url), 'utf8');

test('opens a direct WebSocket and tolerates a brief network interruption', async () => {
    let starts = 0;
    let configuration;
    const context = vm.createContext({
        window: {},
        Blazor: { start(options) { starts++; configuration = options; return Promise.resolve(); } },
        console
    });
    vm.runInContext(source, context);
    vm.runInContext(source, context);
    assert.equal(starts, 1, 'enhanced navigation must not create another circuit');
    const calls = {};
    const builder = {
        withUrl(url, options) { calls.url = url; calls.transport = options.transport; calls.skipNegotiation = options.skipNegotiation; return this; },
        withServerTimeout(value) { calls.timeout = value; return this; },
        withKeepAliveInterval(value) { calls.keepAlive = value; return this; }
    };
    configuration.circuit.configureSignalR(builder);
    assert.equal(calls.url, '/_blazor');
    assert.equal(calls.transport, 1);
    assert.equal(calls.skipNegotiation, true);
    assert.equal(calls.timeout, 60000);
    assert.equal(calls.keepAlive, 15000);
    assert.ok(calls.timeout >= 2 * calls.keepAlive);
    const retries = configuration.circuit.reconnectionOptions;
    assert.equal(retries.retryIntervalMilliseconds(0, retries.maxRetries), 1000);
    assert.equal(retries.retryIntervalMilliseconds(10, retries.maxRetries), 5000);
    assert.equal(retries.retryIntervalMilliseconds(retries.maxRetries, retries.maxRetries), null);
});

test('startup failures expose the existing reload action without automatically reloading forms', async () => {
    const notice = { style: {} };
    let errors = 0;
    const context = vm.createContext({
        window: {},
        Blazor: { start: () => Promise.reject(new Error('offline')) },
        document: { getElementById(id) { assert.equal(id, 'blazor-error-ui'); return notice; } },
        console: { error() { errors++; } }
    });
    vm.runInContext(source, context);
    await new Promise(resolve => setImmediate(resolve));
    assert.equal(notice.style.display, 'block');
    assert.equal(errors, 1);
});
