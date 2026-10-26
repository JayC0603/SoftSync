(() => {
    const tracked = /^\/(roadmap(?:\/|$)|courses\/|assessment(?:\/|$)|practice(?:\/|$)|roleplay(?:\/|$))/i;
    let path = "", sessionId = "", sequence = 0, timer = 0, busy = false, syncing = false;
    const post = (url, body, keepalive = false) => fetch(url, {
        method: "POST", credentials: "same-origin", keepalive,
        headers: { "Content-Type": "application/json" }, body: JSON.stringify(body)
    });
    async function heartbeat(force = false) {
        if (!sessionId || busy || (!force && document.visibilityState !== "visible")) return;
        busy = true;
        try { await post(`/api/learning/sessions/${sessionId}/heartbeat`, { sessionId, sequence: ++sequence }); }
        catch { /* A missed ping earns no time; the server bounds each credited interval. */ }
        finally { busy = false; }
    }
    async function stop() {
        clearInterval(timer); timer = 0;
        if (!sessionId) return;
        const oldId = sessionId; sessionId = "";
        try { await heartbeatOld(oldId); } catch { }
        try { await post(`/api/learning/sessions/${oldId}/end`, {}, true); } catch { }
    }
    async function heartbeatOld(id) {
        try { await post(`/api/learning/sessions/${id}/heartbeat`, { sessionId: id, sequence: ++sequence }, true); } catch { }
    }
    async function sync() {
        if (syncing) return;
        syncing = true;
        try {
        const next = location.pathname;
        if (!tracked.test(next)) { path = next; await stop(); return; }
        if (path === next && sessionId) return;
        await stop(); path = next;
        try {
            const response = await post("/api/learning/sessions/", {
                activityType: next.startsWith("/assessment") ? "assessment" : next.startsWith("/courses/") ? "lesson" : next.startsWith("/practice") ? "practice" : next.startsWith("/roleplay") ? "roleplay" : "roadmap",
                resourceId: next.slice(0, 120)
            });
            if (!response.ok) return;
            const value = await response.json(); sessionId = value.sessionId; sequence = 0;
            timer = setInterval(heartbeat, 20000);
        } catch { /* Activity analytics must never block learning. */ }
        } finally {
            syncing = false;
            if (path !== location.pathname) sync();
        }
    }
    document.addEventListener("visibilitychange", () => { if (document.visibilityState === "hidden") heartbeat(true); });
    window.addEventListener("popstate", sync);
    window.addEventListener("beforeunload", () => { if (sessionId) { heartbeatOld(sessionId); post(`/api/learning/sessions/${sessionId}/end`, {}, true); } });
    if (window.Blazor) Blazor.addEventListener("enhancedload", sync);
    setTimeout(sync, 0);
})();
