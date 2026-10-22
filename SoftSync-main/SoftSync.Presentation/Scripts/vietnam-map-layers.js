import { checkpointData, routeData } from './vietnam-map-data.js';
export async function addJourneyLayers(map) {
 map.addSource('journey-route', {type:'geojson',data:{type:'FeatureCollection',features:[]}});
 map.addLayer({id:'journey-route-halo',type:'line',source:'journey-route',paint:{'line-color':'#ffffff','line-opacity':.9,'line-width':5},layout:{'line-join':'round','line-cap':'round'}});
 map.addLayer({id:'journey-route-line',type:'line',source:'journey-route',paint:{'line-color':'#8560e8','line-width':2,'line-dasharray':[2,3]},layout:{'line-join':'round','line-cap':'round'}});
}
export function updateJourneyLayers(map,payload,geography,viewport) {
 const data=checkpointData(payload.stops,geography);
 if(viewport)for(const feature of data.features){
  const [x,y]=feature.geometry.coordinates;
  feature.geometry.coordinates=[viewport.west+x/12*(viewport.east-viewport.west),viewport.south+y/8*(viewport.north-viewport.south)];
 }
 map.getSource('journey-route').setData(routeData(data));return data;
}
