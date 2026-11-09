// Run: node --test tests/softsync-motion.test.cjs
const { test } = require('node:test');
const assert = require('node:assert/strict');
const vm = require('node:vm');
const fs = require('node:fs');
const source = fs.readFileSync(require('node:path').join(__dirname, '../SoftSync.Presentation/wwwroot/js/softsync-motion.js'), 'utf8');
function setup({ reduced = false, keyboard = false } = {}) {
    const events = {}, blazor = {}, observers = [], frames = [], animations = [];
    const root = { dataset: keyboard ? { motionInput: 'keyboard' } : {} };
    const surface = { dataset: { pageRoute: '/home' }, animate: (frames, options) => {
        const a = { frames, options, cancelled: false, cancel() { this.cancelled = true; } };
        animations.push(a); return a;
    } };
    const media = { matches: reduced, addEventListener: (_, fn) => { events.media = fn; } };
    const document = { documentElement: root, body: {}, querySelector: () => surface, addEventListener: (name, fn) => { events[name] = fn; } };
    const window = { addEventListener: (name, fn) => { events[name] = fn; }, Blazor: { addEventListener: (name, fn) => { blazor[name] = fn; } } };
    const context = { window, document, location: { pathname: '/home', search: '', href: 'https://test/home', origin: 'https://test' }, matchMedia: () => media, requestAnimationFrame: fn => frames.push(fn), getComputedStyle: () => ({ getPropertyValue: () => 'ease' }), MutationObserver: class { constructor(fn) { observers.push(fn); } observe() {} }, sessionStorage: { getItem: () => null, removeItem() {}, setItem() {} }, URL, Date };
    vm.runInNewContext(source, context);
    const flush = () => { while (frames.length) frames.shift()(); };
    flush();
    return { context, animations, root, media, events, observers, surface, flush, blazor, rerun: () => vm.runInNewContext(source, context) };
}
test('waits for committed interactive route and never restarts on data patches', () => {
    const s = setup();
    assert.equal(s.animations.length, 1);
    s.context.location.pathname = '/roadmap';
    s.observers[1](); s.flush();
    assert.equal(s.animations.length, 1);
    s.surface.dataset.pageRoute = '/roadmap';
    s.observers[1](); s.flush();
    assert.equal(s.animations.length, 2);
    assert.ok(s.animations[0].cancelled);
    s.observers[1](); s.blazor.enhancedload(); s.flush();
    assert.equal(s.animations.length, 2);
    assert.equal(s.animations[1].options.duration, 200);
    assert.equal(s.animations[1].options.fill, undefined);
});
test('OS and app reduction cancel motion, keyboard navigation stays instant', () => {
    assert.equal(setup({ reduced: true }).animations.length, 0);
    assert.equal(setup({ keyboard: true }).animations.length, 0);
    const s = setup();
    s.root.dataset.reduceMotion = '1'; s.observers[0]();
    assert.ok(s.animations[0].cancelled);
    s.context.location.pathname = s.surface.dataset.pageRoute = '/settings';
    s.observers[1](); s.flush();
    assert.equal(s.animations.length, 1);
    delete s.root.dataset.reduceMotion;
    s.events.keydown();
    s.context.location.pathname = s.surface.dataset.pageRoute = '/more';
    s.observers[1](); s.flush();
    assert.equal(s.animations.length, 1);
});
test('duplicate script execution does not install another lifecycle', () => {
    const s = setup();
    s.rerun();
    assert.equal(s.observers.length, 2);
    assert.equal(s.animations.length, 1);
});
