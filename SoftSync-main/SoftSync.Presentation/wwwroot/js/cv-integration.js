const connections = new Map();
export function mount(id, origin) {
    unmount(id);
    const frame = document.getElementById(id);
    if (!frame) return;
    const status = document.getElementById('cv-connection-status');
    let busy = false;
    const controller = new AbortController();
    const themeTokens = ['--ss-canvas', '--ss-surface', '--ss-surface-soft', '--ss-surface-accent', '--ss-text', '--ss-text-muted', '--ss-border', '--ss-primary', '--ss-primary-hover', '--ss-success', '--ss-warning', '--ss-danger', '--ss-info'];
    function sendTheme() {
        const root = document.documentElement;
        const styles = getComputedStyle(root);
        const tokens = Object.fromEntries(themeTokens.map(name => [name, styles.getPropertyValue(name).trim()]));
        frame.contentWindow.postMessage({ type: 'softsync-cv-theme', theme: root.dataset.theme === 'dark' ? 'dark' : 'light', reduceMotion: root.dataset.reduceMotion === '1', tokens }, origin);
    }
    async function sendSession() {
        if (busy || controller.signal.aborted) return;
        busy = true;
        try {
            const csrf = await fetch('/api/cv-integration/csrf', { credentials: 'same-origin', cache: 'no-store', signal: controller.signal });
            if (!csrf.ok || csrf.redirected) throw new Error('Phiên SoftSync đã hết hạn. Vui lòng đăng nhập lại.');
            const token = await csrf.json();
            const response = await fetch('/api/cv-integration/session', { method: 'POST', credentials: 'same-origin', cache: 'no-store', headers: { [token.header || 'RequestVerificationToken']: token.token }, signal: controller.signal });
            if (!response.ok || response.redirected) throw new Error(response.status === 401 ? 'Phiên SoftSync đã hết hạn. Vui lòng đăng nhập lại.' : 'Chưa thể kết nối dịch vụ CV. Vui lòng thử lại.');
            frame.contentWindow.postMessage({ type: 'softsync-cv-session', session: await response.json() }, origin);
            if (status) status.hidden = true;
        } catch (error) {
            if (controller.signal.aborted) return;
            const message = error instanceof Error ? error.message : 'Không thể kết nối dịch vụ CV.';
            frame.contentWindow.postMessage({ type: 'softsync-cv-error', message }, origin);
            if (status) { status.hidden = false; status.textContent = message; }
        } finally { busy = false; }
    }
    const listener = event => {
        if (event.origin !== origin || event.source !== frame.contentWindow) return;
        if (event.data?.type === 'softsync-cv-ready') { sendTheme(); void sendSession(); }
    };
    window.addEventListener('message', listener);
    const ready = () => { sendTheme(); frame.contentWindow.postMessage({ type: 'softsync-cv-host-ready' }, origin); };
    const observer = new MutationObserver(sendTheme);
    observer.observe(document.documentElement, { attributes: true, attributeFilter: ['data-theme', 'data-reduce-motion', 'style'] });
    frame.addEventListener('load', ready);
    ready();
    connections.set(id, () => { controller.abort(); observer.disconnect(); window.removeEventListener('message', listener); frame.removeEventListener('load', ready); });
}
export function unmount(id) { connections.get(id)?.(); connections.delete(id); }
