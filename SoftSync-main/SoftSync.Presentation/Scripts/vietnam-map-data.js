// Geographic inputs are independent of lesson state. Coordinates and licenses live with the GeoJSON.
let landmarksPromise;
export function loadLandmarks() {
    landmarksPromise ??= fetch('/data/vietnam-landmarks.json').then(response => {
        if (!response.ok) throw new Error('Landmark data unavailable');
        return response.json();
    }).catch(error => { landmarksPromise = null; throw error; });
    return landmarksPromise;
}
// Adventure coordinates are a view projection, not real-world geographic positions.
// The original landmark and lesson IDs are retained for detail/navigation/authorization.
export const adventureBounds = [[0, 0], [12, 8]];
export function adventurePoint(index, count) {
    if(count<=5) {
        const t=index/Math.max(1,count-1);
        return [3+5.5*t,6-3*t];
    }
    const rows = Math.max(1, Math.ceil(count / 3));
    const row = Math.floor(index / 3), col = index % 3;
    const x = (row % 2 ? 3 - col : col + 1) * 3;
    return [x, 6.7 - row * (5.4 / Math.max(1, rows - 1)) - col * .5];
}
export function checkpointData(stops, geography) {
    const places = new Map(geography.features.map(feature => [feature.properties.id, feature]));
    return { type: 'FeatureCollection', features: stops.flatMap((stop, index) => {
        const place = places.get(stop.landmarkId);
        return place ? [{ type: 'Feature', id: stop.id, geometry: {type:'Point',coordinates:adventurePoint(index, stops.length)},
            properties: { ...stop, place: place.properties.name, placeEn: place.properties.nameEn, source: place.properties.source, label: `${stop.order}`, short: String(stop.order) } }] : [];
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
        version: 8,
        sources: { landscape: { type: 'image', url: '/images/journey/vietnam-adventure-coast-v1.webp',
            coordinates: [[0,8],[12,8],[12,0],[0,0]] } },
        layers: [
            { id: 'land', type: 'background', paint: { 'background-color': dark ? '#162f40' : '#b6dced' } },
            { id: 'adventure-landscape', type: 'raster', source: 'landscape', paint: { 'raster-brightness-max': dark ? .72 : 1, 'raster-fade-duration':0 } }
        ]
    };
}
/* Geographic source is retained independently in vietnam-landmarks.json.
   The adventure illustration intentionally makes no boundary/coordinate claims. */
