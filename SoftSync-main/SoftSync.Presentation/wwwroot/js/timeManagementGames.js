export async function requestNotifications(){ return !('Notification' in window) ? 'unsupported' : await Notification.requestPermission(); }
export function notify(title, body){ if('Notification' in window && Notification.permission==='granted') new Notification(title,{body,icon:'/images/syncy-fish.png'}); }
