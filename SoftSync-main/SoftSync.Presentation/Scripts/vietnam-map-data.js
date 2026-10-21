// Geographic inputs are independent of lesson state. Coordinates and licenses live with the GeoJSON.
let landmarksPromise;
export function loadLandmarks() {
    landmarksPromise ??= fetch('/data/vietnam-landmarks.json').then(response => {
        if (!response.ok) throw new Error('Landmark data unavailable');
        return response.json();
    }).catch(error => { landmarksPromise = null; throw error; });
    return landmarksPromise;
}
export function checkpointData(stops, geography) {
    const places = new Map(geography.features.map(feature => [feature.properties.id, feature]));
    return { type: 'FeatureCollection', features: stops.flatMap(stop => {
        const place = places.get(stop.landmarkId);
        return place ? [{ type: 'Feature', id: stop.id, geometry: place.geometry,
            properties: { ...stop, place: place.properties.name, label: `${stop.boss ? '🏆' : stop.order}  ${stop.title}`, short: String(stop.order) } }] : [];
    }) };
}
export function routeData(data) {
    const points = data.features.map(f => f.geometry.coordinates);
    const unique = points.filter((p, i) => !i || p[0] !== points[i - 1][0] || p[1] !== points[i - 1][1]);
    if (unique.length < 2) return { type: 'FeatureCollection', features: [] };
    // This decorative curve expresses learning order, never a navigable road.
    const line = [];
    for (let i = 1; i < unique.length; i++) {
        const [a, b] = [unique[i - 1], unique[i]];
        for (let n = 0; n <= 24; n++) {
            const t = n / 24;
            line.push([a[0] + (b[0] - a[0]) * t + Math.sin(t * Math.PI * 2) * .18,
                a[1] + (b[1] - a[1]) * t]);
        }
    }
    return { type: 'FeatureCollection', features: [{ type: 'Feature', properties: {}, geometry: { type: 'LineString', coordinates: line } }] };
}
export function baseStyle(dark) {
    return {
        version: 8, glyphs: 'https://tiles.openfreemap.org/fonts/{fontstack}/{range}.pbf',
        sources: { geography: { type: 'vector', url: 'https://tiles.openfreemap.org/planet',
            attribution: '<a href="https://openfreemap.org">OpenFreeMap</a> © <a href="https://openmaptiles.org">OpenMapTiles</a> © <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a>' } },
        layers: [
            { id: 'land', type: 'background', paint: { 'background-color': dark ? '#233b40' : '#d6ebc4' } },
            { id: 'forest', type: 'fill', source: 'geography', 'source-layer': 'landcover', paint: { 'fill-color': dark ? '#2b554e' : '#a4d4a3', 'fill-opacity': .65 } },
            { id: 'parks', type: 'fill', source: 'geography', 'source-layer': 'park', minzoom: 5, paint: { 'fill-color': dark ? '#365e51' : '#b1dcb0', 'fill-opacity': .5 } },
            { id: 'sea', type: 'fill', source: 'geography', 'source-layer': 'water', paint: { 'fill-color': dark ? '#142b4a' : '#8ed4f5' } },
            { id: 'rivers', type: 'line', source: 'geography', 'source-layer': 'waterway', minzoom: 6, paint: { 'line-color': dark ? '#427591' : '#78c6ec', 'line-width': ['interpolate', ['linear'], ['zoom'], 6, .7, 13, 3] } },
            { id: 'provinces', type: 'line', source: 'geography', 'source-layer': 'boundary', minzoom: 6,
                filter: ['all', ['>=', ['get', 'admin_level'], 4], ['!=', ['get', 'maritime'], 1]],
                paint: { 'line-color': dark ? '#628a83' : '#7daf9c', 'line-width': .65, 'line-dasharray': [3, 3] } },
            { id: 'places', type: 'symbol', source: 'geography', 'source-layer': 'place', minzoom: 7,
                filter: ['in', ['get', 'class'], ['literal', ['state', 'city', 'town']]],
                layout: { 'text-field': ['coalesce', ['get', 'name:vi'], ['get', 'name:latin'], ['get', 'name']], 'text-font': ['Noto Sans Regular'], 'text-size': 11, 'text-max-width': 8 },
                paint: { 'text-color': dark ? '#d5e9dc' : '#35574e', 'text-halo-color': dark ? '#203a35' : '#eaf5dc', 'text-halo-width': 2 } }
        ]
    };
}
