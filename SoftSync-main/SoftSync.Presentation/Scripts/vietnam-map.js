import 'maplibre-gl/dist/maplibre-gl.css';
import '../Styles/vietnam-mobile.css';

// Keep the route entry small. A tab switch must not parse WebGL code or create
// a map for a page the learner has already left.
const instances = new WeakMap();
const active = new Set();
const connected = state => !state.disposed && state.root.isConnected && state.canvas.isConnected;

function cancelScheduled(state) {
    clearTimeout(state.delay);
    if (state.idle !== undefined) window.cancelIdleCallback?.(state.idle);
    state.delay = state.idle = undefined;
}
async function load(state) {
    cancelScheduled(state);
    if (!connected(state)) return null;
    if (state.loading) return state.loading;
    state.loading = (async () => {
        try {
            const engine = await import('./vietnam-map-engine.js');
            // Dynamic import cannot be aborted. Discard late results before
            // mounting observers, fetching geography, or allocating WebGL.
            if (!connected(state)) return null;
            state.engine = engine;
            await engine.mount(state.root, state.canvas, state.payload, state.callback);
            if (!connected(state)) { engine.dispose(state.root); return null; }
            if (state.expanded) engine.expand(state.root, true);
            return engine;
        } catch (error) {
            if (connected(state)) {
                console.warn('Vietnam map unavailable:', error.message);
                state.callback.invokeMethodAsync('MapStatus', false).catch(() => {});
            }
            state.loading = null;
            return null;
        }
    })();
    return state.loading;
}
function schedule(state) {
    if (!connected(state) || state.loading || state.delay !== undefined || state.idle !== undefined) return;
    // Allow the new route to paint and a quick next-tab tap to cancel the work.
    state.delay = setTimeout(() => {
        state.delay = undefined;
        if (!connected(state)) return;
        if (window.requestIdleCallback) {
            state.idle = requestIdleCallback(() => { state.idle = undefined; void load(state); }, { timeout: 700 });
        } else {
            void load(state);
        }
    }, 200);
}
export function mount(root, canvas, payload, callback) {
    if (!root?.isConnected || !canvas?.isConnected) return;
    if (instances.has(root)) return update(root, payload);
    const state = { root, canvas, payload, callback, disposed: false, expanded: false };
    instances.set(root, state);
    active.add(state);
    if (window.IntersectionObserver) {
        state.observer = new IntersectionObserver(entries => {
            if (entries.some(entry => entry.isIntersecting)) schedule(state);
            else cancelScheduled(state);
        });
        state.observer.observe(canvas);
    } else {
        schedule(state);
    }
}
export function update(root, payload) {
    const state = instances.get(root);
    if (!state) return;
    state.payload = payload;
    state.engine?.update(root, payload);
}
export async function control(root, action) {
    const state = instances.get(root);
    if (!state) return;
    const engine = await load(state);
    if (engine && connected(state)) engine.control(root, action);
}
export function expand(root, expanded = false) {
    const state = instances.get(root);
    if (!state) return;
    state.expanded = expanded;
    // An explicit Open map action bypasses the idle delay.
    if (expanded) return load(state);
    state.engine?.expand(root, false);
}
export async function retry(root) {
    const state = instances.get(root);
    if (!state) return;
    const engine = await load(state);
    if (engine && connected(state)) await engine.retry(root);
}
export function focusSheet(root, sheet) {
    const state = instances.get(root);
    if (!state || !sheet) return;
    // Lesson details remain usable while the map engine is still loading.
    state.focusBefore ??= document.activeElement;
    sheet.querySelector('button,a')?.focus();
    sheet.onkeydown = event => {
        if (event.key !== 'Tab') return;
        const nodes = [...sheet.querySelectorAll('button:not(:disabled),a[href],input')];
        if (!nodes.length) return;
        if (event.shiftKey && document.activeElement === nodes[0]) { event.preventDefault(); nodes.at(-1).focus(); }
        if (!event.shiftKey && document.activeElement === nodes.at(-1)) { event.preventDefault(); nodes[0].focus(); }
    };
}
export function restoreFocus(root) {
    const state = instances.get(root);
    if (!state) return;
    (state.focusBefore?.isConnected ? state.focusBefore : root.querySelector('.vn-heading button,a'))?.focus();
    state.focusBefore = null;
}
export function dispose(root) {
    const state = instances.get(root);
    if (!state) return;
    state.disposed = true;
    cancelScheduled(state);
    state.observer?.disconnect();
    state.engine?.dispose(root);
    active.delete(state);
    instances.delete(root);
}
// DOM removal can precede .NET disposal or completion of an import/interop.
new MutationObserver(records => {
    if (active.size && records.some(record => record.removedNodes.length))
        for (const state of active) if (!state.root.isConnected) dispose(state.root);
}).observe(document.body, { childList: true, subtree: true });
