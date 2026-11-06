import maplibregl from 'maplibre-gl';
import 'maplibre-gl/dist/maplibre-gl.css';
import '../Styles/vietnam-mobile.css';
import { baseStyle, loadLandmarks, adventureBounds } from './vietnam-map-data.js';
import { addJourneyLayers, updateJourneyLayers } from './vietnam-map-layers.js';

const instances = new WeakMap();
const bounds = adventureBounds;
const isDark = () => document.documentElement.dataset.theme === 'dark';
function notify(state, method, ...args) { if (!state.disposed) return state.callback.invokeMethodAsync(method, ...args).catch(() => {}); }
function paintTheme(state) {
    if (!state.map?.isStyleLoaded()) return;
    for (const layer of baseStyle(isDark()).layers) for (const [key, value] of Object.entries(layer.paint || {})) state.map.setPaintProperty(layer.id, key, value);
}
function fit(state) {
    if (!state.map || state.disposed || state.canvas.clientWidth < 1 || state.canvas.clientHeight < 1) return;
    state.map.resize();
    state.map.setMinZoom(0);
    const camera=state.map.cameraForBounds(bounds,{padding:0});
    const ratio=state.canvas.clientWidth / Math.max(1,state.canvas.clientHeight);
    const coverZoom=Math.min(state.map.getMaxZoom(),Math.max(0,camera.zoom+Math.abs(Math.log2(ratio/(12/8)))));
    state.map.jumpTo({center:[6,4],zoom:coverZoom});
    state.map.setMinZoom(coverZoom);
    const visible=state.map.getBounds();
    state.sceneViewport={west:visible.getWest(),east:visible.getEast(),south:visible.getSouth(),north:visible.getNorth()};
    if(state.loaded)apply(state);
}
function scheduleLabels(state) {
    if (state.disposed || state.labelFrame) return;
    state.labelFrame = requestAnimationFrame(() => {
        state.labelFrame = null;
        if (!state.disposed) layoutLabels(state);
    });
}
async function start(state) {
    if (state.disposed || state.map || state.starting || state.failed || !state.media.matches) return;
    state.starting = true;
    try {
        // Fetch metadata while WebGL initializes and loads the landscape image.
        const geography = loadLandmarks();
        geography.catch(() => {});
        state.map = new maplibregl.Map({ container: state.canvas, style: baseStyle(isDark()),
            center: [6, 4], zoom: 4, minZoom: 0, maxZoom: 12, maxBounds:bounds,
            cooperativeGestures: true, attributionControl: { compact: true, customAttribution: 'Phong cảnh minh họa · SoftSync · <a href="https://www.wikidata.org/wiki/Wikidata:Licensing">Địa danh: Wikidata CC0</a>' },
            dragRotate: false, pitchWithRotate: false, touchPitch: false });
        state.map.scrollZoom.disable(); // Normal page scrolling always remains available in the preview.
        const recordViewport = () => {
            state.canvas.dataset.mapZoom = state.map.getZoom().toFixed(4);
            const center = state.map.getCenter();
            state.canvas.dataset.mapCenter = `${center.lng.toFixed(5)},${center.lat.toFixed(5)}`;
        };
        state.map.on('moveend', recordViewport);
        state.map.on('moveend',()=>scheduleLabels(state));
        state.map.on('resize',()=>scheduleLabels(state));
        state.timeout = setTimeout(() => notify(state, 'MapStatus', false), 15000);
        state.map.once('load', async () => {
            if (state.disposed) return;
            try {
            state.geography = await geography;
            if (state.disposed) return;
            await addJourneyLayers(state.map, state.geography);
            state.loaded = true;
            clearTimeout(state.timeout);
            fit(state); paintTheme(state);
            state.root.dataset.mapReady = 'true';
            notify(state, 'MapStatus', true);
            } catch (error) {
                if (state.disposed) return;
                clearTimeout(state.timeout);
                state.failed = true;
                console.warn('Vietnam map unavailable:', error.message);
                notify(state, 'MapStatus', false);
            }
        });
        state.map.on('error', event => { if (!state.loaded) console.warn('Vietnam map:', event.error?.message); });
    } catch (error) { state.failed = true; console.warn('Vietnam map unavailable:', error.message); notify(state, 'MapStatus', false); }
    finally { state.starting = false; }
}
function apply(state) {
    if (!state.loaded || state.disposed) return;
    state.data = updateJourneyLayers(state.map, state.payload, state.geography, state.sceneViewport);
    state.markers?.forEach(x=>x.marker.remove());
    state.markers=state.data.features.map(feature=>{
        const p=feature.properties, button=document.createElement('button');
        button.type='button';button.className=`vn-adventure-stop ${p.state}`;button.dataset.stopId=p.id;
        button.setAttribute('aria-label',`${state.payload.vietnamese?p.place:p.placeEn} · ${p.title} · ${p.status}`);
        const dot=document.createElement('span');dot.className='vn-stop-dot';dot.textContent=p.state==='completed'?'✓':p.state==='locked'?'🔒':'●';dot.setAttribute('aria-hidden','true');
        const label=document.createElement('span');label.className='vn-stop-label';label.textContent=state.payload.vietnamese?p.place:p.placeEn;
        button.append(dot,label);
        button.addEventListener('click',event=>{event.stopPropagation();state.focusBefore=button;if(button.clusterIds?.length>1)notify(state,'ShowCluster',button.clusterIds);else notify(state,'ShowStop',p.id)});
        const marker=new maplibregl.Marker({element:button,anchor:'center'}).setLngLat(feature.geometry.coordinates).addTo(state.map);
        return {marker,button,label,dot,feature};
    });
    // Retain discovery of existing places without turning the illustration into
    // a geographic map. Positions below are artwork slots, never source coordinates.
    if(!state.root.classList.contains('vn-compact')) {
        const positions=[[2,6.5],[5,5],[9,2],[8,6.5],[10,4.5]];
        state.geography.features.forEach((feature,index)=>{
            if(state.payload.stops.some(s=>s.landmarkId===feature.properties.id))return;
            const p=feature.properties,button=document.createElement('button');
            button.type='button';button.className='vn-adventure-stop vn-adventure-place';
            const label=document.createElement('span');label.className='vn-stop-label';label.textContent=state.payload.vietnamese?p.name:p.nameEn;
            button.append(label);button.setAttribute('aria-label',label.textContent);
            button.addEventListener('click',e=>{e.stopPropagation();state.focusBefore=button;notify(state,'ShowPlace',label.textContent,p.source)});
            let [x,y]=positions[index%positions.length];
            if(state.sceneViewport){const v=state.sceneViewport;x=v.west+x/12*(v.east-v.west);y=v.south+y/8*(v.north-v.south);}
            const marker=new maplibregl.Marker({element:button}).setLngLat([x,y]).addTo(state.map);
            state.markers.push({marker,button,label});
        });
    }
    try { localStorage.setItem(`ss-journey-skill:${state.payload.userId}`, state.payload.selectedKey); } catch { /* Private mode. */ }
    scheduleLabels(state);
}
function layoutLabels(state) {
    if(!state.markers)return;
    const accepted=[], stage=state.canvas.getBoundingClientRect();
    for(const entry of state.markers) {
        entry.button.style.visibility='';entry.button.clusterIds=entry.feature?[entry.feature.properties.id]:[];
        if(entry.dot)entry.dot.textContent=entry.feature.properties.state==='completed'?'✓':entry.feature.properties.state==='locked'?'🔒':'●';
        entry.button.classList.remove('is-condensed');
        const r=entry.button.getBoundingClientRect();
        if(r.left<stage.left+5||r.right>stage.right-5||accepted.some(a=>r.left<a.right+5&&r.right>a.left-5&&r.top<a.bottom+5&&r.bottom>a.top-5))entry.button.classList.add('is-condensed');
        const actual=entry.button.getBoundingClientRect();
        const neighbor=accepted.find(a=>actual.left<a.right+4&&actual.right>a.left-4&&actual.top<a.bottom+4&&actual.bottom>a.top-4);
        if(neighbor && entry.feature && neighbor.entry.feature){
            entry.button.style.visibility='hidden';
            neighbor.entry.button.clusterIds.push(entry.feature.properties.id);
            neighbor.entry.dot.textContent=String(neighbor.entry.button.clusterIds.length);
        } else accepted.push({...actual.toJSON(),entry});
    }
}
export async function mount(root, canvas, payload, callback) {
    if (instances.has(root)) return update(root, payload);
    const state = { root, canvas, payload, callback, media: matchMedia('(min-width:0px)'), disposed: false };
    instances.set(root, state);
    state.onResize = () => { if (!state.map) start(state); else if(state.loaded)fit(state); else state.map.resize(); };
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
        if (feature) state.map.flyTo({ center: feature.geometry.coordinates, zoom: Math.max(state.map.getMinZoom()+1,6), duration: matchMedia('(prefers-reduced-motion:reduce)').matches ? 0 : 500 });
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
export async function retry(root) { const state = instances.get(root); if (!state) return; clearTimeout(state.timeout); delete root.dataset.mapReady; state.map?.remove(); state.map = null; state.loaded = false; state.failed = false; await start(state); }
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
export function dispose(root) { const state = instances.get(root); if (!state) return; state.disposed = true; clearTimeout(state.timeout); cancelAnimationFrame(state.labelFrame); state.resizeObserver.disconnect(); state.themeObserver.disconnect(); state.media.removeEventListener('change', state.onMedia); root.removeEventListener('keydown', state.onKey); state.map?.remove(); instances.delete(root); }
