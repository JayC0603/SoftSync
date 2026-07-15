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
        try { document.cookie = `ss-lang=${code}; path=/; max-age=31536000; samesite=lax`; }
        catch { /* ignore */ }
        try { document.documentElement.lang = code; } catch { /* ignore */ }
    }
};

// Keep AI chat usable across refreshes even when the remote database is
// temporarily unavailable. PostgreSQL remains the primary history store.
window.ssChatHistory = {
    key(userId, sessionId) { return `ss-chat-history-${userId}-${sessionId || 'draft'}`; },
    load(userId, sessionId) {
        try {
            const raw = localStorage.getItem(this.key(userId, sessionId));
            return raw ? JSON.parse(raw) : [];
        } catch { return []; }
    },
    save(userId, sessionId, messages) {
        try { localStorage.setItem(this.key(userId, sessionId), JSON.stringify(messages)); }
        catch { /* ignore storage quota/privacy-mode failures */ }
    },
    scrollTo(elementId) {
        const element = document.getElementById(elementId);
        if (element) element.scrollIntoView({ behavior: 'smooth', block: 'center' });
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

// Preserve an unfinished onboarding wizard across refreshes in this tab.
window.ssWizard = {
    key(userId) { return `ss-wizard-${userId}`; },
    get(userId) {
        try {
            const raw = sessionStorage.getItem(this.key(userId));
            return raw ? JSON.parse(raw) : null;
        } catch { return null; }
    },
    set(userId, state) {
        try { sessionStorage.setItem(this.key(userId), JSON.stringify(state)); }
        catch { /* ignore */ }
    },
    clear(userId) {
        try { sessionStorage.removeItem(this.key(userId)); }
        catch { /* ignore */ }
    }
};

// Keep an unfinished entrance assessment stable across refreshes. The draft is
// scoped to the signed-in user and contains only question/option ids, never
// question text or scores.
window.ssAssessment = {
    key(userId) { return `ss-assessment-${userId}`; },
    get(userId) {
        try {
            const raw = localStorage.getItem(this.key(userId));
            return raw ? JSON.parse(raw) : null;
        } catch { return null; }
    },
    set(userId, state) {
        try { localStorage.setItem(this.key(userId), JSON.stringify(state)); }
        catch { /* ignore */ }
    },
    clear(userId) {
        try { localStorage.removeItem(this.key(userId)); }
        catch { /* ignore */ }
    }
};

// Preserve an in-progress roadmap mini quiz across refreshes. Submitted
// attempts are still persisted by the server; this only keeps the unfinished
// five-question attempt in the current browser.
window.ssRoadmapQuiz = {
    key(userId, itemId) { return `ss-roadmap-quiz-${userId}-${itemId}`; },
    get(userId, itemId) {
        try {
            const raw = localStorage.getItem(this.key(userId, itemId));
            return raw ? JSON.parse(raw) : null;
        } catch { return null; }
    },
    set(userId, itemId, state) {
        try { localStorage.setItem(this.key(userId, itemId), JSON.stringify(state)); }
        catch { /* ignore */ }
    },
    clear(userId, itemId) {
        try { localStorage.removeItem(this.key(userId, itemId)); }
        catch { /* ignore */ }
    }
};

// Submit a plain HTML form by id (used to POST the logout form after the
// "Leave your learning journey?" modal is confirmed).
window.ssSubmitForm = function (id) {
    const f = document.getElementById(id);
    if (f) f.submit();
};

// Theme + accessibility preferences. Persisted in localStorage and applied to
// <html> as data-theme / data-reduce-motion so the CSS in main.css can react.
// apply() runs immediately (see the inline bootstrap in App.razor) to avoid a
// flash of the wrong theme before Blazor starts.
window.ssTheme = {
    // pref: "light" | "dark" | "system"
    resolve(pref) {
        if (pref === 'light' || pref === 'dark') return pref;
        try {
            return window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light';
        } catch { return 'light'; }
    },
    // Mirror the RESOLVED theme into a cookie so the server can render
    // data-theme on <html> during SSR / enhanced navigation (no flash / revert).
    _cookie(name, value) {
        try { document.cookie = name + '=' + value + '; path=/; max-age=31536000; samesite=lax'; }
        catch { /* ignore */ }
    },
    apply(pref, reduceMotion) {
        try {
            const el = document.documentElement;
            const resolved = this.resolve(pref || 'light');
            el.setAttribute('data-theme', resolved);
            if (reduceMotion) el.setAttribute('data-reduce-motion', '1');
            else el.removeAttribute('data-reduce-motion');
            this._cookie('ss-theme', resolved);
            this._cookie('ss-reduce-motion', reduceMotion ? '1' : '0');
        } catch { /* ignore */ }
    },
    set(pref, reduceMotion) {
        try {
            localStorage.setItem('ss-theme', pref);
            localStorage.setItem('ss-reduce-motion', reduceMotion ? '1' : '0');
        } catch { /* ignore */ }
        this.apply(pref, reduceMotion);
    },
    // Re-apply from storage; called on startup by the App.razor bootstrap.
    init() {
        let pref = 'light', rm = false;
        try {
            pref = localStorage.getItem('ss-theme') || 'light';
            rm = localStorage.getItem('ss-reduce-motion') === '1';
        } catch { /* ignore */ }
        this.apply(pref, rm);
    },
    // Seed the client from the server-stored preference (DB) the first time a
    // browser is used — but never override a choice the user already made here.
    syncFromServer(pref, reduceMotion) {
        try {
            if (localStorage.getItem('ss-theme')) return;
        } catch { /* ignore */ }
        this.set(pref || 'light', !!reduceMotion);
    }
};

// Apply saved theme as early as the bundle runs.
try { window.ssTheme.init(); } catch { /* ignore */ }

// Accessible text sizing. Stored per browser and applied to the root so all
// rem-based screens (including Roadmap and Settings) update immediately.
window.ssFontSize = {
    apply(scale) {
        const value = scale === 'small' ? '93.75%' : scale === 'large' ? '112.5%' : '100%';
        document.documentElement.style.fontSize = value;
        document.documentElement.setAttribute('data-font-size', scale || 'normal');
    },
    set(scale) {
        try { localStorage.setItem('ss-font-size', scale); } catch { /* ignore */ }
        this.apply(scale);
    },
    init() {
        let scale = 'normal';
        try { scale = localStorage.getItem('ss-font-size') || 'normal'; } catch { /* ignore */ }
        this.apply(scale);
        return scale;
    }
};
try { window.ssFontSize.init(); } catch { /* ignore */ }
