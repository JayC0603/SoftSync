(() => {
    const root = document.documentElement;
    function syncSidebar() {
        document.querySelectorAll('.ss-sidebar-toggle').forEach(button => {
            button.setAttribute('aria-expanded', String(root.dataset.sidebar !== 'collapsed'));
        });
    }
    try { root.dataset.sidebar = localStorage.getItem('ss-sidebar') || 'expanded'; } catch {}
    window.ssDesign = {
        togglePassword(button) {
            const input = document.getElementById(button.getAttribute('aria-controls'));
            if (!input) return;
            const visible = input.type === 'password';
            input.type = visible ? 'text' : 'password';
            button.setAttribute('aria-pressed', String(visible));
            const icon = button.querySelector('i');
            if (icon) icon.className = visible ? 'bi bi-eye-slash' : 'bi bi-eye';
        },
        toggleSidebar() {
            root.dataset.sidebar = root.dataset.sidebar === 'collapsed' ? 'expanded' : 'collapsed';
            try { localStorage.setItem('ss-sidebar', root.dataset.sidebar); } catch {}
            syncSidebar();
        },
        toggleTheme() {
            const theme = root.dataset.theme === 'dark' ? 'light' : 'dark';
            root.dataset.theme = theme;
            try {
                localStorage.setItem('ss-theme', theme);
                document.cookie = `ss-theme=${theme}; path=/; max-age=31536000; samesite=lax`;
            } catch {}
        }
    };
    document.addEventListener('DOMContentLoaded', syncSidebar);
    document.addEventListener('enhancedload', syncSidebar);
    // Focus stays inside the existing mentor dialogs; Escape activates their
    // existing Blazor close action rather than changing component state in JS.
    document.addEventListener('DOMContentLoaded', () => {
        let dialog = null;
        let returnFocus = null;
        const focusable = node => [...node.querySelectorAll('button:not([disabled]), a[href], input:not([disabled]), select, textarea, [tabindex="0"]')].filter(item => item.getClientRects().length);
        const observeDialog = () => {
            const next = document.querySelector('.ss-mentor-modal[aria-modal="true"], .mentor-modal[aria-modal="true"]');
            if (next === dialog) return;
            if (next) {
                returnFocus = document.activeElement;
                dialog = next;
                (focusable(dialog)[0] || dialog).focus();
            } else {
                dialog = null;
                if (returnFocus?.isConnected) returnFocus.focus();
                returnFocus = null;
            }
        };
        new MutationObserver(observeDialog).observe(document.body, { childList: true, subtree: true });
        document.addEventListener('keydown', event => {
            if (!dialog) return;
            if (event.key === 'Escape') {
                const close = dialog.querySelector('.ss-mentor-modal-close, .mentor-modal-close');
                if (close) { event.preventDefault(); close.click(); }
            }
            if (event.key !== 'Tab') return;
            const items = focusable(dialog);
            if (!items.length) { event.preventDefault(); return; }
            const first = items[0], last = items[items.length - 1];
            if (event.shiftKey && (document.activeElement === first || !dialog.contains(document.activeElement))) { event.preventDefault(); last.focus(); }
            else if (!event.shiftKey && (document.activeElement === last || !dialog.contains(document.activeElement))) { event.preventDefault(); first.focus(); }
        });
    });
})();
