(() => {
    // Enhanced navigation must never install another set of observers/timers.
    if (window.__ssDesignBound) return;
    window.__ssDesignBound = true;
    const root = document.documentElement;
    const oncePerFrame = callback => {
        let pending = false;
        return () => {
            if (pending) return;
            pending = true;
            requestAnimationFrame(() => { pending = false; callback(); });
        };
    };
    let sidebarExpanded = false;
    const syncSidebar = () => {
        const expanded = sidebarExpanded;
        const state = expanded ? 'expanded' : 'collapsed';
        if (document.documentElement.dataset.sidebar !== state)
            document.documentElement.dataset.sidebar = state;
        document.querySelectorAll('[data-sidebar-toggle]').forEach(button =>
            button.setAttribute('aria-expanded', String(expanded)));
    };
    // Keep the user's explicit choice across page loads and browser sessions.
    // Migrate the old tab-only preference without overriding a saved choice.
    const sidebarKey = 'ss-sidebar-expanded';
    try {
        const saved = localStorage.getItem(sidebarKey);
        sidebarExpanded = (saved ?? sessionStorage.getItem(sidebarKey)) === '1';
        if (saved === null) localStorage.setItem(sidebarKey, sidebarExpanded ? '1' : '0');
    } catch {
        try { sidebarExpanded = sessionStorage.getItem(sidebarKey) === '1'; } catch {}
    }
    syncSidebar();
    // Enhanced navigation can patch attributes on <html> with server defaults.
    // Only the toggle changes the preference; DOM updates must restore it.
    new MutationObserver(syncSidebar).observe(document.documentElement, {
        attributes: true, attributeFilter: ['data-sidebar']
    });
    window.ssDesign = {
        toggleSidebar() {
            sidebarExpanded = !sidebarExpanded;
            const value = sidebarExpanded ? '1' : '0';
            try { localStorage.setItem(sidebarKey, value); } catch {}
            try { sessionStorage.setItem(sidebarKey, value); } catch {}
            syncSidebar();
        },
        scrollMentors(button, direction) {
            const track = button.closest('section')?.querySelector('.ss-mentor-carousel');
            if (!track) return;
            track.dataset.resumeAt = String(Date.now() + 8000);
            const step = mentorStep(track);
            track.scrollBy({ left: direction * step, behavior: root.dataset.reduceMotion === '1' || matchMedia('(prefers-reduced-motion: reduce)').matches ? 'instant' : 'smooth' });
        },
        toggleMentorRotation(button) {
            const track = button.closest('section')?.querySelector('.ss-mentor-carousel');
            if (!track) return;
            const paused = track.dataset.rotationPaused !== 'true';
            track.dataset.rotationPaused = String(paused);
            button.setAttribute('aria-pressed', String(paused));
            button.textContent = paused ? button.dataset.resumeLabel : button.dataset.pauseLabel;
        },
        async toggleFullscreen() {
            try {
                if (document.fullscreenElement) await document.exitFullscreen();
                else await root.requestFullscreen();
            } catch { /* Fullscreen may be unavailable in embedded browsers. */ }
        },
        togglePassword(button) {
            const input = document.getElementById(button.getAttribute('aria-controls'));
            if (!input) return;
            const visible = input.type === 'password';
            input.type = visible ? 'text' : 'password';
            button.setAttribute('aria-pressed', String(visible));
            const icon = button.querySelector('i');
            if (icon) icon.className = visible ? 'bi bi-eye-slash' : 'bi bi-eye';
        },
        setAccountLanguage(code) {
            if (code !== 'vi' && code !== 'en') return;
            try { localStorage.setItem('ss-lang', code); } catch {}
            try { document.cookie = `ss-lang=${code}; path=/; max-age=31536000; samesite=lax`; } catch {}
            if (root.lang === code) return;
            const page = document.querySelector('#ss-auth-page.auth-login');
            const content = page?.querySelector('[data-account-translations]');
            if (content) {
                const translations = new Map();
                for (const pair of JSON.parse(content.textContent)) {
                    translations.set(pair[0], pair[code === 'vi' ? 0 : 1]);
                    translations.set(pair[1], pair[code === 'vi' ? 0 : 1]);
                }
                const translate = value => {
                    const text = value.trim();
                    const next = translations.get(text);
                    return next === undefined ? value : value.replace(text, next);
                };
                for (const scope of [page, document.querySelector('.ss-site-footer-shell')]) {
                    if (!scope) continue;
                    const walker = document.createTreeWalker(scope, NodeFilter.SHOW_TEXT);
                    while (walker.nextNode()) {
                        const node = walker.currentNode;
                        if (!node.parentElement.closest('script, style, textarea'))
                            node.nodeValue = translate(node.nodeValue);
                    }
                    for (const element of scope.querySelectorAll('[placeholder], [aria-label], [title], [alt]')) {
                        for (const name of ['placeholder', 'aria-label', 'title', 'alt']) {
                            if (element.hasAttribute(name))
                                element.setAttribute(name, translate(element.getAttribute(name)));
                        }
                    }
                }
                root.lang = code;
                document.title = translate(document.title);
                page.querySelectorAll('[data-account-language]').forEach(button =>
                    button.setAttribute('aria-pressed', String(button.dataset.accountLanguage === code)));
                return;
            }
            // Other static account forms still request their server-rendered language.
            window.location.reload();
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
    // Clear the retired forced-mobile preference so desktop returns to its natural layout.
    delete root.dataset.mobileView;
    try { localStorage.removeItem('ss-mobile-view'); } catch {}
    const reducedMotion = () => root.dataset.reduceMotion === '1' || matchMedia('(prefers-reduced-motion: reduce)').matches;
    const mentorStep = track => (track.firstElementChild?.getBoundingClientRect().width || track.clientWidth) + (parseFloat(getComputedStyle(track).columnGap) || 0);
    const mentorTracks = new Set();
    const observeMentors = () => document.querySelectorAll('.ss-mentor-carousel').forEach(track => {
        if (mentorTracks.has(track)) return;
        mentorTracks.add(track);
        const postpone = () => { track.dataset.resumeAt = String(Date.now() + 8000); };
        for (const event of (track.matches('.sh-mentor-grid') ? ['pointerdown', 'wheel', 'keydown', 'focusin'] : ['pointerdown', 'wheel', 'keydown', 'focusin', 'pointerleave', 'focusout']))
            track.addEventListener(event, postpone, { passive: true });
        let drag = null;
        let suppressClick = false;
        track.addEventListener('pointerdown', event => {
            if (event.pointerType !== 'mouse' || event.button !== 0 || event.target.closest('button, input, select')) return;
            drag = { id: event.pointerId, x: event.clientX, scroll: track.scrollLeft, moved: false };
            suppressClick = false;
        });
        track.addEventListener('pointermove', event => {
            if (!drag || event.pointerId !== drag.id) return;
            const distance = event.clientX - drag.x;
            if (!drag.moved && Math.abs(distance) < 6) return;
            drag.moved = true;
            track.setPointerCapture(event.pointerId);
            track.classList.add('is-dragging');
            track.scrollLeft = drag.scroll - distance;
            event.preventDefault();
            postpone();
        });
        const finishDrag = event => {
            if (!drag || drag.id !== event.pointerId) return;
            suppressClick = drag.moved;
            drag = null;
            track.classList.remove('is-dragging');
            if (track.hasPointerCapture(event.pointerId)) track.releasePointerCapture(event.pointerId);
            postpone();
            setTimeout(() => { suppressClick = false; }, 0);
        };
        for (const event of ['pointerup', 'pointercancel', 'lostpointercapture']) track.addEventListener(event, finishDrag);
        track.addEventListener('pointerleave', event => { if (drag && !drag.moved) finishDrag(event); });
        track.addEventListener('dragstart', event => event.preventDefault());
        track.addEventListener('click', event => {
            if (suppressClick) { event.preventDefault(); event.stopPropagation(); }
        }, true);
        track.addEventListener('keydown', event => {
            if (event.target !== track || !['ArrowLeft', 'ArrowRight'].includes(event.key)) return;
            event.preventDefault();
            const step = mentorStep(track);
            track.scrollBy({ left: event.key === 'ArrowRight' ? step : -step, behavior: reducedMotion() ? 'instant' : 'smooth' });
        });
    });
    document.addEventListener('DOMContentLoaded', () => {
        syncSidebar();
        observeMentors();
        const refreshMentors = oncePerFrame(observeMentors);
        new MutationObserver(records => {
            // Text updates, counters and chat streaming don't add carousels.
            if (records.some(record => [...record.addedNodes].some(node =>
                node instanceof Element && (node.matches('.ss-mentor-carousel') || node.querySelector('.ss-mentor-carousel')))))
                refreshMentors();
        }).observe(document.body, { childList: true, subtree: true });
    });
    document.addEventListener('enhancedload', observeMentors);
    document.addEventListener('enhancedload', syncSidebar);
    window.addEventListener('pageshow', syncSidebar);
    setInterval(() => {
        if (document.hidden || root.dataset.reduceMotion === '1' || matchMedia('(prefers-reduced-motion: reduce)').matches) return;

        for (const track of mentorTracks) {
            if (!track.isConnected) { mentorTracks.delete(track); continue; }
            if (innerWidth > 900 && !track.matches('.ss-home-mentor-carousel, .mentor-cards, .mh-mentor-track')) continue;
            const rect = track.getBoundingClientRect();
            const compactHome = track.matches('.sh-mentor-grid');
            const outside = compactHome ? rect.bottom <= 0 || rect.top >= innerHeight : rect.top < 0 || rect.top + Math.min(rect.height, 240) > innerHeight;
            if (outside || Number(track.dataset.resumeAt || 0) > Date.now() || track.dataset.rotationPaused === 'true' || track.matches(compactHome ? ':focus-within' : ':hover, :focus-within') || document.querySelector('dialog[open]')) continue;
            const max = track.scrollWidth - track.clientWidth;
            if (max <= 1) continue;
            const step = mentorStep(track);
            if (compactHome) {
                const atEnd = track.scrollLeft >= max - 2;
                track.scrollTo({ left: atEnd ? 0 : Math.min(max, track.scrollLeft + step), behavior: atEnd ? 'instant' : 'smooth' });
                continue;
            }
            track.scrollTo({ left: track.scrollLeft >= max - 2 ? 0 : Math.min(max, track.scrollLeft + step), behavior: 'smooth' });
        }
    }, 5500);
    document.addEventListener('fullscreenchange', () => document.querySelectorAll('.ss-header-fullscreen i').forEach(icon => {
        icon.className = document.fullscreenElement ? 'bi bi-fullscreen-exit' : 'bi bi-fullscreen';
    }));
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
        new MutationObserver(oncePerFrame(observeDialog)).observe(document.body, { childList: true, subtree: true });
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
