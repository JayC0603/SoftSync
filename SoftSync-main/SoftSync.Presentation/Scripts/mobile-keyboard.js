// WebViews can resize fixed navigation above the software keyboard. Keep it out
// of the typing area without overriding the browser's focus/scroll behavior.
(() => {
    const root = document.documentElement;
    const viewport = window.visualViewport;
    const touch = matchMedia('(any-pointer: coarse)');
    let restingHeight = window.innerHeight;
    let restingWidth = window.innerWidth;
    let frame = 0;
    const isEditing = () => {
        const element = document.activeElement;
        return element instanceof HTMLElement && (element.isContentEditable ||
            element.matches('textarea, input:not([type="button"]):not([type="submit"]):not([type="reset"]):not([type="checkbox"]):not([type="radio"]):not([type="range"]):not([type="color"]):not([type="file"]):not([type="hidden"])'));
    };
    const sync = () => {
        frame = 0;
        const editing = isEditing();
        // Rotation/window resizing must not be mistaken for a keyboard.
        if (Math.abs(window.innerWidth - restingWidth) > 80) {
            restingWidth = window.innerWidth;
            restingHeight = window.innerHeight;
        }
        const visibleHeight = viewport?.height ?? window.innerHeight;
        const zoomed = viewport && Math.abs(viewport.scale - 1) > 0.05;
        const keyboard = touch.matches && editing && !zoomed &&
            Math.max(restingHeight, window.innerHeight) - visibleHeight > 150;
        root.toggleAttribute('data-soft-keyboard', keyboard);
        if (!editing) restingHeight = window.innerHeight;
    };
    const schedule = () => { if (!frame) frame = requestAnimationFrame(sync); };
    document.addEventListener('focusin', schedule);
    document.addEventListener('focusout', schedule);
    window.addEventListener('resize', schedule, { passive: true });
    viewport?.addEventListener('resize', schedule, { passive: true });
    viewport?.addEventListener('scroll', schedule, { passive: true });
    window.addEventListener('pageshow', schedule);
    sync();
})();
