(function () {
    if (window.ssPageNavigationBound) return;
    window.ssPageNavigationBound = true;
    var pending = null;
    var stalledTimer = null;
    var links = '.ss-mobile-bottom-nav a, .ss-navpills a';
    function syncLinks() {
        var main = document.getElementById('main-content');
        if (!main) return;
        var path = main.dataset.pageRoute || location.pathname;
        document.querySelectorAll(links).forEach(function (link) {
            var href = new URL(link.href, location.href).pathname;
            var current = path === href || (href !== '/' && path.startsWith(href.replace(/\/$/, '') + '/'));
            if (!current && link.dataset.routeRoots) {
                current = link.dataset.routeRoots.split(' ').some(function (root) {
                    return path === root || path.startsWith(root + '/');
                });
            }
            if (link.classList.contains('active') !== current) link.classList.toggle('active', current);
            if (current && link.getAttribute('aria-current') !== 'page') link.setAttribute('aria-current', 'page');
            else if (!current && link.hasAttribute('aria-current')) link.removeAttribute('aria-current');
        });
    }
    // Interactive shell parameters can arrive a frame after the streamed body.
    // Reconcile the anchor state with the body before the browser paints it.
    new MutationObserver(function (records) {
        if (records.some(function (record) {
            return record.target.matches('#main-content, ' + links);
        })) reconcile();
    }).observe(document.body, { attributes: true, attributeFilter: ['data-page-route', 'class'], childList: true, subtree: true });
    syncLinks();
    // BFCache can restore the DOM while a previous navigation was pending.
    window.addEventListener('pageshow', clear);

    function clear() {
        clearTimeout(stalledTimer);
        stalledTimer = null;
        pending = null;
        document.documentElement.removeAttribute('data-navigation-pending');
        var main = document.getElementById('main-content');
        if (main) { main.removeAttribute('aria-busy'); main.inert = false; }
        var status = document.getElementById('ss-navigation-status');
        if (status) status.hidden = true;
        document.querySelectorAll('[data-navigation-pending]').forEach(function (link) {
            link.removeAttribute('data-navigation-pending');
        });
    }
    function reconcile() {
        syncLinks();
        var main = document.getElementById('main-content');
        if (!pending || !main) return;
        if (main.dataset.pageRoute === pending.path) clear();
    }
    document.addEventListener('click', function (event) {
        if (event.defaultPrevented || event.button !== 0 || event.ctrlKey || event.metaKey || event.shiftKey || event.altKey) return;
        var link = event.target.closest('a[href]');
        if (!link || link.hasAttribute('download') || (link.target && link.target !== '_self')) return;
        if (link.closest('[data-enhance-nav="false"]')) return;
        var url = new URL(link.href, location.href);
        var main = document.getElementById('main-content');
        if (!main || url.origin !== location.origin || !/^https?:$/.test(url.protocol)) return;
        if (url.pathname === location.pathname && url.search === location.search && url.hash) return;
        // Preserve native authentication, downloads and non-page endpoints.
        if (/^\/(Account|api|_framework|_blazor)(\/|$)/i.test(url.pathname)) return;
        // Re-selecting the current tab should not fetch and rebuild the page.
        if (url.pathname === main.dataset.pageRoute && url.search === location.search && !url.hash) {
            event.preventDefault();
            return;
        }
        clear();
        var request = { path: url.pathname };
        pending = request;
        document.documentElement.setAttribute('data-navigation-pending', 'true');
        main.setAttribute('aria-busy', 'true');
        link.setAttribute('data-navigation-pending', 'true');
        // Keep the shell usable. Normal route changes need no blocking banner.
        stalledTimer = setTimeout(function () {
            if (pending !== request) return;
            // Leave a native escape route if a request stalls or the network drops.
            if (main.isConnected) { main.inert = false; main.removeAttribute('aria-busy'); }
            var current = document.getElementById('ss-navigation-status');
            if (!current) return;
            current.hidden = false;
            current.dataset.loading = 'false';
            var english = document.documentElement.lang === 'en';
            current.querySelector('span').textContent = english ? 'This page is taking longer to load.' : 'Trang đang tải lâu hơn bình thường.';
            var retry = current.querySelector('a');
            retry.textContent = english ? 'Try again' : 'Thử lại';
            retry.href = url.href;
            retry.setAttribute('data-enhance-nav', 'false');
            retry.hidden = false;
        }, 15000);
    });
    window.ssBindPageNavigation = function () {
        Blazor.addEventListener('enhancedload', reconcile);
        Blazor.addEventListener('enhancednavigationend', function () {
            // Includes redirects to account pages and aborted navigations.
            clear();
        });
    };
})();
