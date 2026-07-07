// Bundle entry — imports the stylesheet so Vite emits dist/main.css,
// and wires the cursor-reactive liquid-glass highlight.
import '../Styles/main.css';

// Liquid Glass — cursor-reactive specular highlight.
// Elements with `.ss-glass-interactive` get their `--mx`/`--my` custom
// properties tracked to the pointer. Delegated + rAF-throttled so it stays
// cheap no matter how many glass surfaces exist.
(function () {
    if (window.__ssLiquidGlassBound) return;
    window.__ssLiquidGlassBound = true;

    if (window.matchMedia('(prefers-reduced-motion: reduce)').matches) return;

    let queued = false;
    let el = null;
    let px = 0;
    let py = 0;

    function apply() {
        queued = false;
        if (!el || !el.isConnected) return;
        const r = el.getBoundingClientRect();
        if (r.width === 0 || r.height === 0) return;
        el.style.setProperty('--mx', (((px - r.left) / r.width) * 100).toFixed(2) + '%');
        el.style.setProperty('--my', (((py - r.top) / r.height) * 100).toFixed(2) + '%');
    }

    document.addEventListener('pointermove', (e) => {
        const hit = e.target.closest && e.target.closest('.ss-glass-interactive');
        if (!hit) return;
        el = hit;
        px = e.clientX;
        py = e.clientY;
        if (!queued) {
            queued = true;
            requestAnimationFrame(apply);
        }
    }, { passive: true });

    document.addEventListener('pointerout', (e) => {
        const hit = e.target.closest && e.target.closest('.ss-glass-interactive');
        if (hit) {
            hit.style.setProperty('--mx', '50%');
            hit.style.setProperty('--my', '50%');
        }
    }, { passive: true });
})();

// Language preference persistence — called from Blazor via JS interop.
// Stored in localStorage so the choice survives reloads/new visits.
window.ssLang = {
    get() {
        try { return localStorage.getItem('ss-lang'); } catch { return null; }
    },
    set(code) {
        try { localStorage.setItem('ss-lang', code); } catch { /* ignore */ }
    }
};

// Welcome flag — set as a short-lived cookie by the server right after a
// successful login, read once by the WelcomeToast component, then cleared so
// the greeting animation shows exactly once per sign-in.
window.ssWelcome = {
    get() {
        try {
            const m = document.cookie.match(/(?:^|; )ss-welcome=([^;]*)/);
            return m ? decodeURIComponent(m[1]) : null;
        } catch { return null; }
    },
    clear() {
        try { document.cookie = 'ss-welcome=; Max-Age=0; path=/'; } catch { /* ignore */ }
    }
};

// Submit a plain HTML form by id (used to POST the logout form after the
// "Leave your learning journey?" modal is confirmed).
window.ssSubmitForm = function (id) {
    const f = document.getElementById(id);
    if (f) f.submit();
};
