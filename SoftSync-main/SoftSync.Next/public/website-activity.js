(() => {
    if (window.top !== window.self) return;
    if (window.ssWebsiteActivity) return;
    window.ssWebsiteActivity = true;
    let id = null, sequence = 0, lastInteraction = Date.now(), stopped = false, pending = false;
    const active = () => document.visibilityState === 'visible' && Date.now() - lastInteraction < 60000;
    for (const name of ['pointerdown', 'pointermove', 'keydown', 'scroll', 'touchstart'])
        window.addEventListener(name, () => { lastInteraction = Date.now(); }, { passive: true });
    async function pulse() {
        if (stopped || pending) return;
        pending = true;
        try {
            if (!id) {
                const response = await fetch('/api/visits/', { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify({ active: active(), sequence: 0 }), cache: 'no-store' });
                if (response.ok) id = (await response.json()).id;
            } else {
                const response = await fetch(`/api/visits/${id}`, { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify({ active: active(), sequence: ++sequence }), cache: 'no-store' });
                if (response.status === 404) { id = null; sequence = 0; }
            }
        } catch { /* Retry on the next heartbeat without interrupting navigation. */ }
        finally { pending = false; if (stopped) end(); }
    }
    function end() {
        if (!id) return;
        navigator.sendBeacon(`/api/visits/${id}`, new Blob([JSON.stringify({ sequence: ++sequence, active: false, end: true })], { type: 'application/json' }));
        id = null;
    }
    document.addEventListener('visibilitychange', () => { void pulse(); });
    window.addEventListener('pagehide', () => { stopped = true; end(); });
    window.addEventListener('pageshow', () => { stopped = false; void pulse(); });
    setInterval(pulse, 20000);
    void pulse();
})();
