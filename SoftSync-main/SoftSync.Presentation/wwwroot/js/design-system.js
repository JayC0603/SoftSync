(() => {
    const root = document.documentElement;
    let footer = null;
    const measureFooter = () => {
        if (!footer?.isConnected) return;
        const layout = footer.parentElement;
        layout.style.setProperty('--ss-footer-height', `${Math.ceil(footer.getBoundingClientRect().height)}px`);
    };
    const footerObserver = new ResizeObserver(measureFooter);
    const connectFooter = () => {
        const next = document.querySelector('.ss-workspace > .eco-footer, .ss-public-site > .eco-footer');
        if (next === footer) return;
        footerObserver.disconnect();
        footer = next;
        if (footer) { footerObserver.observe(footer); measureFooter(); }
    };
    document.addEventListener('DOMContentLoaded', () => {
        connectFooter();
        new MutationObserver(connectFooter).observe(document.body, { childList: true, subtree: true });
    });
    document.addEventListener('enhancedload', connectFooter);
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
        toggleTheme() {
            const theme = root.dataset.theme === 'dark' ? 'light' : 'dark';
            root.dataset.theme = theme;
            try {
                localStorage.setItem('ss-theme', theme);
                document.cookie = `ss-theme=${theme}; path=/; max-age=31536000; samesite=lax`;
            } catch {}
        }
    };
    // Header disclosures use native keyboard behavior. Keep only one open and
    // dismiss on selection, outside click or Escape without replacing auth state.
    document.addEventListener('toggle', event => {
        if (!event.target.matches?.('.ss-header-menu') || !event.target.open) return;
        document.querySelectorAll('.ss-header-menu[open]').forEach(menu => {
            if (menu !== event.target) menu.open = false;
        });
    }, true);
    document.addEventListener('click', event => {
        const target = event.target instanceof Element ? event.target : null;
        document.querySelectorAll('.ss-header-menu[open]').forEach(menu => {
            if (!target || !menu.contains(target) || target.closest('.ss-header-dropdown a, .ss-header-dropdown button')) menu.open = false;
        });
    });
    document.addEventListener('keydown', event => {
        if (event.key !== 'Escape') return;
        const menu = document.querySelector('.ss-header-menu[open]');
        if (menu) { menu.open = false; menu.querySelector('summary')?.focus(); }
    });
    // Focus stays inside the existing mentor dialogs; Escape activates their
    // existing Blazor close action rather than changing component state in JS.
    document.addEventListener('DOMContentLoaded', () => {
        let dialog = null;
        let returnFocus = null;
        let savedBodyOverflow = null;
        const focusable = node => [...node.querySelectorAll('button:not([disabled]), a[href], input:not([disabled]), select, textarea, [tabindex="0"]')].filter(item => item.getClientRects().length);
        const observeDialog = () => {
            const next = document.querySelector('.ss-mentor-modal[aria-modal="true"], .mentor-modal[aria-modal="true"]');
            if (next === dialog) return;
            if (next) {
                returnFocus = document.activeElement;
                dialog = next;
                const overlay = dialog.closest('dialog');
                if (overlay && !overlay.open) overlay.showModal();
                if (savedBodyOverflow === null) savedBodyOverflow = document.body.style.overflow;
                document.body.style.overflow = 'hidden';
                (focusable(dialog)[0] || dialog).focus();
            } else {
                dialog = null;
                if (savedBodyOverflow !== null) document.body.style.overflow = savedBodyOverflow;
                savedBodyOverflow = null;
                if (returnFocus?.isConnected) returnFocus.focus();
                returnFocus = null;
            }
        };
        new MutationObserver(observeDialog).observe(document.body, { childList: true, subtree: true });
        observeDialog();
        document.addEventListener('cancel', event => {
            if (!event.target.matches?.('dialog.mentor-modal-backdrop, dialog.ss-mentor-modal-backdrop')) return;
            event.preventDefault();
            event.target.querySelector('.mentor-modal-close, .ss-mentor-modal-close')?.click();
        }, true);
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
