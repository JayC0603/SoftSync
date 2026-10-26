(function () {
    // Enhanced navigation must not start Blazor or register handlers twice.
    if (window.ssBlazorStarted) return;
    window.ssBlazorStarted = true;

    var recovery = null;
    var editedFields = new Set();
    document.addEventListener('input', trackEdit, true);
    document.addEventListener('change', trackEdit, true);
    function trackEdit(event) {
        var field = event.target;
        if (field.matches('input:not([type="hidden"]):not([type="button"]):not([type="submit"]), textarea, select, [contenteditable="true"]'))
            editedFields.add(field);
    }
    function hasDraft() {
        // Disconnected fields belong to a page the user has already left.
        for (var field of editedFields) {
            if (field.isConnected) return true;
            editedFields.delete(field);
        }
        // Also protect values filled by password managers without input events.
        return Array.from(document.querySelectorAll('input:not([type="hidden"]):not([type="button"]):not([type="submit"]), textarea'))
            .some(function (field) {
                return field.type === 'checkbox' || field.type === 'radio'
                    ? field.checked !== field.defaultChecked : field.value !== field.defaultValue;
            });
    }
    function connectionUp() {
        if (recovery) recovery.cancelled = true;
        recovery = null;
    }
    async function freshSession(process) {
        process.finished = true;
        if (hasDraft()) { return; }
        if (document.visibilityState === 'hidden') { return; }
        // A broken deployment must not trap the browser in a reload loop.
        try {
            var last = Number(sessionStorage.getItem('ss-connection-reload') || 0);
            if (Date.now() - last < 60000) { return; }
        } catch { return; }
        try {
            var response = await fetch('/health', { cache: 'no-store', credentials: 'same-origin', signal: AbortSignal.timeout(5000) });
            if (!response.ok || (await response.json()).status !== 'ok') throw new Error('Server not ready');
            if (process.cancelled) return;
            // Recheck after the request: the user may have started editing.
            if (hasDraft()) { return; }
            if (document.visibilityState === 'hidden') { return; }
            sessionStorage.setItem('ss-connection-reload', String(Date.now()));
            location.reload();
        } catch {
            // Recovery stays silent; retry when the connection becomes available.
        }
    }
    function connectionDown() {
        if (recovery) return;
        var process = { cancelled: false, finished: false };
        recovery = process;
        process.task = (async function () {
            for (var delay of [0, 1000, 3000]) {
                if (delay) await new Promise(function (resolve) { setTimeout(resolve, delay); });
                if (process.cancelled) return;
                try {
                    var resumed = await Blazor.reconnect();
                    if (process.cancelled) return;
                    if (resumed) { connectionUp(); return; }
                    // A reachable server rejected the old circuit. Retrying the
                    // same circuit cannot restore an instance that was replaced.
                    await freshSession(process);
                    return;
                } catch { /* A network outage can still recover on the next attempt. */ }
            }
            process.finished = true;
            // Recovery stays silent; retry when the connection becomes available.
        })();
    }
    function retryWhenAvailable() {
        if (document.visibilityState === 'hidden' || !recovery?.finished) return;
        recovery.cancelled = true;
        recovery = null;
        connectionDown();
    }
    window.addEventListener('online', retryWhenAvailable);
    document.addEventListener('visibilitychange', retryWhenAvailable);

    Blazor.start({
        circuit: {
            configureSignalR: function (builder) {
                // Vercel pins a WebSocket to one instance. A separate negotiate
                // request can reach another instance and lose its connection ID.
                builder.withUrl('/_blazor', { transport: 1, skipNegotiation: true });
                builder.withServerTimeout(60000).withKeepAliveInterval(15000);
            },
            reconnectionHandler: {
                onConnectionDown: connectionDown,
                onConnectionUp: connectionUp
            }
        }
    }).then(function () {
        // Enhanced-navigation events are available after Blazor has started.
        if (window.ssBindPageNavigation) window.ssBindPageNavigation();
    }).catch(function (error) {
        console.error('SoftSync could not start its interactive connection.', error);
        // Keep the rendered page and native links available without a reload banner.
    });
})();
