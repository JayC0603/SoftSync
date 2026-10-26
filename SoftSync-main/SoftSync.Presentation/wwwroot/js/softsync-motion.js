(() => {
    if (window.ssMotion) return;
    const root = document.documentElement;
    const media = matchMedia('(prefers-reduced-motion: reduce)');
    let currentPath = '', currentSurface, animation;
    const reduced = () => media.matches || root.dataset.reduceMotion === '1';
    function enter() {
        const surface = document.querySelector('#main-content.ss-main-content, main.marketing, main, .neo-content');
        const path = location.pathname;
        if (!surface || (path === currentPath && surface === currentSurface)) return;
        currentPath = path;
        currentSurface = surface;
        animation?.cancel();
        if (reduced() || root.dataset.motionInput === 'keyboard' || !surface.animate) return;
        animation = surface.animate([
            { opacity: .6, transform: 'translateY(8px)' },
            { opacity: 1, transform: 'translateY(0)' }
        ], { duration: 240, easing: 'cubic-bezier(.23, 1, .32, 1)' });
    }
    window.ssMotion = { enter };
    document.addEventListener('keydown', () => { root.dataset.motionInput = 'keyboard'; animation?.cancel(); }, { passive: true });
    document.addEventListener('pointerdown', () => { root.dataset.motionInput = 'pointer'; }, { passive: true });
    media.addEventListener('change', () => { if (reduced()) animation?.cancel(); });
    new MutationObserver(() => { if (reduced()) animation?.cancel(); }).observe(root, { attributes: true, attributeFilter: ['data-reduce-motion'] });
    // Blazor navigation and DOM replacement; polling chart updates do not reanimate.
    let queued = false;
    function schedule() {
        if (queued) return;
        queued = true;
        requestAnimationFrame(() => { queued = false; enter(); });
    }
    new MutationObserver(schedule).observe(document.body, { childList: true, subtree: true });
    window.addEventListener('popstate', schedule);
    window.addEventListener('softsync:navigate', schedule);
    window.addEventListener('pageshow', schedule);
    if (window.Blazor?.addEventListener) window.Blazor.addEventListener('enhancedload', schedule);
    schedule();
})();
