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
    function showNotice(state) {
        var notice = document.getElementById('ss-connection-notice');
        if (!notice) return;
        var english = document.documentElement.lang === 'en';
        var messages = english ? {
            retrying: 'Connection interrupted. Reconnecting…',
            draft: 'The session expired. Your unsaved input is still here. Reload when you are ready.',
            failed: 'Unable to restore the connection. Check your network, then reload.',
            expired: 'The session expired. Reload to continue.'
        } : {
            retrying: 'Kết nối bị gián đoạn. Đang kết nối lại…',
            draft: 'Phiên đã hết hạn. Nội dung chưa lưu vẫn được giữ trên trang. Tải lại khi bạn đã sẵn sàng.',
            failed: 'Chưa thể khôi phục kết nối. Kiểm tra mạng rồi tải lại trang.',
            expired: 'Phiên đã hết hạn. Tải lại để tiếp tục.'
        };
        notice.querySelector('p').textContent = messages[state];
        var button = notice.querySelector('button');
        button.textContent = english ? 'Reload page' : 'Tải lại trang';
        button.hidden = state === 'retrying';
        notice.hidden = false;
    }
    function connectionUp() {
        if (recovery) recovery.cancelled = true;
        recovery = null;
        var notice = document.getElementById('ss-connection-notice');
        if (notice) notice.hidden = true;
    }
    async function freshSession(process) {
        process.finished = true;
        if (hasDraft()) { showNotice('draft'); return; }
        if (document.visibilityState === 'hidden') { showNotice('expired'); return; }
        // A broken deployment must not trap the browser in a reload loop.
        try {
            var last = Number(sessionStorage.getItem('ss-connection-reload') || 0);
            if (Date.now() - last < 60000) { showNotice('expired'); return; }
        } catch { showNotice('expired'); return; }
        try {
            var response = await fetch('/health', { cache: 'no-store', credentials: 'same-origin', signal: AbortSignal.timeout(5000) });
            if (!response.ok || (await response.json()).status !== 'ok') throw new Error('Server not ready');
            if (process.cancelled) return;
            // Recheck after the request: the user may have started editing.
            if (hasDraft()) { showNotice('draft'); return; }
            if (document.visibilityState === 'hidden') { showNotice('expired'); return; }
            sessionStorage.setItem('ss-connection-reload', String(Date.now()));
            location.reload();
        } catch {
            if (!process.cancelled) showNotice('failed');
        }
    }
    function connectionDown() {
        if (recovery) return;
        var process = { cancelled: false, finished: false };
        recovery = process;
        showNotice('retrying');
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
            if (!process.cancelled) showNotice('failed');
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
        // Keep server-rendered links available and expose the existing retry UI.
        var notice = document.getElementById('blazor-error-ui');
        if (notice) notice.style.display = 'block';
    });
})();
