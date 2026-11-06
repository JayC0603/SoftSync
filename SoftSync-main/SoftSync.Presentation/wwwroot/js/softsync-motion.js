(() => {
    if (window.ssMotion) return;
    const root = document.documentElement;
    const media = matchMedia('(prefers-reduced-motion: reduce)');
    const storageKey = 'ss-motion-navigation';
    let currentRoute, currentSurface, animation, queued = false, bound = false;
    const reduced = () => media.matches || root.dataset.reduceMotion === '1';
    const cancel = () => { animation?.cancel(); animation = undefined; };

    // Carry keyboard intent across a full document navigation, without making
    // subsequent visits inherit a stale input preference.
    try {
        const intent = JSON.parse(sessionStorage.getItem(storageKey) || 'null');
        sessionStorage.removeItem(storageKey);
        if (intent && Date.now() - intent.at < 15000 && intent.href === location.href)
            root.dataset.motionInput = intent.input;
    } catch { /* Storage can be unavailable in private or restricted contexts. */ }

    function enter() {
        const surface = document.querySelector('#main-content, main.marketing, main, .neo-content');
        if (!surface) return;
        // Interactive routing may update the address before its new body. Wait
        // for MainLayout's route marker so the old screen never slides away.
        if (surface.dataset.pageRoute && surface.dataset.pageRoute !== location.pathname) return;
        const route = location.pathname + location.search;
        if (route === currentRoute && surface === currentSurface) return;
        currentRoute = route;
        currentSurface = surface;
        cancel();
        if (reduced() || root.dataset.motionInput === 'keyboard' || !surface.animate) return;
        const easing = getComputedStyle(root).getPropertyValue('--ease-out').trim() || 'cubic-bezier(.23, 1, .32, 1)';
        animation = surface.animate([
            { opacity: .82, transform: 'translateY(6px)' },
            { opacity: 1, transform: 'translateY(0)' }
        ], { duration: 200, easing });
        // No fill mode, overlays, hidden classes, or delayed navigation: content
        // stays usable even if navigation interrupts the animation.
    }
    function bindBlazor() {
        if (bound || !window.Blazor?.addEventListener) return;
        bound = true;
        window.Blazor.addEventListener('enhancedload', schedule);
        window.Blazor.addEventListener('enhancednavigationend', schedule);
    }
    function schedule() {
        bindBlazor();
        if (queued) return;
        queued = true;
        requestAnimationFrame(() => { queued = false; enter(); });
    }
    window.ssMotion = { enter: schedule };
    document.addEventListener('keydown', () => { root.dataset.motionInput = 'keyboard'; cancel(); }, { passive: true });
    document.addEventListener('pointermove', event => {
        if (event.pointerType === 'mouse' && (event.movementX || event.movementY)) root.dataset.motionInput = 'pointer';
    }, { passive: true });
    document.addEventListener('pointerdown', () => { root.dataset.motionInput = 'pointer'; }, { passive: true });
    document.addEventListener('click', event => {
        if (event.button !== 0 || event.ctrlKey || event.metaKey || event.altKey || event.shiftKey) return;
        const link = event.target.closest?.('a[href]');
        if (!link || link.hasAttribute('download') || (link.target && link.target !== '_self')) return;
        const url = new URL(link.href, location.href);
        if (url.origin !== location.origin) return;
        // detail=0 also covers assistive-technology activation.
        if (event.detail === 0) { root.dataset.motionInput = 'keyboard'; cancel(); }
        try { sessionStorage.setItem(storageKey, JSON.stringify({ href: url.href, input: root.dataset.motionInput || 'pointer', at: Date.now() })); } catch { }
    }, { capture: true, passive: true });
    media.addEventListener('change', () => { if (reduced()) cancel(); });
    new MutationObserver(() => { if (reduced()) cancel(); }).observe(root, { attributes: true, attributeFilter: ['data-reduce-motion'] });
    // Child updates cover interactive rendering; the route attribute covers a
    // reused shell. Same-route charts and streamed patches never restart motion.
    new MutationObserver(schedule).observe(document.body, { childList: true, subtree: true, attributes: true, attributeFilter: ['data-page-route'] });
    window.addEventListener('popstate', schedule);
    window.addEventListener('softsync:navigate', schedule);
    window.addEventListener('pageshow', schedule);
    window.addEventListener('pagehide', cancel);
    schedule();
})();
