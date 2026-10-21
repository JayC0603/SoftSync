import maplibregl from 'maplibre-gl';
import 'maplibre-gl/dist/maplibre-gl.css';
import '../Styles/vietnam-mobile.css';
import { baseStyle, loadLandmarks } from './vietnam-map-data.js';
import { addJourneyLayers, updateJourneyLayers } from './vietnam-map-layers.js';

const instances = new WeakMap();
const bounds = [[102, 8], [116, 24]];
const isDark = () => document.documentElement.dataset.theme === 'dark';
function notify(state, method, ...args) { if (!state.disposed) return state.callback.invokeMethodAsync(method, ...args).catch(() => {}); }
function paintTheme(state) {
    if (!state.map?.isStyleLoaded()) return;
    for (const layer of baseStyle(isDark()).layers) for (const [key, value] of Object.entries(layer.paint || {})) state.map.setPaintProperty(layer.id, key, value);
}
function fit(state) {
    if (!state.map) return;
    state.map.resize();
    state.map.fitBounds(bounds, { padding: { top: 40, bottom: 28, left: 30, right: 45 }, duration: 0 });
}
async function start(state) {
    if (state.disposed || state.map || state.starting || state.failed || !state.media.matches) return;
    state.starting = true;
    try {
        state.geography = await loadLandmarks();
        if (state.disposed) return;
        state.map = new maplibregl.Map({ container: state.canvas, style: baseStyle(isDark()),
            center: [108, 16], zoom: 4, minZoom: 3, maxZoom: 14, maxBounds: [[97, 3], [122, 28]],
            cooperativeGestures: true, attributionControl: { compact: true },
            dragRotate: false, pitchWithRotate: false, touchPitch: false });
        state.map.scrollZoom.disable(); // Normal page scrolling always remains available in the preview.
        const recordViewport = () => {
            state.canvas.dataset.mapZoom = state.map.getZoom().toFixed(4);
            const center = state.map.getCenter();
            state.canvas.dataset.mapCenter = `${center.lng.toFixed(5)},${center.lat.toFixed(5)}`;
        };
        state.map.on('moveend', recordViewport);
        state.timeout = setTimeout(() => notify(state, 'MapStatus', false), 15000);
        state.map.once('load', async () => {
            if (state.disposed) return;
            await addJourneyLayers(state.map, state.geography);
            state.loaded = true;
            clearTimeout(state.timeout);
            apply(state); fit(state); paintTheme(state);
            notify(state, 'MapStatus', true);
        });
        state.map.on('error', event => { if (!state.loaded) console.warn('Vietnam map:', event.error?.message); });
        state.map.on('click', 'checkpoint-circles', event => { state.focusBefore = document.activeElement; notify(state, 'ShowStop', event.features[0].properties.id); });
        state.map.on('click', 'checkpoint-clusters', async event => {
            const feature = event.features[0];
            const leaves = await state.map.getSource('checkpoints').getClusterLeaves(feature.properties.cluster_id, 100, 0);
            state.focusBefore = document.activeElement;
            notify(state, 'ShowCluster', leaves.map(x => x.properties.id));
        });
        const showPlace = event => {
            if (state.map.queryRenderedFeatures(event.point, { layers: ['checkpoint-circles', 'checkpoint-clusters'] }).length) return;
            const place = event.features[0].properties;
            state.focusBefore = document.activeElement;
            notify(state, 'ShowPlace', state.payload.vietnamese ? place.name : place.nameEn, place.source);
        };
        for (const id of ['landmark-icons', 'island-points', 'city-names', 'island-names']) state.map.on('click', id, showPlace);
        for (const id of ['checkpoint-circles', 'checkpoint-clusters', 'landmark-icons', 'island-points']) {
            state.map.on('mouseenter', id, () => { state.map.getCanvas().style.cursor = 'pointer'; });
            state.map.on('mouseleave', id, () => { state.map.getCanvas().style.cursor = ''; });
        }
    } catch (error) { state.failed = true; console.warn('Vietnam map unavailable:', error.message); notify(state, 'MapStatus', false); }
    finally { state.starting = false; }
}
function apply(state) {
    if (!state.loaded || state.disposed) return;
    state.data = updateJourneyLayers(state.map, state.payload, state.geography);
    try { localStorage.setItem(`ss-journey-skill:${state.payload.userId}`, state.payload.selectedKey); } catch { /* Private mode. */ }
}
export async function mount(root, canvas, payload, callback) {
    if (instances.has(root)) return update(root, payload);
    const state = { root, canvas, payload, callback, media: matchMedia('(max-width:900px)'), disposed: false };
    instances.set(root, state);
    state.onResize = () => { if (!state.map) start(state); else state.map.resize(); };
    state.resizeObserver = new ResizeObserver(state.onResize); state.resizeObserver.observe(canvas);
    state.onMedia = () => { if (state.media.matches) start(state); };
    state.media.addEventListener('change', state.onMedia);
    state.themeObserver = new MutationObserver(() => paintTheme(state));
    state.themeObserver.observe(document.documentElement, { attributes: true, attributeFilter: ['data-theme'] });
    state.onKey = event => { if (event.key === 'Escape' && root.classList.contains('is-expanded') && !root.querySelector('.vn-sheet')) notify(state, 'ExitExpanded'); };
    root.addEventListener('keydown', state.onKey);
    try {
        const saved = localStorage.getItem(`ss-journey-skill:${payload.userId}`);
        if (saved && saved !== payload.selectedKey && !new URL(location.href).searchParams.has('skill')) notify(state, 'SelectSavedCourse', saved);
    } catch { /* Private mode; the server-provided course remains selected. */ }
    await start(state);
}
export function update(root, payload) { const state = instances.get(root); if (!state) return; state.payload = payload; apply(state); }
export function control(root, action) {
    const state = instances.get(root); if (!state?.loaded) return;
    root.dataset.mapAction = action;
    if (action === 'zoomIn') state.map.zoomIn();
    if (action === 'zoomOut') state.map.zoomOut();
    if (action === 'reset') fit(state);
    if (action === 'current') {
        const feature = state.data.features.find(x => x.properties.state === 'current') || state.data.features[0];
        if (feature) state.map.flyTo({ center: feature.geometry.coordinates, zoom: 7, duration: matchMedia('(prefers-reduced-motion:reduce)').matches ? 0 : 500 });
    }
}
export function expand(root, expanded = false) {
    const state = instances.get(root); if (!state) return;
    // Expanded exploration permits one-finger pan; preview preserves page scrolling.
    if (state.map) {
        if (expanded) state.map.cooperativeGestures.disable();
        else state.map.cooperativeGestures.enable();
    }
    state.map?.scrollZoom.disable();
    requestAnimationFrame(() => { fit(state); root.querySelector('.vn-heading button')?.focus(); });
}
export async function retry(root) { const state = instances.get(root); if (!state) return; clearTimeout(state.timeout); state.map?.remove(); state.map = null; state.loaded = false; state.failed = false; await start(state); }
export function focusSheet(root, sheet) {
    const state = instances.get(root); if (!state) return;
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
export function restoreFocus(root) { const state = instances.get(root); if (!state) return; (state.focusBefore?.isConnected ? state.focusBefore : root.querySelector('.vn-heading button'))?.focus(); state.focusBefore = null; }
export function dispose(root) { const state = instances.get(root); if (!state) return; state.disposed = true; clearTimeout(state.timeout); state.resizeObserver.disconnect(); state.themeObserver.disconnect(); state.media.removeEventListener('change', state.onMedia); root.removeEventListener('keydown', state.onKey); state.map?.remove(); instances.delete(root); }
