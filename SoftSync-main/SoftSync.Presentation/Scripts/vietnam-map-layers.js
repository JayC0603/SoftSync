import { checkpointData, routeData } from './vietnam-map-data.js';

const circleColor = ['match', ['get', 'state'], 'completed', '#25bd84', 'locked', '#c9d2e2', '#8156ff'];
export async function addJourneyLayers(map, geography) {
    const bubble = document.createElement('canvas'); bubble.width = 120; bubble.height = 60;
    const ctx = bubble.getContext('2d'); ctx.fillStyle = '#ffffff'; ctx.strokeStyle = '#cbbaff'; ctx.lineWidth = 2;
    ctx.beginPath(); ctx.roundRect(1, 1, 118, 58, 10); ctx.fill(); ctx.stroke();
    map.addImage('checkpoint-bubble', ctx.getImageData(0, 0, 120, 60), { stretchX: [[12, 108]], stretchY: [[12, 48]], content: [8, 8, 112, 52] });
    const art = {
        pagoda: '<path d="M18 34h28v23H18z" fill="#ffefc8"/><path d="M10 34h44L40 23H24zM14 22h36L38 12H26zM22 11h20L32 3z" fill="#c66a42" stroke="#8a503d" stroke-width="2"/><path d="M28 57V42h8v15" fill="#6e8698"/><path d="M21 39v7m22-7v7" stroke="#cfa57a" stroke-width="3"/>',
        bridge: '<path d="M7 44h50M12 48V32m40 16V32" stroke="#d7a04c" stroke-width="5"/><path d="M12 32Q32 7 52 32" fill="none" stroke="#f1ca6a" stroke-width="6"/><path d="M19 43V28m8 15V23m10 20V23m8 20V28" stroke="#f5db9d" stroke-width="2"/>',
        skyline: '<path d="M11 57V28h12v29m4 0V11h15v46m4 0V34h10v23" fill="#75b9dc" stroke="#3e7da6" stroke-width="2"/><path d="M33 11V3m-2 16h8m-8 7h8m-8 7h8m-8 7h8m-8 7h8" stroke="#e4f5ff" stroke-width="2"/>',
        trophy: '<path d="M21 10h22v18c0 16-22 16-22 0z" fill="#ffd252" stroke="#e3a429" stroke-width="2"/><path d="M21 14H11v7c0 9 8 12 13 12m19-19h10v7c0 9-8 12-13 12" fill="none" stroke="#e3a429" stroke-width="4"/><path d="M32 39v12m-12 6h24l-4-6H24z" stroke="#e3a429" fill="#ffd252" stroke-width="3"/>'
    };
    await Promise.all(Object.entries(art).map(async ([id, paths]) => {
        const img = new Image();
        img.src = `data:image/svg+xml;charset=utf-8,${encodeURIComponent(`<svg xmlns="http://www.w3.org/2000/svg" width="64" height="64" viewBox="0 0 64 64">${paths}</svg>`)}`;
        await img.decode(); map.addImage(`landmark-${id}`, img);
    }));
    map.addSource('landmarks', { type: 'geojson', data: geography });
    map.addSource('checkpoints', { type: 'geojson', data: { type: 'FeatureCollection', features: [] }, cluster: true, clusterRadius: 24, clusterMaxZoom: 11 });
    map.addSource('journey-route', { type: 'geojson', data: { type: 'FeatureCollection', features: [] } });
    map.addLayer({ id: 'journey-route-halo', type: 'line', source: 'journey-route', paint: { 'line-color': '#ffffff', 'line-opacity': .9, 'line-width': 7 }, layout: { 'line-join': 'round', 'line-cap': 'round' } });
    map.addLayer({ id: 'journey-route-line', type: 'line', source: 'journey-route', paint: { 'line-color': '#8861f9', 'line-width': 3, 'line-dasharray': [2, 2] }, layout: { 'line-join': 'round', 'line-cap': 'round' } });
    map.addLayer({ id: 'island-points', type: 'circle', source: 'landmarks', filter: ['==', ['get', 'kind'], 'archipelago'], paint: { 'circle-radius': 4, 'circle-color': '#f4e4a9', 'circle-stroke-color': '#378cb2', 'circle-stroke-width': 1 } });
    map.addLayer({ id: 'island-names', type: 'symbol', source: 'landmarks', filter: ['==', ['get', 'kind'], 'archipelago'], layout: { 'text-field': ['get', 'name'], 'text-size': 10, 'text-font': ['Noto Sans Regular'], 'text-offset': [0, 1.3], 'text-max-width': 9 }, paint: { 'text-color': '#285777', 'text-halo-color': '#d5f0ff', 'text-halo-width': 2 } });
    map.addLayer({ id: 'landmark-icons', type: 'symbol', source: 'landmarks', filter: ['==', ['get', 'kind'], 'city'], layout: { 'icon-image': ['concat', 'landmark-', ['get', 'decoration']], 'icon-size': ['interpolate', ['linear'], ['zoom'], 3, .7, 8, 1.1], 'icon-offset': [-30, -45], 'icon-allow-overlap': true } });
    map.addLayer({ id: 'city-names', type: 'symbol', source: 'landmarks', filter: ['==', ['get', 'kind'], 'city'], layout: { 'text-field': ['get', 'name'], 'text-font': ['Noto Sans Regular'], 'text-size': 10, 'text-offset': [-1, -2.3], 'text-allow-overlap': false }, paint: { 'text-color': '#fff', 'text-halo-color': '#276c8f', 'text-halo-width': 3 } });
    map.addLayer({ id: 'checkpoint-glow', type: 'circle', source: 'checkpoints', filter: ['all', ['!', ['has', 'point_count']], ['==', ['get', 'state'], 'current']], paint: { 'circle-radius': 24, 'circle-color': '#986dff', 'circle-opacity': .3, 'circle-blur': .3 } });
    map.addLayer({ id: 'checkpoint-circles', type: 'circle', source: 'checkpoints', filter: ['!', ['has', 'point_count']], paint: { 'circle-radius': 16, 'circle-color': circleColor, 'circle-stroke-width': 3, 'circle-stroke-color': '#fff' } });
    map.addLayer({ id: 'checkpoint-numbers', type: 'symbol', source: 'checkpoints', filter: ['!', ['has', 'point_count']], layout: { 'text-field': ['to-string', ['get', 'order']], 'text-font': ['Noto Sans Bold'], 'text-size': 15, 'text-allow-overlap': true }, paint: { 'text-color': '#fff' } });
    map.addLayer({ id: 'checkpoint-titles', type: 'symbol', source: 'checkpoints', filter: ['!', ['has', 'point_count']], layout: { 'text-field': ['concat', ['get', 'title'], '\n', ['get', 'status']], 'text-font': ['Noto Sans Regular'], 'text-size': 10, 'text-anchor': 'left', 'text-offset': [2.4, 0], 'text-max-width': 12, 'text-allow-overlap': false, 'icon-image': 'checkpoint-bubble', 'icon-text-fit': 'both', 'icon-text-fit-padding': [6, 7, 6, 7] }, paint: { 'text-color': '#273355' } });
    map.addLayer({ id: 'boss-trophy', type: 'symbol', source: 'checkpoints', filter: ['all', ['!', ['has', 'point_count']], ['==', ['get', 'boss'], true]], layout: { 'icon-image': 'landmark-trophy', 'icon-size': .65, 'icon-offset': [-42, 10], 'icon-allow-overlap': true } });
    map.addLayer({ id: 'checkpoint-clusters', type: 'circle', source: 'checkpoints', filter: ['has', 'point_count'], paint: { 'circle-color': '#8061db', 'circle-radius': 20, 'circle-stroke-width': 3, 'circle-stroke-color': '#fff' } });
    map.addLayer({ id: 'checkpoint-cluster-count', type: 'symbol', source: 'checkpoints', filter: ['has', 'point_count'], layout: { 'text-field': ['concat', ['to-string', ['get', 'point_count']], ' mốc'], 'text-font': ['Noto Sans Bold'], 'text-size': 11, 'text-allow-overlap': true }, paint: { 'text-color': '#fff' } });
}
export function updateJourneyLayers(map, payload, geography) {
    const data = checkpointData(payload.stops, geography);
    map.getSource('checkpoints').setData(data);
    map.getSource('journey-route').setData(routeData(data));
    map.setLayoutProperty('island-names', 'text-field', ['get', payload.vietnamese ? 'name' : 'nameEn']);
    map.setLayoutProperty('city-names', 'text-field', ['get', payload.vietnamese ? 'name' : 'nameEn']);
    map.setLayerZoomRange('checkpoint-titles', payload.stops.length <= 4 ? 0 : 6, 24);
    return data;
}
